using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AiPlan
{
    private AiOption firstRealOption; // 要回傳給真實遊戲執行的那一步
    private float finalScore;

    public AiPlan(AiOption firstOption, float finalScore)
    {
        this.firstRealOption = firstOption;
        this.finalScore = finalScore;
    }

    public AiOption getOption() => firstRealOption;

    public float getScore() => finalScore;
}

public class AiManager : MonoBehaviour
{
    private static AiManager instance;
    private static readonly int OPTION_BUFFER_TIME_MILLIS = 1000;

    // ==========================================
    // 演算法控制參數
    // ==========================================
    private const int BEAM_WIDTH = 4; // 束搜尋寬度：每層只取前 4 個最高分的動作深搜，防組合爆炸
    private const int MAX_DEPTH = 5;  // 最大推演深度 (一回合內的連擊次數上限)

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public static PlayerData initAiPlayerData(bool isLocal, bool isWhite, AiPersonality personality, AiDifficulty difficulty)
    {
        AiProfile aiProfile = new AiProfile(personality, difficulty);
        PlayerData aiPlayerData = new PlayerData(isLocal, isWhite, aiProfile.getDefaultDeckList(), true, aiProfile);

        return aiPlayerData;
    }

    public static AiOption getBestMove(bool isLocal, int currentTurnCount)
    {
        // 1. 取得當前真實盤面與玩家資料的「純資料深度快照」
        BoardState currentState = BoardManager.getCloneBoardState();
        PlayerData currentAlly = MatchDataManager.getPlayerData(isLocal).Clone();
        PlayerData currentEnemy = MatchDataManager.getPlayerData(!isLocal).Clone();

        // 建立 Hash 表，用於記錄推演過的盤面，防止重複計算 (例如：先走A再打B = 先打B再走A)
        HashSet<string> visitedStates = new HashSet<string>();

        // 2. 啟動 DFS 虛擬推演
        AiPlan bestPlan = RunDFS(currentState, currentAlly, currentEnemy, currentTurnCount, 0, visitedStates, null);

        // 如果 DFS 找不到任何有意義的路徑(或被防呆機制全部淘汰)，回傳 null 結束回合
        if (bestPlan == null || bestPlan.getScore() <= -90000f || bestPlan.getOption() == null)
            return null;

        // 3. 抓出這條黃金路徑的「第一步」，並加上個性的難度雜訊
        AiOption bestAction = bestPlan.getOption();
        float noise = Random.Range(-currentAlly.getAiProfile().getNoiseRange(), currentAlly.getAiProfile().getNoiseRange());
        bestAction.setScore(bestPlan.getScore() + noise);

        return bestAction;
    }

    /// <summary>
    /// 核心 DFS 遞迴搜尋與 Beam Search 快篩
    /// </summary>
    private static AiPlan RunDFS(BoardState state, PlayerData ally, PlayerData enemy, int currentTurnCount, int depth, HashSet<string> visitedStates, AiOption firstActionOfThisPath)
    {
        // 1. 終止條件：達到深度限制，或是遊戲在虛擬推演中已分出勝負 (國王死亡)
        if (depth >= MAX_DEPTH || state.getKing(true) == null || state.getKing(false) == null)
        {
            float score = AiEvaluator.EvaluateBoardState(state, ally, enemy, currentTurnCount);
            return new AiPlan(firstActionOfThisPath, score);
        }

        // 2. 收集當前虛擬狀態下的「所有合法動作」
        List<AiOption> candidates = GenerateLegalVirtualActions(state, ally, currentTurnCount);

        // 終止條件：已經沒有資源/步數可以做任何動作
        if (candidates.Count == 0)
        {
            float score = AiEvaluator.EvaluateBoardState(state, ally, enemy, currentTurnCount);
            return new AiPlan(firstActionOfThisPath, score);
        }

        // 3. 束搜尋快篩 (Beam Search)：每一步都稍微推演一下，然後評分淘汰
        foreach (var candidate in candidates)
        {
            BoardState testState = state.Clone();
            PlayerData testAlly = ally.Clone();
            PlayerData testEnemy = enemy.Clone();

            candidate.executeVirtually(testState, testAlly, testEnemy, currentTurnCount);
            float quickScore = AiEvaluator.EvaluateBoardState(testState, testAlly, testEnemy, currentTurnCount);
            candidate.setQuickScore(quickScore);
        }

        // 淘汰低分爛路，只保留分數最高的前 N 個分支
        var topCandidates = candidates.OrderByDescending(c => c.getQuickScore()).Take(BEAM_WIDTH).ToList();

        AiPlan bestPlan = new AiPlan(firstActionOfThisPath, float.MinValue);

        // 4. 針對留下來的精英路徑，正式進入深層遞迴
        foreach (var candidate in topCandidates)
        {
            BoardState nextState = state.Clone();
            PlayerData nextAlly = ally.Clone();
            PlayerData nextEnemy = enemy.Clone();

            // 執行虛擬動作
            candidate.executeVirtually(nextState, nextAlly, nextEnemy, currentTurnCount);

            // 狀態去重 (Hash Check)
            string stateHash = GenerateStateHash(nextState, nextAlly);
            if (visitedStates.Contains(stateHash)) continue;
            visitedStates.Add(stateHash);

            // 紀錄這條遞迴路徑的「第一步」 (用來回傳給畫面的真實 Option)
            AiOption rootAction = (depth == 0) ? candidate : firstActionOfThisPath;

            // ★ 隨機性中斷判定 (如抽牌) / 保留動作中斷判定
            if (candidate.isRandomEffect() || candidate.isHoldAction())
            {
                float score = AiEvaluator.EvaluateBoardState(nextState, nextAlly, nextEnemy, currentTurnCount);
                if (score > bestPlan.getScore())
                    bestPlan = new AiPlan(rootAction, score);
                continue; // 遇到這兩種卡，直接中斷該條路徑的探勘，保留當下最高分
            }

            // 遞迴往下挖下一層
            AiPlan subPlan = RunDFS(nextState, nextAlly, nextEnemy, currentTurnCount, depth + 1, visitedStates, rootAction);

            // 如果子路徑的總分比目前找到的還高，則替換掉最佳解
            if (subPlan.getScore() > bestPlan.getScore())
            {
                bestPlan = subPlan;
            }
        }

        // 如果迴圈跑完沒有任何有效路徑 (例如全被 visitedStates 剪枝)，直接結算當下盤面分數
        if (bestPlan.getScore() == float.MinValue)
        {
            float fallbackScore = AiEvaluator.EvaluateBoardState(state, ally, enemy, currentTurnCount);
            return new AiPlan(firstActionOfThisPath, fallbackScore);
        }

        return bestPlan;
    }

    /// <summary>
    /// 輕量化狀態雜湊：壓縮棋盤與資源
    /// </summary>
    private static string GenerateStateHash(BoardState state, PlayerData ally)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ally.getMana()).Append("_").Append(ally.getHandCardCount()).Append("|");

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece p = state.getChessPieceAtPosition(x, y);
                if (p != null)
                {
                    sb.Append((int)p.getChessType()).Append(p.isWhite() ? "W" : "B").Append(x).Append(y).Append("-");
                }
            }
        }
        return sb.ToString();
    }

    /// <summary>
    /// 在虛擬空間產生合法的 Option (取代原先直接呼叫 MatchDataManager 的方法)
    /// </summary>
    private static List<AiOption> GenerateLegalVirtualActions(BoardState state, PlayerData ally, int currentTurnCount)
    {
        List<AiOption> options = new List<AiOption>();
        bool isWhite = ally.IsWhite();
        int currentMana = ally.getMana();

        // 1. 虛擬移動選項
        int moveLimit = 1 + ally.getStatusContainer().getExtraMoveCount(currentTurnCount);
        if (ally.getMoveCount() < moveLimit)
        {
            List<ChessPiece> pieceList = new List<ChessPiece>();
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    ChessPiece p = state.getChessPieceAtPosition(x, y);
                    if (p != null && p.isWhite() == isWhite)
                        pieceList.Add(p);
                }
            }

            foreach (var piece in pieceList)
            {
                if (ally.getStatusContainer().canMoveChess(piece.getChessType(), currentTurnCount) && piece.canMove(currentTurnCount))
                {
                    int cost = piece.getMoveCost(currentTurnCount) + ally.getExtraMoveCost(currentTurnCount);
                    if (currentMana >= cost)
                    {
                        List<MoveAction> moveActions = new List<MoveAction>();
                        piece.getMovementType(currentTurnCount).getAvailableMoves(state, piece.getPosition(), isWhite, moveActions);

                        foreach (var move in moveActions)
                        {
                            options.Add(new ChessMoveOption(piece, piece.getPosition(), move.targetPos, 0f));
                        }
                    }
                }
            }
        }

        // 2. 虛擬卡片使用選項
        foreach (Card card in ally.getHandList())
        {
            if (ally.getStatusContainer().canUseCard(card.getId(), currentTurnCount))
            {
                int finalCost = ally.isCardNonCost(currentTurnCount) ? 0 : card.getCost() + ally.getExtraCardCost(currentTurnCount);
                if (currentMana >= finalCost)
                {
                    CardEffect cardEffect = CardEffectManager.getCardEffect(card);
                    if (cardEffect != null)
                    {
                        // 從卡片效果端索取所有合法的打法
                        List<CardUseOption> rawOptions = cardEffect.getUseOptions(ally.IsLocal(), card, currentMana, currentTurnCount);
                        foreach (var opt in rawOptions)
                        {
                            opt.setScore(0f);
                            options.Add(opt);
                        }
                    }
                }
            }
        }

        // 3. 虛擬保留手牌選項
        if (ally.canUseHoldAction())
        {
            foreach (Card card in ally.getHandList())
            {
                options.Add(new CardHoldOption(card, 0f));
            }
        }

        return options;
    }

    public static async Task doBestMove(bool isLocal, int currentTurnCount)
    {
        AiOption bestMove;

        while (!GameManager.IsGameOver() && (bestMove = getBestMove(isLocal, currentTurnCount)) != null)
        {
            while (HandViewManager.isPlayingAnimation())
                await Task.Delay(100);

            await Task.Delay(OPTION_BUFFER_TIME_MILLIS);

            await bestMove.execute(isLocal);

            // ★ 修正死鎖 Bug：如果當前評估的最佳解是「保留手牌(囤牌)」，代表主動操作的效益已達上限，必須跳出迴圈結束回合
            // 否則由於保留手牌不消耗盤面步數，DFS 會覺得囤牌穩賺不賠而無限重複動作導致當機
            if (bestMove is CardHoldOption)
            {
                break;
            }
        }

        while (HandViewManager.isPlayingAnimation())
            await Task.Delay(100);

        if (!GameManager.IsGameOver())
            GameManager.ChangeTurn();
    }
}