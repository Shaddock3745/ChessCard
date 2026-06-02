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

    public static bool canHoldCard(PlayerData playerData)
    {
        return playerData.canHoldCard();
    }

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
        return player.getStatusContainer().canAttack(currentTurnCount);
    }

    private static int calculateGlobalMovesUsed(List<ActionRecord> records, int currentTurnCount)
    {
        var movesByPiece = records
            .GroupBy(r => r.piece);

        int globalMovesUsed = 0;
        foreach (var group in movesByPiece)
        {
            ChessPiece movedPiece = group.Key;
            globalMovesUsed += Mathf.Max(0, group.Count() - movedPiece.getExtraMoveCount(currentTurnCount));
        }
        return globalMovesUsed;
    }
    public static bool reachMoveLimit(PlayerData player, int currentTurnCount)
    {
        return calculateGlobalMovesUsed(player.getChessMoveRecords(), currentTurnCount) >= MAX_MOVE_COUNT_PER_TURN + player.getStatusContainer().getExtraMoveCount(currentTurnCount);
    }

    public static bool canPieceMoveThisTurn(PlayerData player, ChessPiece piece, int currentTurnCount)
    {
        if (player.getStatusContainer().isLimitedToMoveSpecificChess(currentTurnCount))
            if (player.getStatusContainer().getLimitedToMoveSpecificChess(currentTurnCount) != piece)
                return false;

        List<ActionRecord> records = player.getChessMoveRecords();
        int pieceMoves = records.Count(r => r.piece == piece && r.type == ActionType.Move);
        int pieceBonusMoves = piece.getExtraMoveCount(currentTurnCount);

        if (pieceMoves >= MAX_MOVE_COUNT_PER_TURN + pieceBonusMoves)
            return false;

        if (pieceMoves < pieceBonusMoves)
            return true;

        return calculateGlobalMovesUsed(records, currentTurnCount) < MAX_MOVE_COUNT_PER_TURN + player.getStatusContainer().getExtraMoveCount(currentTurnCount);
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
