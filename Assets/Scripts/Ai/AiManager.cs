using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    private static AiManager instance;
    private static readonly int OPTION_BUFFER_TIME_MILLIS = 1000;

    private const int BEAM_WIDTH = 6;
    private const int MAX_PLAN_DEPTH = 8;        // 大軍壓境等群體移動需要較深,調到 8
    private const int FINAL_CANDIDATES = 25;
    private const float SCORE_IMPROVE_EPS = 0.2f;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static PlayerData initAiPlayerData(bool isLocal, bool isWhite, AiPersonality personality, AiDifficulty difficulty)
    {
        AiProfile aiProfile = new AiProfile(personality, difficulty);
        return new PlayerData(isLocal, isWhite, aiProfile.getDefaultDeckList(), true, aiProfile);
    }

    private class PlanState
    {
        public BoardState board;
        public PlayerData ally, enemy;
        public List<AiOption> actions;
        public bool terminal;
        public float heuristic;
    }

    // 輕量計畫紀錄:只存「動作序列 + 啟發分」,不持有盤面 clone。
    // 用於 allPlans 累積,最後精算時再從 root replay 重建盤面,避免大量盤面 clone 撐爆記憶體。
    private class PlanRecord
    {
        public List<AiOption> actions;
        public float heuristic;

        public PlanRecord(List<AiOption> actions, float heuristic)
        {
            this.actions = actions;
            this.heuristic = heuristic;
        }
    }

    public static async Task<List<AiOption>> getBestTurnPlanAsync(bool isLocal, int currentTurnCount)
    {
        BoardState rootState = BoardManager.getCloneBoardState();
        PlayerData rootAlly = MatchDataManager.getPlayerData(isLocal).Clone();
        PlayerData rootEnemy = MatchDataManager.getPlayerData(!isLocal).Clone();

        // 取出這個 AI 的個性 / 難度
        AiProfile profile = rootAlly.getAiProfile();
        AiWeights w = profile.getWeights();
        float noise = profile.getNoiseRange();

        // 全平台都在主執行緒上跑,靠「時間切片」避免卡頓(見 RunBeamSearch),
        // 因此不需要 Task.Run / 背景執行緒,WebGL 與桌面共用同一條路。
        return await RunBeamSearch(rootState, rootAlly, rootEnemy, currentTurnCount, w, noise);
    }

    // 每幀最多花在 AI 運算上的時間(毫秒)。超過就讓出一幀讓畫面更新,維持流暢。
    private const double FRAME_BUDGET_MS = 8.0;

    private static async Task<List<AiOption>> RunBeamSearch(BoardState rootState, PlayerData rootAlly, PlayerData rootEnemy, int currentTurnCount, AiWeights w, float noise)
    {
        System.Random rng = new System.Random(); // 主執行緒,但仍用 System.Random 保持與盤面評估無關
        var sw = System.Diagnostics.Stopwatch.StartNew();

        PlanState start = new PlanState
        {
            board = rootState,
            ally = rootAlly,
            enemy = rootEnemy,
            actions = new List<AiOption>(),
            terminal = false,
            heuristic = AiEvaluator.EvaluateBoardState(rootState, rootAlly, rootEnemy, currentTurnCount, !rootAlly.IsWhite(), w)
        };

        List<PlanState> beam = new List<PlanState> { start };
        // allPlans 只保存「動作序列 + 啟發分」(輕量),不持有盤面 clone,避免節點累積把記憶體撐爆。
        List<PlanRecord> allPlans = new List<PlanRecord> { new PlanRecord(start.actions, start.heuristic) };

        for (int depth = 0; depth < MAX_PLAN_DEPTH; depth++)
        {
            List<PlanState> candidates = new List<PlanState>();

            foreach (var node in beam)
            {
                if (node.terminal) continue;
                List<AiOption> legal = GenerateLegalVirtualActions(node.board, node.ally, currentTurnCount);

                foreach (var action in legal)
                {
                    if (action is CardHoldOption) continue;

                    BoardState nb = node.board.Clone();
                    PlayerData na = node.ally.Clone();
                    PlayerData ne = node.enemy.Clone();
                    action.executeVirtually(nb, na, ne, currentTurnCount);

                    List<AiOption> acts = new List<AiOption>(node.actions) { action };
                    // 啟發分加入「個性的用卡偏好」,避免奇招的鋪牌分支被提早剪掉
                    float heuristic = AiEvaluator.EvaluateBoardState(nb, na, ne, currentTurnCount, !na.IsWhite(), w)
                                    + w.cardPreference * CountCardUses(acts);

                    PlanState child = new PlanState
                    {
                        board = nb,
                        ally = na,
                        enemy = ne,
                        actions = acts,
                        terminal = action.isRandomEffect(),
                        heuristic = heuristic
                    };

                    candidates.Add(child);                         // 短命:本層挑完下一輪 beam 後即可回收
                    allPlans.Add(new PlanRecord(acts, heuristic));  // 只留輕量紀錄,不持有盤面

                    // 時間切片:本幀算超過預算就讓出一幀,避免單執行緒卡頓(WebGL 必須,桌面也順帶受惠)
                    if (sw.Elapsed.TotalMilliseconds > FRAME_BUDGET_MS)
                    {
                        await Awaitable.NextFrameAsync();
                        sw.Restart();
                    }
                }
            }

            if (candidates.Count == 0) break;
            candidates.Sort((a, b) => b.heuristic.CompareTo(a.heuristic));
            beam = candidates.GetRange(0, Mathf.Min(BEAM_WIDTH, candidates.Count));
        }

        // 取前 N 條做「對手回應」精算 + 個性偏好 + 難度雜訊
        allPlans.Sort((a, b) => b.heuristic.CompareTo(a.heuristic));
        int take = Mathf.Min(FINAL_CANDIDATES, allPlans.Count);

        float baseScore = EvaluateWithOpponentReply(rootState, rootAlly, rootEnemy, currentTurnCount, w); // 基準不加雜訊
        List<AiOption> bestActions = start.actions; // 預設:不動(空計畫)
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < take; i++)
        {
            PlanRecord plan = allPlans[i];

            // 從 root 重新 replay 出該計畫的盤面(只對前 N 名做)。executeVirtually 為確定性,
            // 重建結果與搜尋當下一致,因此可安全地用「記憶體換 CPU」。
            BoardState pb = rootState.Clone();
            PlayerData pa = rootAlly.Clone();
            PlayerData pe = rootEnemy.Clone();
            foreach (var act in plan.actions)
                act.executeVirtually(pb, pa, pe, currentTurnCount);

            float score = EvaluateWithOpponentReply(pb, pa, pe, currentTurnCount, w)
                        + w.cardPreference * CountCardUses(plan.actions)
                        + (float)(rng.NextDouble() * 2.0 - 1.0) * noise; // ★ 難度雜訊
            if (score > bestScore) { bestScore = score; bestActions = plan.actions; }

            // 時間切片:精算階段同樣讓幀
            if (sw.Elapsed.TotalMilliseconds > FRAME_BUDGET_MS)
            {
                await Awaitable.NextFrameAsync();
                sw.Restart();
            }
        }

        if (bestActions.Count == 0 || bestScore < baseScore + SCORE_IMPROVE_EPS)
        {
            AiOption hold = DecideHoldOrEnd(rootAlly);
            return hold != null ? new List<AiOption> { hold } : new List<AiOption>();
        }
        return bestActions;
    }

    // 對手最佳「單一步」(吃子或佈置威脅),取最傷我方的結果
    private static float EvaluateWithOpponentReply(BoardState state, PlayerData ally, PlayerData enemy, int currentTurnCount, AiWeights w)
    {
        bool myWhite = ally.IsWhite();
        int et = currentTurnCount + 1;
        float worst = AiEvaluator.EvaluateBoardState(state, ally, enemy, currentTurnCount, !myWhite, w);

        List<MoveAction> buf = new List<MoveAction>(28);
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                ChessPiece p = state.getChessPieceAtPosition(x, y);
                if (p == null || p.isWhite() == myWhite || !p.canMove(et)) continue;

                buf.Clear();
                p.getMovementType(et).getAvailableMoves(state, p.getPosition(), p.isWhite(), buf);

                foreach (var m in buf)
                {
                    if (m.actionType == ActionType.Attack)
                    {
                        ChessPiece target = state.getChessPieceAtPosition(m.targetPos);
                        if (target == null || !target.canBeAttacked(et) || !p.canAttack(et)) continue;
                    }
                    BoardState nb = state.Clone();
                    nb.moveChessPiece(p.getPosition(), m.targetPos);
                    float s = AiEvaluator.EvaluateBoardState(nb, ally, enemy, et, myWhite, w);
                    if (s < worst) worst = s;
                }
            }
        return worst;
    }

    private static int CountCardUses(List<AiOption> actions)
    {
        int c = 0;
        foreach (var a in actions) if (a is CardUseOption) c++;
        return c;
    }

    private static AiOption DecideHoldOrEnd(PlayerData ally)
    {
        if (!ally.canUseHoldAction()) return null;
        Card bestCard = null; int bestCost = int.MinValue;
        foreach (Card c in ally.getHandList())
            if (c.getCost() > bestCost) { bestCost = c.getCost(); bestCard = c; }
        if (bestCard != null && bestCost >= 2) return new CardHoldOption(bestCard);
        return null;
    }

    private static List<AiOption> GenerateLegalVirtualActions(BoardState state, PlayerData ally, int currentTurnCount)
    {
        List<AiOption> options = new List<AiOption>();
        bool isWhite = ally.IsWhite();
        int currentMana = ally.getMana();

        foreach (Card card in ally.getHandList())
        {
            if (ally.getStatusContainer().canUseCard(card.getId(), currentTurnCount))
            {
                int finalCost = ally.isCardNonCost(currentTurnCount) ? 0 : card.getCost() + ally.getExtraCardCost(currentTurnCount);
                if (currentMana >= finalCost)
                {
                    CardEffect cardEffect = CardEffectManager.getCardEffect(card);
                    if (cardEffect != null)
                        foreach (var opt in cardEffect.getUseOptions(state, ally.IsLocal(), card, currentMana, currentTurnCount))
                            options.Add(opt);
                }
            }
        }

        List<MoveAction> moveBuf = new List<MoveAction>(28);
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece == null || piece.isWhite() != isWhite) continue;
                ActionValidator.getLegalMoves(state, ally, piece, currentTurnCount, moveBuf);
                foreach (var move in moveBuf)
                    options.Add(new ChessMoveOption(piece, piece.getPosition(), move.targetPos));
            }

        if (ally.canUseHoldAction())
            foreach (Card card in ally.getHandList()) options.Add(new CardHoldOption(card));

        return options;
    }

    public static async Task doBestMove(bool isLocal, int currentTurnCount)
    {
        try
        {
            // 隨機卡執行後手牌才真正確定 → 打到隨機卡就停,用新手牌重算剩下的回合
            int safety = 0;
            bool replan = true;

            while (replan && !GameManager.IsGameOver() && safety++ < 16)
            {
                replan = false;
                List<AiOption> plan = await getBestTurnPlanAsync(isLocal, currentTurnCount);

                if (plan.Count == 0)
                    break;

                foreach (var action in plan)
                {
                    if (GameManager.IsGameOver())
                        break;

                    while (HandViewManager.isPlayingAnimation())
                        await Awaitable.WaitForSecondsAsync(0.1f);

                    await Awaitable.WaitForSecondsAsync(OPTION_BUFFER_TIME_MILLIS / 1000f);
                    await action.execute(isLocal);

                    if (action.isRandomEffect())
                    {
                        replan = true;
                        break;              // 隨機結算 → 重新規劃
                    }

                    if (action is CardHoldOption)
                        break;              // 保留 → 回合結束
                }
            }

            while (HandViewManager.isPlayingAnimation() || BoardManager.isPlayingAnimation())
                await Awaitable.WaitForSecondsAsync(0.1f);

            if (!GameManager.IsGameOver()) GameManager.ChangeTurn();
        }
        catch (System.OperationCanceledException)
        {
            // 退出 Play / 物件被銷毀時,Awaitable.NextFrameAsync 會丟取消例外,屬正常中止,靜默忽略。
        }
        catch (System.Exception e)
        {
            // doBestMove 是 fire-and-forget(GameManager 用 _ = 呼叫),例外不抓會被默默吞掉,
            // 導致 AI 靜靜地不動。這裡明確印出來,WebGL 也能在瀏覽器 console 看到真正原因。
            Debug.LogError($"[AI] doBestMove 發生例外: {e}");
        }
    }

    private static void LogPlan(PlanState best, float bestScore, float baseScore)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"[Beam AI] 計畫分 {bestScore:F0} (停手基準 {baseScore:F0}) | 共 {best.actions.Count} 步: ");
        foreach (var a in best.actions)
        {
            if (a is CardUseOption c) sb.Append($"[卡:{c.getCardname()}] ");
            else if (a is ChessMoveOption m) sb.Append($"[移:{m.getChessName()}→{m.getTargetPosition()}] ");
            else if (a is CardHoldOption) sb.Append("[保留] ");
        }
        if (best.actions.Count == 0) sb.Append("(直接結束回合)");
        UnityEngine.Debug.Log(sb.ToString());
    }
}