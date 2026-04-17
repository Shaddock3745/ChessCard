using System.Collections.Generic;
using UnityEngine;

public class AiEvaluator
{
    private static readonly float ATTACK_COEFFICIENT = 10f;
    private static readonly float DISTANCE_COEFFICIENT = 1.5f;
    private static readonly float BOARD_WIDTH = 8f;
    private static readonly Vector2 CENTER_POSITION = new Vector2(3.5f, 3.5f);

    public static float getPieceValue(ChessType pieceType)
    {
        return pieceType switch
        {
            ChessType.Pawn => 1f,
            ChessType.Knight => 3f,
            ChessType.Bishop => 3f,
            ChessType.Rook => 5f,
            ChessType.Queen => 10f,
            ChessType.King => 20f,
            _ => 0f
        };
    }

    public static float getPositionValue(Vector2Int position)
    {
        return (BOARD_WIDTH - Vector2.Distance(position, CENTER_POSITION)) * DISTANCE_COEFFICIENT;
    }

    private static float evaluateChessMove(MoveAction action)
    {
        float score = 0f;

        if (action.actionType == ActionType.Attack)
            score += getPieceValue(BoardManager.getChessPieceAtPosition(action.targetPos).getChessType()) * ATTACK_COEFFICIENT;

        score += getPositionValue(action.targetPos);

        return score;
    }

    public static List<ChessMoveOption> getMoveOptionsByChessPiece(ChessPiece piece)
    {

        List<MoveAction> moveActions = piece.getAvailableMoves();
        List<ChessMoveOption> moveOptions = new List<ChessMoveOption>();

        foreach (MoveAction moveAction in moveActions)
            moveOptions.Add(new ChessMoveOption(piece, piece.getPosition(), moveAction.targetPos, evaluateChessMove(moveAction)));

        return moveOptions;
    }

    public static List<CardUseOption> getCardUseOptionsByCard(PlayerData playerData, Card card, int currentMana)
    {
        CardEffect cardEffect = CardEffectManager.getCardEffect(card);

        if (cardEffect != null)
            return cardEffect.getUseOptions(playerData.IsLocal(), card, currentMana);
        else
            return new List<CardUseOption>();
    }
}
