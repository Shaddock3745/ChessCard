using System.Collections.Generic;
using UnityEngine;

public class AiEvaluator
{
    private const float SCORE_WIN = 99999f;
    private const float SCORE_LOSE = -99999f;

    // 會被個性 attack/defend 縮放的基準值
    private const float BASE_THREAT_PRESSURE_W = 0.2f;
    private const float BASE_KING_ZONE_W = 10f;
    private const float BASE_KING_HARASS = 60f;

    // 非 AI 呼叫(例如 Calculate Score 按鈕)用的中庸權重
    private static readonly AiWeights DEFAULT_WEIGHTS = new AiWeights
    { attack = 1f, defend = 1f, mobilityW = 3f, handCardW = 20f, ownerReactDiscount = 0.4f, statusWeight = 5f, cardPreference = 0f };

    private static float GetPieceValue(ChessType type) => type switch
    {
        ChessType.Pawn => 100f,
        ChessType.Knight => 320f,
        ChessType.Bishop => 330f,
        ChessType.Rook => 500f,
        ChessType.Queen => 900f,
        ChessType.King => 20000f,
        _ => 0f
    };

    private static readonly int[,] PawnPST = {
        {  0,  0,  0,  0,  0,  0,  0,  0 },{ 50, 50, 50, 50, 50, 50, 50, 50 },
        { 10, 10, 20, 30, 30, 20, 10, 10 },{  5,  5, 10, 25, 25, 10,  5,  5 },
        {  0,  0,  0, 20, 20,  0,  0,  0 },{  5, -5,-10,  0,  0,-10, -5,  5 },
        {  5, 10, 10,-20,-20, 10, 10,  5 },{  0,  0,  0,  0,  0,  0,  0,  0 } };
    private static readonly int[,] KnightPST = {
        {-50,-40,-30,-30,-30,-30,-40,-50},{-40,-20,  0,  0,  0,  0,-20,-40},
        {-30,  0, 10, 15, 15, 10,  0,-30},{-30,  5, 15, 20, 20, 15,  5,-30},
        {-30,  0, 15, 20, 20, 15,  0,-30},{-30,  5, 10, 15, 15, 10,  5,-30},
        {-40,-20,  0,  5,  5,  0,-20,-40},{-50,-40,-30,-30,-30,-30,-40,-50} };
    private static readonly int[,] BishopPST = {
        {-20,-10,-10,-10,-10,-10,-10,-20},{-10,  0,  0,  0,  0,  0,  0,-10},
        {-10,  0,  5, 10, 10,  5,  0,-10},{-10,  5,  5, 10, 10,  5,  5,-10},
        {-10,  0, 10, 10, 10, 10,  0,-10},{-10, 10, 10, 10, 10, 10, 10,-10},
        {-10,  5,  0,  0,  0,  0,  5,-10},{-20,-10,-10,-10,-10,-10,-10,-20} };
    private static readonly int[,] RookPST = {
        {  0,  0,  0,  0,  0,  0,  0,  0},{  5, 10, 10, 10, 10, 10, 10,  5},
        { -5,  0,  0,  0,  0,  0,  0, -5},{ -5,  0,  0,  0,  0,  0,  0, -5},
        { -5,  0,  0,  0,  0,  0,  0, -5},{ -5,  0,  0,  0,  0,  0,  0, -5},
        { -5,  0,  0,  0,  0,  0,  0, -5},{  0,  0,  0,  5,  5,  0,  0,  0} };
    private static readonly int[,] QueenPST = {
        {-20,-10,-10, -5, -5,-10,-10,-20},{-10,  0,  0,  0,  0,  0,  0,-10},
        {-10,  0,  5,  5,  5,  5,  0,-10},{ -5,  0,  5,  5,  5,  5,  0, -5},
        {  0,  0,  5,  5,  5,  5,  0, -5},{-10,  5,  5,  5,  5,  5,  0,-10},
        {-10,  0,  5,  0,  0,  0,  0,-10},{-20,-10,-10, -5, -5,-10,-10,-20} };
    private static readonly int[,] KingPST = {
        {-30,-40,-40,-50,-50,-40,-40,-30},{-30,-40,-40,-50,-50,-40,-40,-30},
        {-30,-40,-40,-50,-50,-40,-40,-30},{-30,-40,-40,-50,-50,-40,-40,-30},
        {-20,-30,-30,-40,-40,-30,-30,-20},{-10,-20,-20,-20,-20,-20,-20,-10},
        { 20, 20,  0,  0,  0,  0, 20, 20},{ 20, 30, 10,  0,  0, 10, 30, 20} };

    private static float GetPSTValue(ChessType type, Vector2Int pos, bool isWhite)
    {
        int x = pos.x; int y = isWhite ? (7 - pos.y) : pos.y;
        return type switch
        {
            ChessType.Pawn => PawnPST[y, x],
            ChessType.Knight => KnightPST[y, x],
            ChessType.Bishop => BishopPST[y, x],
            ChessType.Rook => RookPST[y, x],
            ChessType.Queen => QueenPST[y, x],
            ChessType.King => KingPST[y, x],
            _ => 0f
        };
    }

    // 相容版(中庸權重)
    public static float EvaluateBoardState(BoardState state, PlayerData ally, PlayerData enemy, int turn)
        => EvaluateBoardState(state, ally, enemy, turn, !ally.IsWhite(), DEFAULT_WEIGHTS);
    public static float EvaluateBoardState(BoardState state, PlayerData ally, PlayerData enemy, int turn, bool sideToMoveIsWhite)
        => EvaluateBoardState(state, ally, enemy, turn, sideToMoveIsWhite, DEFAULT_WEIGHTS);

    // ★ 主評估:吃個性權重 w
    public static float EvaluateBoardState(BoardState state, PlayerData allyData, PlayerData enemyData, int currentTurnCount, bool sideToMoveIsWhite, AiWeights w)
    {
        bool isWhite = allyData.IsWhite();
        bool allyToMove = (sideToMoveIsWhite == isWhite);

        ChessPiece allyKing = state.getKing(isWhite);
        ChessPiece enemyKing = state.getKing(!isWhite);
        if (enemyKing == null) return SCORE_WIN;
        if (allyKing == null) return SCORE_LOSE;

        // === 1. 攻擊圖 + 最低身價攻擊者 + 機動力 ===
        ulong whiteAttacks = 0UL, blackAttacks = 0UL;
        float[] whiteMinAtk = new float[64];
        float[] blackMinAtk = new float[64];
        for (int i = 0; i < 64; i++) { whiteMinAtk[i] = float.MaxValue; blackMinAtk[i] = float.MaxValue; }

        int allyMobility = 0, enemyMobility = 0;
        List<MoveAction> moveBuf = new List<MoveAction>(28);

        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                ChessPiece p = state.getChessPieceAtPosition(x, y);
                if (p == null) continue;

                moveBuf.Clear();
                p.getMovementType(currentTurnCount).getAvailableMoves(state, p.getPosition(), p.isWhite(), moveBuf);

                if (p.isWhite() == isWhite) allyMobility += moveBuf.Count;
                else enemyMobility += moveBuf.Count;

                if (!p.canAttack(currentTurnCount)) continue;

                float pv = GetPieceValue(p.getChessType());
                foreach (var m in moveBuf)
                {
                    if (m.actionType != ActionType.Attack) continue;
                    int idx = m.targetPos.y * 8 + m.targetPos.x;
                    ulong bit = 1UL << idx;
                    if (p.isWhite()) { whiteAttacks |= bit; if (pv < whiteMinAtk[idx]) whiteMinAtk[idx] = pv; }
                    else { blackAttacks |= bit; if (pv < blackMinAtk[idx]) blackMinAtk[idx] = pv; }
                }
            }

        // === 2. 防守圖 ===
        bool[] whiteDefended = new bool[64];
        bool[] blackDefended = new bool[64];
        List<ChessPiece> defBuf = new List<ChessPiece>(16);
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                ChessPiece p = state.getChessPieceAtPosition(x, y);
                if (p == null || !p.canAttack(currentTurnCount)) continue;
                defBuf.Clear();
                p.getMovementType(currentTurnCount).getProtectablePieces(state, p.getPosition(), p.isWhite(), defBuf);
                foreach (var d in defBuf)
                {
                    int idx = d.getPosition().y * 8 + d.getPosition().x;
                    if (p.isWhite()) whiteDefended[idx] = true; else blackDefended[idx] = true;
                }
            }

        ulong enemyAttacks = isWhite ? blackAttacks : whiteAttacks;
        ulong allyAttacks = isWhite ? whiteAttacks : blackAttacks;

        // === 3. 國王致命檢查 ===
        Vector2Int akp = allyKing.getPosition(), ekp = enemyKing.getPosition();
        bool allyKingAttacked = (enemyAttacks & (1UL << (akp.y * 8 + akp.x))) != 0;
        bool enemyKingAttacked = (allyAttacks & (1UL << (ekp.y * 8 + ekp.x))) != 0;
        if (enemyKingAttacked && allyToMove) return SCORE_WIN;
        if (allyKingAttacked && !allyToMove) return SCORE_LOSE;

        // === 4. 逐子:材料 + 位置 + 狀態 − 受威脅損失(攻守分權重) + 威脅壓制 ===
        float score = 0f;
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece == null) continue;

                int sq = y * 8 + x;
                ulong bit = 1UL << sq;
                bool ownerWhite = piece.isWhite();
                float pieceVal = GetPieceValue(piece.getChessType());

                float baseVal = pieceVal
                              + GetPSTValue(piece.getChessType(), piece.getPosition(), ownerWhite)
                              + piece.getStatusContainerAiValue(ownerWhite == isWhite, currentTurnCount) * w.statusWeight;

                if (piece.getChessType() != ChessType.King)
                {
                    float tl = ThreatLoss(piece, sq, ownerWhite);
                    tl *= (ownerWhite == isWhite) ? w.defend : w.attack; // 我方子損失看防守傾向,敵方子可吃看攻擊傾向
                    baseVal -= tl;
                }

                if (ownerWhite == isWhite) score += baseVal;
                else score -= baseVal;

                // 威脅壓制:我方攻擊到的敵方非王棋子(即使有保護也給分)× 攻擊傾向
                if (ownerWhite != isWhite && piece.getChessType() != ChessType.King && (allyAttacks & bit) != 0)
                    score += BASE_THREAT_PRESSURE_W * w.attack * pieceVal;
            }

        // === 5. 護王 / 逼宮(攻守分權重) ===
        if (allyKingAttacked && allyToMove) score -= BASE_KING_HARASS * w.defend;
        if (enemyKingAttacked && !allyToMove) score += BASE_KING_HARASS * w.attack;
        score += BASE_KING_ZONE_W * w.attack * CountZonePressure(ekp, allyAttacks);
        score -= BASE_KING_ZONE_W * w.defend * CountZonePressure(akp, enemyAttacks);

        // === 6. 機動力 + 手牌資源 ===
        score += w.mobilityW * (allyMobility - enemyMobility);
        score += w.handCardW * (allyData.getHandCardCount() - enemyData.getHandCardCount());

        // === 7. 玩家層級狀態 ===
        score += allyData.getStatusContainerAiValue(currentTurnCount) * w.statusWeight;
        score -= enemyData.getStatusContainerAiValue(currentTurnCount) * w.statusWeight;

        return score;

        // ---- 區域函式 ----
        float ThreatLoss(ChessPiece piece, int sq, bool ownerWhite)
        {
            bool attacked = ((ownerWhite ? blackAttacks : whiteAttacks) & (1UL << sq)) != 0;
            if (!attacked) return 0f;
            float pieceVal = GetPieceValue(piece.getChessType());
            float minAtk = ownerWhite ? blackMinAtk[sq] : whiteMinAtk[sq];
            bool defended = ownerWhite ? whiteDefended[sq] : blackDefended[sq];
            float loss = defended ? Mathf.Max(0f, pieceVal - minAtk) : pieceVal;
            if (ownerWhite == sideToMoveIsWhite) loss *= w.ownerReactDiscount;
            return loss;
        }

        int CountZonePressure(Vector2Int kingPos, ulong attackerMap)
        {
            int count = 0;
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    int nx = kingPos.x + dx, ny = kingPos.y + dy;
                    if (nx < 0 || nx >= 8 || ny < 0 || ny >= 8) continue;
                    if ((attackerMap & (1UL << (ny * 8 + nx))) != 0) count++;
                }
            return count;
        }
    }

    public static float ScoreToWinRate(float rawScore)
    {
        if (rawScore >= 90000f) return 1.0f;
        if (rawScore <= -90000f) return 0.0f;
        return 1.0f / (1.0f + Mathf.Exp(-0.005f * rawScore));
    }
}