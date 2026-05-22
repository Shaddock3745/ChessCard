using System.Collections.Generic;
using UnityEngine;

public class ActionValidator
{
    private static readonly int MAX_MOVE_COUNT_PER_TURN = 1;
    private static readonly List<MoveAction> rawMovesBuffer = new List<MoveAction>(64);

    public static bool canUseCard(PlayerData player, Card card, int currentTurnCount)
    {
        int extraCost = player.getStatusContainer().getGlobalExtraCardCost(currentTurnCount);
        int finalCost = card.getCost() + extraCost;

        return player.getMana() >= finalCost
            && player.getStatusContainer().canUseCard(card.getId(), currentTurnCount)
            && CardEffectManager.canActivateCard(player.IsLocal(), card, currentTurnCount);
    }

    public static bool canAttackGlobally(PlayerData player, int currentTurnCount)
    {
        return !reachMoveLimit(player) && player.getStatusContainer().canAttack(currentTurnCount);
    }

    public static bool reachMoveLimit(PlayerData player)
    {
        return player.getMoveCount() >= MAX_MOVE_COUNT_PER_TURN;
    }

    public static bool canSummon(PlayerData player, int currentTurnCount)
    {
        return player.getStatusContainer().canSummon(currentTurnCount);
    }

    public static void getLegalMoves(PlayerData player, ChessPiece piece, int currentTurnCount, List<MoveAction> bufferList)
    {
        bufferList.Clear();

        if (reachMoveLimit(player) || piece.isWhite() != player.IsWhite())
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
