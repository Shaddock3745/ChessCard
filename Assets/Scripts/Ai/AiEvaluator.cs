using System.Collections.Generic;
using UnityEngine;

public class AiEvaluator
{
    // ==========================================
    // 權重常數定義 (Weights & Constants)
    // ==========================================
    private const float SCORE_WIN = 99999f;
    private const float SCORE_LOSE = -99999f;
    private const float SCORE_CHECKED_PENALTY = -5000f;

    // 資源資產權重平衡 (解決守財奴與不打牌問題)
    private const float BASE_WEIGHT_MANA = 0.5f;      // 魔力不跨回合留存，降低保留價值以鼓勵消耗
    private const float BASE_WEIGHT_HAND_CARD = 2.0f; // 手牌基礎保留分
    private const float BASE_WEIGHT_HOLD_CARD = 3.5f; // 保留區資產價值 (高於手牌，鼓勵多餘魔力跨回合蓄水)
    private const float BASE_WEIGHT_MOBILITY = 0.5f;

    private static readonly List<MoveAction> threatCheckBuffer = new List<MoveAction>(64);
    private static readonly List<MoveAction> legalMoveBuffer = new List<MoveAction>(64);

    public static float EvaluateBoardState(BoardState state, PlayerData allyData, PlayerData enemyData, int currentTurnCount)
    {
        bool allyIsWhite = allyData.IsWhite();
        AiProfile profile = allyData.getAiProfile();

        // 1. 【第一層級：神聖級】生死與解將判定
        float absoluteScore = EvaluateAbsoluteTier(state, allyIsWhite, currentTurnCount);
        if (Mathf.Abs(absoluteScore) >= 90000f) return absoluteScore;

        List<ChessPiece> allyPieces = GetAllPieces(state, allyIsWhite);
        List<ChessPiece> enemyPieces = GetAllPieces(state, !allyIsWhite);

        float totalScore = absoluteScore;

        // 2. 【第二層級：核心戰略級】物資、單向曝險防守、微量進攻火網
        totalScore += EvaluateStrategicTier(state, allyPieces, enemyPieces, allyData, enemyData, currentTurnCount, profile);

        // 3. 【第三層級：戰術陣地級】中心控制、大子滲透、國王安全網
        totalScore += EvaluateTacticalTier(state, allyPieces, enemyPieces, allyIsWhite, profile);

        // 4. 【第四層級：運營微調級】時效狀態核銷、機動力、資源留存清算
        totalScore += EvaluateOperationalTier(state, allyPieces, enemyPieces, allyData, enemyData, currentTurnCount, profile);

        return totalScore;
    }

    private static float EvaluateAbsoluteTier(BoardState state, bool allyIsWhite, int currentTurnCount)
    {
        ChessPiece allyKing = state.getKing(allyIsWhite);
        ChessPiece enemyKing = state.getKing(!allyIsWhite);

        if (enemyKing == null) return SCORE_WIN;
        if (allyKing == null) return SCORE_LOSE;

        if (IsSquareThreatened(state, allyKing.getPosition(), !allyIsWhite, currentTurnCount))
            return SCORE_CHECKED_PENALTY;

        return 0f;
    }

    private static float EvaluateStrategicTier(BoardState state, List<ChessPiece> allyPieces, List<ChessPiece> enemyPieces, PlayerData allyData, PlayerData enemyData, int currentTurnCount, AiProfile profile)
    {
        float strategicScore = 0f;

        foreach (var piece in allyPieces)
        {
            float dynamicValue = GetPieceDynamicValue(piece, allyData.IsLocal(), currentTurnCount, profile);
            strategicScore += dynamicValue;
            // 扣除我方棋子暴露在危險中的風險分數 (防禦與直接擊殺誘因)
            strategicScore -= EvaluatePieceRisk(state, piece, dynamicValue, allyData.IsWhite(), currentTurnCount, profile);
        }

        foreach (var piece in enemyPieces)
        {
            float dynamicValue = GetPieceDynamicValue(piece, enemyData.IsLocal(), currentTurnCount, profile);
            strategicScore -= dynamicValue;
            // 給予瞄準敵軍的微量進攻紅利 (走位包圍與鋪墊誘因，設有嚴格上限防止只看不如吃)
            strategicScore += EvaluateThreateningBonus(state, piece, dynamicValue, !allyData.IsWhite(), currentTurnCount, profile);
        }

        return strategicScore;
    }

    private static float EvaluateThreateningBonus(BoardState state, ChessPiece enemyPiece, float enemyPieceValue, bool isEnemyWhite, int currentTurnCount, AiProfile profile)
    {
        if (IsSquareThreatened(state, enemyPiece.getPosition(), !isEnemyWhite, currentTurnCount))
        {
            return (enemyPieceValue * 0.25f) * profile.getAttackWeight();
        }
        return 0f;
    }

    private static float EvaluatePieceRisk(BoardState state, ChessPiece piece, float pieceValue, bool isWhite, int currentTurnCount, AiProfile profile)
    {
        if (!IsSquareThreatened(state, piece.getPosition(), !isWhite, currentTurnCount)) return 0f;

        bool isProtectedByAlly = IsSquareThreatened(state, piece.getPosition(), isWhite, currentTurnCount);

        if (!isProtectedByAlly)
        {
            // 無保護白給棋子 (Hanging piece)：扣除幾乎全額戰力分值
            return (pieceValue * 0.9f) * profile.getProtectWeight();
        }
        else
        {
            // 有友軍保護，檢查敵方是否能以小博大進行不對等交換
            float lowestAttackerValue = GetLowestAttackerValue(state, piece.getPosition(), !isWhite, currentTurnCount);
            if (lowestAttackerValue < pieceValue)
            {
                return ((pieceValue - lowestAttackerValue) * 0.8f) * profile.getProtectWeight();
            }
        }
        return 0f;
    }

    private static float GetLowestAttackerValue(BoardState state, Vector2Int targetPos, bool byWhite, int currentTurnCount)
    {
        float lowestValue = 9999f;
        List<ChessPiece> attackers = GetAllPieces(state, byWhite);
        foreach (var p in attackers)
        {
            if (!p.canAttack(currentTurnCount)) continue;
            threatCheckBuffer.Clear();
            p.getMovementType(currentTurnCount).getAvailableMoves(state, p.getPosition(), byWhite, threatCheckBuffer);
            foreach (var move in threatCheckBuffer)
            {
                if (move.targetPos == targetPos && move.actionType == ActionType.Attack)
                {
                    float val = GetPieceBaseValue(p.getChessType());
                    if (val < lowestValue) lowestValue = val;
                    break;
                }
            }
        }
        return lowestValue;
    }

    private static float EvaluateTacticalTier(BoardState state, List<ChessPiece> allyPieces, List<ChessPiece> enemyPieces, bool allyIsWhite, AiProfile profile)
    {
        float tacticalScore = 0f;

        foreach (var piece in allyPieces)
            tacticalScore += GetPositionalValue(piece, allyIsWhite);

        foreach (var piece in enemyPieces)
            tacticalScore -= GetPositionalValue(piece, !allyIsWhite);

        tacticalScore += EvaluateKingSafety(state, state.getKing(allyIsWhite), allyIsWhite) * profile.getProtectWeight();
        tacticalScore -= EvaluateKingSafety(state, state.getKing(!allyIsWhite), !allyIsWhite) * profile.getAttackWeight();

        return tacticalScore;
    }

    private static float EvaluateOperationalTier(BoardState state, List<ChessPiece> allyPieces, List<ChessPiece> enemyPieces, PlayerData allyData, PlayerData enemyData, int currentTurnCount, AiProfile profile)
    {
        float opScore = 0f;

        opScore += GetTotalMobility(state, allyPieces, currentTurnCount) * BASE_WEIGHT_MOBILITY * profile.getMobilityWeight();
        opScore -= GetTotalMobility(state, enemyPieces, currentTurnCount) * BASE_WEIGHT_MOBILITY * profile.getMobilityWeight();

        // 剩餘物資清算
        float allyResourceScore = (allyData.getMana() * BASE_WEIGHT_MANA)
                                + (allyData.getHandCardCount() * BASE_WEIGHT_HAND_CARD)
                                + (allyData.getHoldingList().Count * BASE_WEIGHT_HOLD_CARD);
        opScore += allyResourceScore * profile.getResourceWeight();

        float enemyResourceScore = (enemyData.getMana() * BASE_WEIGHT_MANA)
                                 + (enemyData.getHandCardCount() * BASE_WEIGHT_HAND_CARD)
                                 + (enemyData.getHoldingList().Count * BASE_WEIGHT_HOLD_CARD);
        opScore -= enemyResourceScore * profile.getResourceWeight();

        // 時效性狀態核銷：若全域移動步數已耗盡，卡片給予的全域戰鬥狀態評估分值打一折 (杜絕空發法術)
        float allyStatusVal = allyData.getStatusContainer().getStatusContainerAiValue(allyData.IsLocal(), currentTurnCount);
        int allyMoveLimit = 1 + allyData.getStatusContainer().getExtraMoveCount(currentTurnCount);
        if (allyData.getMoveCount() >= allyMoveLimit) allyStatusVal *= 0.1f;
        opScore += allyStatusVal * profile.getCardPreference();

        float enemyStatusVal = enemyData.getStatusContainer().getStatusContainerAiValue(enemyData.IsLocal(), currentTurnCount);
        int enemyMoveLimit = 1 + enemyData.getStatusContainer().getExtraMoveCount(currentTurnCount);
        if (enemyData.getMoveCount() >= enemyMoveLimit) enemyStatusVal *= 0.1f;
        opScore -= enemyStatusVal * profile.getCardPreference();

        return opScore;
    }

    public static float GetPieceBaseValue(ChessType type) => type switch
    {
        ChessType.Pawn => 10f,
        ChessType.Knight => 30f,
        ChessType.Bishop => 30f,
        ChessType.Rook => 50f,
        ChessType.Queen => 90f,
        ChessType.King => 0f,
        _ => 0f
    };

    private static float GetPieceDynamicValue(ChessPiece piece, bool isLocal, int currentTurnCount, AiProfile profile)
    {
        float baseVal = GetPieceBaseValue(piece.getChessType());
        if (baseVal == 0) return 0;

        float statusModifiers = piece.getStatusContainer().getStatusContainerAiValue(isLocal, piece, currentTurnCount);

        // 時效性狀態核銷：若棋子本身已無行動/攻擊能力，附帶的增益狀態分值打一折
        if (!piece.canMove(currentTurnCount) && !piece.canAttack(currentTurnCount))
        {
            statusModifiers *= 0.1f;
        }

        statusModifiers *= profile.getCardPreference();
        return Mathf.Max(1f, baseVal + statusModifiers);
    }

    private static float GetPositionalValue(ChessPiece piece, bool isWhite)
    {
        float score = 0f;
        Vector2Int pos = piece.getPosition();

        // 1. 中心控制力 (全棋子通用)
        float distToCenter = Vector2.Distance(pos, new Vector2(3.5f, 3.5f));
        score += (8f - distToCenter) * 0.5f;

        // 2. 推進與領地滲透指數 (解決召喚後原地發呆問題)
        if (piece.getChessType() == ChessType.Pawn)
        {
            // 士兵：底線指數推進分
            int rankAdvancement = isWhite ? pos.y - 1 : 6 - pos.y;
            if (rankAdvancement > 0)
                score += Mathf.Pow(1.8f, rankAdvancement);
        }
        else if (piece.getChessType() != ChessType.King)
        {
            // 大子 (車/馬/象/象/后)：敵方半場領地滲透分，驅使進場後向前壓制火網
            int penetration = isWhite ? pos.y : 7 - pos.y;
            score += penetration * 0.3f;
        }

        return score;
    }

    private static float EvaluateKingSafety(BoardState state, ChessPiece king, bool isWhite)
    {
        if (king == null) return 0f;
        float safetyScore = 0f;
        Vector2Int kPos = king.getPosition();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                Vector2Int checkPos = new Vector2Int(kPos.x + dx, kPos.y + dy);
                if (checkPos.x >= 0 && checkPos.x < 8 && checkPos.y >= 0 && checkPos.y < 8)
                {
                    ChessPiece neighbor = state.getChessPieceAtPosition(checkPos.x, checkPos.y);
                    if (neighbor != null && neighbor.isWhite() == isWhite)
                    {
                        safetyScore += 5f;
                    }
                }
            }
        }
        return safetyScore;
    }

    private static bool IsSquareThreatened(BoardState state, Vector2Int targetPos, bool byWhite, int currentTurnCount)
    {
        List<ChessPiece> threatPieces = GetAllPieces(state, byWhite);
        foreach (var piece in threatPieces)
        {
            if (!piece.canAttack(currentTurnCount)) continue;

            threatCheckBuffer.Clear();
            piece.getMovementType(currentTurnCount).getAvailableMoves(state, piece.getPosition(), byWhite, threatCheckBuffer);
            foreach (var move in threatCheckBuffer)
            {
                if (move.targetPos == targetPos && move.actionType == ActionType.Attack)
                    return true;
            }
        }
        return false;
    }

    private static int GetTotalMobility(BoardState state, List<ChessPiece> pieces, int currentTurnCount)
    {
        int totalMoves = 0;
        foreach (var piece in pieces)
        {
            if (!piece.canMove(currentTurnCount)) continue;

            legalMoveBuffer.Clear();
            piece.getMovementType(currentTurnCount).getAvailableMoves(state, piece.getPosition(), piece.isWhite(), legalMoveBuffer);
            totalMoves += legalMoveBuffer.Count;
        }
        return totalMoves;
    }

    private static List<ChessPiece> GetAllPieces(BoardState state, bool isWhite)
    {
        List<ChessPiece> pieces = new List<ChessPiece>();
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece p = state.getChessPieceAtPosition(x, y);
                if (p != null && p.isWhite() == isWhite)
                    pieces.Add(p);
            }
        }
        return pieces;
    }
}