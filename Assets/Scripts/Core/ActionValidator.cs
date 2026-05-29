using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActionRecord
{
    public ChessPiece piece;
    public Vector2Int fromPos;
    public Vector2Int toPos;
    public ActionType type;

    public ActionRecord(ChessPiece piece, Vector2Int fromPos, Vector2Int toPos, ActionType type)
    {
        this.piece = piece;
        this.fromPos = fromPos;
        this.toPos = toPos;
        this.type = type;
    }
}

public class ActionValidator
{
    private static readonly int MAX_MOVE_COUNT_PER_TURN = 1;
    private static readonly List<MoveAction> rawMovesBuffer = new List<MoveAction>(64);

    public static bool canUseCard(PlayerData player, Card card, int currentTurnCount)
    {
        int extraCost = player.getStatusContainer().getGlobalExtraCardCost(currentTurnCount);
        int finalCost = player.isCardNonCost(currentTurnCount) ? 0 : card.getCost() + extraCost;

        return player.getMana() >= finalCost
            && player.getStatusContainer().canUseCard(card.getId(), currentTurnCount)
            && CardEffectManager.canActivateCard(player.IsLocal(), card, currentTurnCount);
    }

    public static bool canAttackGlobally(PlayerData player, int currentTurnCount)
    {
        return !reachMoveLimit(player, currentTurnCount) && player.getStatusContainer().canAttack(currentTurnCount);
    }

    private static int calculateNormalMovesUsed(List<ActionRecord> records, int currentTurnCount)
    {
        var movesByPiece = records
            .Where(r => r.type == ActionType.Move)
            .GroupBy(r => r.piece);

        int normalMovesUsed = 0;

        foreach (var group in movesByPiece)
            normalMovesUsed += Mathf.Max(0, group.Count() - group.Key.getExtraMoveCount(currentTurnCount));

        return normalMovesUsed;
    }

    public static bool reachMoveLimit(PlayerData player, int currentTurnCount)
    {
        int globalMaxMoves = MAX_MOVE_COUNT_PER_TURN + player.getStatusContainer().getExtraMoveCount(currentTurnCount);
        int normalMovesUsed = calculateNormalMovesUsed(player.getChessMoveRecords(), currentTurnCount);

        return normalMovesUsed >= globalMaxMoves;
    }

    public static bool canPieceMoveThisTurn(PlayerData player, ChessPiece piece, int currentTurnCount)
    {
        List<ActionRecord> records = player.getChessMoveRecords();

        int pieceMoves = records.Count(r => r.piece == piece);
        int globalMaxMoves = MAX_MOVE_COUNT_PER_TURN + player.getExtraMoveCount(currentTurnCount);

        if (player.getStatusContainer().isLimitedToMoveSpecificChess(currentTurnCount))
            if (player.getStatusContainer().getLimitedToMoveSpecificChess(currentTurnCount) != piece)
                return false;

        int pieceBonusMoves = piece.getExtraMoveCount(currentTurnCount);
        int pieceMaxMoves = MAX_MOVE_COUNT_PER_TURN + pieceBonusMoves;

        if (pieceMoves >= pieceMaxMoves)
            return false;

        int normalMovesUsed = calculateNormalMovesUsed(records, currentTurnCount);

        if (pieceBonusMoves > 0)
            return normalMovesUsed < globalMaxMoves || pieceMoves > 0;
        else
            return normalMovesUsed < globalMaxMoves;
    }

    public static bool canSummon(PlayerData player, int currentTurnCount)
    {
        return player.getStatusContainer().canSummon(currentTurnCount);
    }

    public static void getLegalMoves(PlayerData player, ChessPiece piece, int currentTurnCount, List<MoveAction> bufferList)
    {
        bufferList.Clear();

        if (!canPieceMoveThisTurn(player, piece, currentTurnCount) || piece.isWhite() != player.IsWhite())
            return;

        int finalMoveCost = piece.getMoveCost(currentTurnCount) + player.getStatusContainer().getGlobalExtraMoveCost(currentTurnCount);
        if (player.getMana() < finalMoveCost ||
            !player.getStatusContainer().canMoveChess(piece.getChessType(), currentTurnCount) ||
            !piece.canMove(currentTurnCount))
        {
            return;
        }

        rawMovesBuffer.Clear();
        piece.getMovementType(currentTurnCount).getAvailableMoves(piece.getPosition(), piece.isWhite(), rawMovesBuffer);

        foreach (var move in rawMovesBuffer)
        {
            if (move.actionType == ActionType.Attack)
            {
                ChessPiece target = BoardManager.getChessPieceAtPosition(move.targetPos);
                if (canAttackGlobally(player, currentTurnCount) &&
                    piece.canAttack(currentTurnCount) &&
                    target.canBeAttacked(currentTurnCount) &&
                    piece.canAttackSpecificTypeChess(target.getChessType(), currentTurnCount))
                {
                    bufferList.Add(move);
                }
            }
            else
                bufferList.Add(move);
        }

        return;
    }
}
