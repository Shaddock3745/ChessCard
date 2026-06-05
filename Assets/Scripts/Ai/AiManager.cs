using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AiPlan
{
    private AiOption firstRealOption;
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

    private const int BEAM_WIDTH = 12;
    private const int MAX_DEPTH = 6;

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

    public static AiOption getBestMove(bool isLocal, int currentTurnCount)
    {
        BoardState currentState = BoardManager.getCloneBoardState();
        PlayerData currentAlly = MatchDataManager.getPlayerData(isLocal).Clone();
        PlayerData currentEnemy = MatchDataManager.getPlayerData(!isLocal).Clone();

        HashSet<string> visitedStates = new HashSet<string>();

        AiPlan bestPlan = RunDFS(currentState, currentAlly, currentEnemy, currentTurnCount, 0, visitedStates, null);

        if (bestPlan == null || bestPlan.getScore() <= -90000f || bestPlan.getOption() == null)
            return null;

        AiOption bestAction = bestPlan.getOption();
        float noise = Random.Range(-currentAlly.getAiProfile().getNoiseRange(), currentAlly.getAiProfile().getNoiseRange());
        bestAction.setScore(bestPlan.getScore() + noise);

        return bestAction;
    }

    private static AiPlan RunDFS(BoardState state, PlayerData ally, PlayerData enemy, int currentTurnCount, int depth, HashSet<string> visitedStates, AiOption firstActionOfThisPath)
    {
        if (depth >= MAX_DEPTH || state.getKing(true) == null || state.getKing(false) == null)
        {
            float score = AiEvaluator.EvaluateBoardState(state, ally, enemy, currentTurnCount);
            return new AiPlan(firstActionOfThisPath, score);
        }

        List<AiOption> candidates = GenerateLegalVirtualActions(state, ally, currentTurnCount);

        if (candidates.Count == 0)
        {
            float score = AiEvaluator.EvaluateBoardState(state, ally, enemy, currentTurnCount);
            return new AiPlan(firstActionOfThisPath, score);
        }

        foreach (var candidate in candidates)
        {
            BoardState testState = state.Clone();
            PlayerData testAlly = ally.Clone();
            PlayerData testEnemy = enemy.Clone();

            candidate.executeVirtually(testState, testAlly, testEnemy, currentTurnCount);
            float quickScore = AiEvaluator.EvaluateBoardState(testState, testAlly, testEnemy, currentTurnCount);
            candidate.setQuickScore(quickScore);
        }

        var topCandidates = candidates.OrderByDescending(c => c.getQuickScore()).Take(BEAM_WIDTH).ToList();
        AiPlan bestPlan = new AiPlan(firstActionOfThisPath, float.MinValue);

        foreach (var candidate in topCandidates)
        {
            BoardState nextState = state.Clone();
            PlayerData nextAlly = ally.Clone();
            PlayerData nextEnemy = enemy.Clone();

            candidate.executeVirtually(nextState, nextAlly, nextEnemy, currentTurnCount);

            string stateHash = GenerateStateHash(nextState, nextAlly);
            if (visitedStates.Contains(stateHash)) continue;
            visitedStates.Add(stateHash);

            AiOption rootAction = (depth == 0) ? candidate : firstActionOfThisPath;

            if (candidate.isRandomEffect() || candidate.isHoldAction())
            {
                float score = AiEvaluator.EvaluateBoardState(nextState, nextAlly, nextEnemy, currentTurnCount);
                if (score > bestPlan.getScore()) bestPlan = new AiPlan(rootAction, score);
                continue;
            }

            AiPlan subPlan = RunDFS(nextState, nextAlly, nextEnemy, currentTurnCount, depth + 1, visitedStates, rootAction);

            if (subPlan.getScore() > bestPlan.getScore())
            {
                bestPlan = subPlan;
            }
        }

        if (bestPlan.getScore() == float.MinValue)
        {
            float fallbackScore = AiEvaluator.EvaluateBoardState(state, ally, enemy, currentTurnCount);
            return new AiPlan(firstActionOfThisPath, fallbackScore);
        }

        return bestPlan;
    }

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

    private static List<AiOption> GenerateLegalVirtualActions(BoardState state, PlayerData ally, int currentTurnCount)
    {
        List<AiOption> options = new List<AiOption>();
        bool isWhite = ally.IsWhite();
        int currentMana = ally.getMana();

        // 1. 虛擬卡片使用選項 (移至最前，讓 AI 優先上 Buff 再移動)
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
                        List<CardUseOption> rawOptions = cardEffect.getUseOptions(state, ally.IsLocal(), card, currentMana, currentTurnCount);
                        foreach (var opt in rawOptions)
                        {
                            opt.setScore(0f);
                            options.Add(opt);
                        }
                    }
                }
            }
        }

        // 2. 虛擬移動選項
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