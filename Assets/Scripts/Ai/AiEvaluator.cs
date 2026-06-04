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

    // 基礎權重乘數 (將由 AiProfile 進行倍率放大/縮小)
    private const float BASE_WEIGHT_MANA = 2f;
    private const float BASE_WEIGHT_HAND_CARD = 3f;
    private const float BASE_WEIGHT_MOBILITY = 0.5f;

    // 避免重複 new 的共用快取容器 (由於推演是循序發生，靜態快取可大幅降低 GC)
    private static readonly List<MoveAction> threatCheckBuffer = new List<MoveAction>(64);
    private static readonly List<MoveAction> legalMoveBuffer = new List<MoveAction>(64);

    /// <summary>
    /// 終極狀態評估函式：給定一個盤面與雙方資料，回傳該盤面對 Ally 而言的總體分數。
    /// 分數越高，代表此盤面/路徑越值得選擇。
    /// </summary>
    public static float EvaluateBoardState(BoardState state, PlayerData allyData, PlayerData enemyData, int currentTurnCount)
    {
        bool allyIsWhite = allyData.IsWhite();
        AiProfile profile = allyData.getAiProfile(); // ★ 取得個性權重

        // 1. 【第一層級：神聖級 (Absolute)】生死交關
        float absoluteScore = EvaluateAbsoluteTier(state, allyIsWhite, currentTurnCount);
        if (Mathf.Abs(absoluteScore) >= 90000f) return absoluteScore;

        List<ChessPiece> allyPieces = GetAllPieces(state, allyIsWhite);
        List<ChessPiece> enemyPieces = GetAllPieces(state, !allyIsWhite);

        float totalScore = absoluteScore;

        // 2. 【第二層級：核心戰略級 (Strategic)】- 物質實力與靜態交換(SEE)
        // ★ 傳入 profile，讓個性影響攻守判斷
        totalScore += EvaluateStrategicTier(state, allyPieces, enemyPieces, allyData, enemyData, currentTurnCount, profile);

        // 3. 【第三層級：戰術陣地級 (Tactical)】- 位置、護衛、推進潛力
        totalScore += EvaluateTacticalTier(state, allyPieces, enemyPieces, allyIsWhite, profile);

        // 4. 【第四層級：運營微調級 (Operational)】- 機動力、殘餘資源、狀態
        totalScore += EvaluateOperationalTier(state, allyPieces, enemyPieces, allyData, enemyData, currentTurnCount, profile);

        return totalScore;
    }


    // ==========================================
    // 模組化層級實作 (Tier Implementations)
    // ==========================================

    /// <summary>
    /// 第一層級：判斷生死與將軍的絕對狀態
    /// </summary>
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

    /// <summary>
    /// 第二層級：計算雙方動態棋力值與火力交換威脅 (SEE)
    /// </summary>
    private static float EvaluateStrategicTier(BoardState state, List<ChessPiece> allyPieces, List<ChessPiece> enemyPieces, PlayerData allyData, PlayerData enemyData, int currentTurnCount, AiProfile profile)
    {
        float strategicScore = 0f;

        // 計算我方實力與威脅 (加分)
        foreach (var piece in allyPieces)
        {
            float dynamicValue = GetPieceDynamicValue(piece, allyData.IsLocal(), currentTurnCount, profile);
            strategicScore += dynamicValue;
            strategicScore += EvaluatePieceThreatsAndProtection(state, piece, dynamicValue, allyData.IsWhite(), currentTurnCount, profile);
        }

        // 計算敵方實力與威脅 (扣分)
        foreach (var piece in enemyPieces)
        {
            float dynamicValue = GetPieceDynamicValue(piece, enemyData.IsLocal(), currentTurnCount, profile);
            strategicScore -= dynamicValue;
            strategicScore -= EvaluatePieceThreatsAndProtection(state, piece, dynamicValue, !allyData.IsWhite(), currentTurnCount, profile);
        }

        return strategicScore;
    }

    /// <summary>
    /// 第三層級：計算陣地控制、士兵推進指數、國王護衛網
    /// </summary>
    private static float EvaluateTacticalTier(BoardState state, List<ChessPiece> allyPieces, List<ChessPiece> enemyPieces, bool allyIsWhite, AiProfile profile)
    {
        float tacticalScore = 0f;

        foreach (var piece in allyPieces)
            tacticalScore += GetPositionalValue(piece, allyIsWhite);

        foreach (var piece in enemyPieces)
            tacticalScore -= GetPositionalValue(piece, !allyIsWhite);

        // ★ 防守權重 (protectWeight) 極大影響國王安全評分
        tacticalScore += EvaluateKingSafety(state, state.getKing(allyIsWhite), allyIsWhite) * profile.getProtectWeight();
        tacticalScore -= EvaluateKingSafety(state, state.getKing(!allyIsWhite), !allyIsWhite) * profile.getAttackWeight(); // 敵方國王不安全，對激進 AI 來說是好事

        return tacticalScore;
    }

    /// <summary>
    /// 第四層級：計算全體機動性、殘餘魔力與手牌、全域玩家狀態
    /// </summary>
    private static float EvaluateOperationalTier(BoardState state, List<ChessPiece> allyPieces, List<ChessPiece> enemyPieces, PlayerData allyData, PlayerData enemyData, int currentTurnCount, AiProfile profile)
    {
        float opScore = 0f;

        // 1. 機動力 ★ 乘上 mobilityWeight
        opScore += GetTotalMobility(state, allyPieces, currentTurnCount) * BASE_WEIGHT_MOBILITY * profile.getMobilityWeight();
        opScore -= GetTotalMobility(state, enemyPieces, currentTurnCount) * BASE_WEIGHT_MOBILITY * profile.getMobilityWeight();

        // 2. 殘餘經濟資源 ★ 乘上 resourceWeight (決定 AI 是吝嗇還是揮霍)
        opScore += ((allyData.getMana() * BASE_WEIGHT_MANA) + (allyData.getHandCardCount() * BASE_WEIGHT_HAND_CARD)) * profile.getResourceWeight();
        opScore -= ((enemyData.getMana() * BASE_WEIGHT_MANA) + (enemyData.getHandCardCount() * BASE_WEIGHT_HAND_CARD)) * profile.getResourceWeight();

        // 3. 玩家全域狀態 ★ 乘上 cardPreference (因為全域狀態通常由強大魔法卡產生，奇招型極愛)
        opScore += allyData.getStatusContainer().getStatusContainerAiValue(allyData.IsLocal(), currentTurnCount) * profile.getCardPreference();
        opScore -= enemyData.getStatusContainer().getStatusContainerAiValue(enemyData.IsLocal(), currentTurnCount) * profile.getCardPreference();

        return opScore;
    }

    // ==========================================
    // 底層細節輔助方法 (Low-Level Helpers)
    // ==========================================

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

        // ★ 棋子身上的狀態價值乘上 cardPreference (奇招型會把帶 Buff 的棋子當成寶貝)
        float statusModifiers = piece.getStatusContainerAiValue(isLocal, currentTurnCount) * profile.getCardPreference();

        return Mathf.Max(1f, baseVal + statusModifiers);
    }

    /// <summary>
    /// 靜態交換評估 (SEE) 與保護網計算
    /// </summary>
    private static float EvaluatePieceThreatsAndProtection(BoardState state, ChessPiece attacker, float attackerValue, bool isWhite, int currentTurnCount, AiProfile profile)
    {
        float score = 0f;
        if (!attacker.canAttack(currentTurnCount)) return 0f;

        legalMoveBuffer.Clear();
        attacker.getMovementType(currentTurnCount).getAvailableMoves(state, attacker.getPosition(), isWhite, legalMoveBuffer);

        foreach (var move in legalMoveBuffer)
        {
            ChessPiece target = state.getChessPieceAtPosition(move.targetPos);

            if (target != null)
            {
                if (target.isWhite() != isWhite)
                {
                    // === 攻擊敵方 ===
                    float targetValue = GetPieceBaseValue(target.getChessType());
                    if (targetValue == 0) continue;

                    bool isTargetProtected = IsSquareThreatened(state, move.targetPos, target.isWhite(), currentTurnCount);

                    if (!isTargetProtected)
                    {
                        // ★ 白嫖敵軍：激進型給予更高評價
                        score += (targetValue * 0.8f) * profile.getAttackWeight();
                    }
                    else
                    {
                        // 目標有保護，計算換子虧盈
                        if (attackerValue <= targetValue)
                            score += ((targetValue - attackerValue * 0.5f) * 0.5f) * profile.getAttackWeight();
                        else
                            score -= ((attackerValue - targetValue) * 0.8f) * profile.getProtectWeight(); // ★ 虧本生意：保守型嚴格避免
                    }
                }
                else
                {
                    // === 友軍保護網 ===
                    if (target.getChessType() != ChessType.King)
                    {
                        // ★ 友軍互保：保守型給予極高評價
                        score += (GetPieceBaseValue(target.getChessType()) * 0.1f) * profile.getProtectWeight();
                    }
                }
            }
        }
        return score;
    }

    /// <summary>
    /// 計算陣地、中心控制與士兵推進潛力
    /// </summary>
    private static float GetPositionalValue(ChessPiece piece, bool isWhite)
    {
        float score = 0f;
        Vector2Int pos = piece.getPosition();

        // 1. 中心控制 (越靠近中心 3.5, 3.5 越高分)
        float distToCenter = Vector2.Distance(pos, new Vector2(3.5f, 3.5f));
        score += (8f - distToCenter) * 0.5f;

        // 2. 士兵推進指數 (越靠近底線分數成指數上升)
        if (piece.getChessType() == ChessType.Pawn)
        {
            int rankAdvancement = isWhite ? pos.y - 1 : 6 - pos.y;
            if (rankAdvancement > 0)
                score += Mathf.Pow(1.8f, rankAdvancement);
        }

        return score;
    }

    /// <summary>
    /// 評估國王護衛網 (周圍八格的安全程度)
    /// </summary>
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
                if (checkPos.x < 0 || checkPos.x > 7 || checkPos.y < 0 || checkPos.y > 7) continue;

                ChessPiece neighbor = state.getChessPieceAtPosition(checkPos);
                if (neighbor != null && neighbor.isWhite() == isWhite)
                {
                    safetyScore += 5f; // 友軍駐守加分
                }
            }
        }
        return safetyScore;
    }

    /// <summary>
    /// 判定指定格子是否正處於某方陣營的火力威脅之下
    /// </summary>
    private static bool IsSquareThreatened(BoardState state, Vector2Int targetPos, bool byWhite, int currentTurnCount)
    {
        List<ChessPiece> threatPieces = GetAllPieces(state, byWhite);
        foreach (var piece in threatPieces)
        {
            if (!piece.canAttack(currentTurnCount)) continue;

            threatCheckBuffer.Clear();
            // 修正：依照新的 MovementType 介面，傳入 state
            piece.getMovementType(currentTurnCount).getAvailableMoves(state, piece.getPosition(), byWhite, threatCheckBuffer);
            foreach (var move in threatCheckBuffer)
            {
                if (move.targetPos == targetPos && move.actionType == ActionType.Attack)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 計算機動性總和 (合法步數)
    /// </summary>
    private static int GetTotalMobility(BoardState state, List<ChessPiece> pieces, int currentTurnCount)
    {
        int totalMoves = 0;
        foreach (var piece in pieces)
        {
            if (!piece.canMove(currentTurnCount)) continue;

            legalMoveBuffer.Clear();
            // 修正：依照新的 MovementType 介面，傳入 state
            piece.getMovementType(currentTurnCount).getAvailableMoves(state, piece.getPosition(), piece.isWhite(), legalMoveBuffer);
            totalMoves += legalMoveBuffer.Count;
        }
        return totalMoves;
    }

    /// <summary>
    /// 取得場上某陣營的所有存活棋子
    /// </summary>
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