using System.Collections.Generic;
using UnityEngine;

public class AiEvaluator
{
    private static readonly float ATTACK_COEFFICIENT = 10f;
    private static readonly float DISTANCE_COEFFICIENT = 1.5f;
    private static readonly float BOARD_WIDTH = 8f;
    private static readonly Vector2 CENTER_POSITION = new Vector2(3.5f, 3.5f);

    private static float getPieceValue(ChessPiece piece)
    {
        return piece.getChessType() switch
        {
            ChessType.Pawn => 1f,
            ChessType.Knight => 3f,
            ChessType.Bishop => 3f,
            ChessType.Rook => 5f,
            ChessType.Queen => 9f,
            ChessType.King => 20f,
            _ => 0f
        };
    }

    private static float evaluateChessMove(MoveAction action)
    {
        float score = 0f;

        if (action.actionType == ActionType.Attack)
            score += getPieceValue(BoardManager.getChessPieceAtPosition(action.targetPos)) * ATTACK_COEFFICIENT;

        score += (BOARD_WIDTH - Vector2.Distance(action.targetPos, CENTER_POSITION)) * DISTANCE_COEFFICIENT;

        return score;
    }

    public static List<ChessMoveOption> getMoveOptionsByChessPiece(Vector2Int piecePos)
    {
        List<MoveAction> moveActions = BoardManager.getChessPieceAtPosition(piecePos).getAvailableMoves(piecePos);
        List<ChessMoveOption> moveOptions = new List<ChessMoveOption>();

        foreach (MoveAction moveAction in moveActions)
            moveOptions.Add(new ChessMoveOption(piecePos, moveAction.targetPos, evaluateChessMove(moveAction)));

        return moveOptions;
    }

    public static List<CardUseOption> getCardUseOptionsByCard(Card card)
    {
        List<CardUseOption> useOptions = new List<CardUseOption>();


        return useOptions;
    }
}
