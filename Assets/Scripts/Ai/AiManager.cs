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

    // 輕量計畫紀錄:只存「動作序列 + 啟發分 + 是否封頂」,完全不持有盤面 clone。
    // beam 與 allPlans 都用它;要盤面時才從 root replay 重建(見 ReconstructState),
    // 讓同一時間最多只有 1~2 份盤面 clone 存活,WebGL 記憶體友善。
    private class PlanRecord
    {
        public List<AiOption> actions;
        public float heuristic;
        public bool terminal;

        public PlanRecord(List<AiOption> actions, float heuristic, bool terminal)
        {
            this.actions = actions;
            this.heuristic = heuristic;
            this.terminal = terminal;
        }
    }

    // 從 root 重放動作序列,重建出該計畫的盤面(用完即丟)。
    // executeVirtually 為確定性(隨機效果一律封頂、不會被展開),所以重放結果與搜尋當下一致。
    private static (BoardState, PlayerData, PlayerData) ReconstructState(
        BoardState rootState, PlayerData rootAlly, PlayerData rootEnemy, List<AiOption> actions, int turn)
    {
        BoardState b = rootState.Clone();
        PlayerData a = rootAlly.Clone();
        PlayerData e = rootEnemy.Clone();
        foreach (var act in actions)
            act.executeVirtually(b, a, e, turn);
        return (b, a, e);
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

        float rootHeuristic = AiEvaluator.EvaluateBoardState(rootState, rootAlly, rootEnemy, currentTurnCount, !rootAlly.IsWhite(), w);
        PlanRecord start = new PlanRecord(new List<AiOption>(), rootHeuristic, false);

        // beam 與 allPlans 都只存輕量 PlanRecord(動作序列),不持有盤面 clone。
        List<PlanRecord> beam = new List<PlanRecord> { start };
        List<PlanRecord> allPlans = new List<PlanRecord> { start };

        for (int depth = 0; depth < MAX_PLAN_DEPTH; depth++)
        {
            List<PlanRecord> candidates = new List<PlanRecord>();

            foreach (var node in beam)
            {
                if (node.terminal) continue;

                // ★ 只在展開此節點時才從 root 重建盤面,展開完即釋放(同時最多 1~2 份盤面活著)
                var (nodeBoard, nodeAlly, nodeEnemy) = ReconstructState(rootState, rootAlly, rootEnemy, node.actions, currentTurnCount);
                List<AiOption> legal = GenerateLegalVirtualActions(nodeBoard, nodeAlly, currentTurnCount);

                foreach (var action in legal)
                {
                    if (action is CardHoldOption) continue;

                    BoardState nb = nodeBoard.Clone();
                    PlayerData na = nodeAlly.Clone();
                    PlayerData ne = nodeEnemy.Clone();
                    action.executeVirtually(nb, na, ne, currentTurnCount);

                    List<AiOption> acts = new List<AiOption>(node.actions) { action };
                    // 啟發分加入「個性的用卡偏好」,避免奇招的鋪牌分支被提早剪掉
                    float heuristic = AiEvaluator.EvaluateBoardState(nb, na, ne, currentTurnCount, !na.IsWhite(), w)
                                    + w.cardPreference * CountCardUses(acts);

                    PlanRecord child = new PlanRecord(acts, heuristic, action.isRandomEffect());
                    candidates.Add(child);
                    allPlans.Add(child);
                    // nb/na/ne 在此迴圈結束即無引用 → 可回收(不長期持有)

                    // 時間切片:本幀算超過預算就讓出一幀,避免單執行緒卡頓(WebGL 必須,桌面也順帶受惠)
                    if (sw.Elapsed.TotalMilliseconds > FRAME_BUDGET_MS)
                    {
                        await Awaitable.NextFrameAsync();
                        sw.Restart();
                    }
                }
            }

            if (candidates.Count == 0) break;
            candidates.Sort((x, y) => y.heuristic.CompareTo(x.heuristic));
            beam = candidates.GetRange(0, Mathf.Min(BEAM_WIDTH, candidates.Count));
        }

        // 取前 N 條做「對手回應」精算 + 個性偏好 + 難度雜訊
        allPlans.Sort((x, y) => y.heuristic.CompareTo(x.heuristic));
        int take = Mathf.Min(FINAL_CANDIDATES, allPlans.Count);

        float baseScore = EvaluateWithOpponentReply(rootState, rootAlly, rootEnemy, currentTurnCount, w); // 基準不加雜訊
        List<AiOption> bestActions = start.actions; // 預設:不動(空計畫)
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < take; i++)
        {
            PlanRecord plan = allPlans[i];

            // 從 root 重建該計畫盤面(只對前 N 名做),用記憶體換 CPU
            var (pb, pa, pe) = ReconstructState(rootState, rootAlly, rootEnemy, plan.actions, currentTurnCount);

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

        // LogPlan(bestActions, bestScore, baseScore);

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

    private static void LogPlan(List<AiOption> actions, float bestScore, float baseScore)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"[Beam AI] 計畫分 {bestScore:F0} (停手基準 {baseScore:F0}) | 共 {actions.Count} 步: ");
        foreach (var a in actions)
        {
            if (a is CardUseOption c) sb.Append($"[卡:{c.getCardname()}] ");
            else if (a is ChessMoveOption m) sb.Append($"[移:{m.getChessName()}→{m.getTargetPosition()}] ");
            else if (a is CardHoldOption) sb.Append("[保留] ");
        }
        if (actions.Count == 0) sb.Append("(直接結束回合)");
        UnityEngine.Debug.Log(sb.ToString());
    }
}