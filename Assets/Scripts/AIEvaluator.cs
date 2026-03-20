using System.Collections.Generic;
using UnityEngine;

public class AIEvaluator
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

    public static float evaluateMove(ChessPiece piece, MoveAction action)
    {
        float score = 0f;

        if (action.actionType == ActionType.Attack)
            score += getPieceValue(BoardManager.getChessPieceAtPosition(action.targetPos)) * ATTACK_COEFFICIENT;

        score += (BOARD_WIDTH - Vector2.Distance(action.targetPos, CENTER_POSITION)) * DISTANCE_COEFFICIENT;

        return score;
    }

    public static List<MoveOption> getMoveByChessPiece(ChessPiece piece, Vector2Int piecePos)
    {
        List<MoveAction> moveActions = piece.getAvailableMoves(piecePos);
        List<MoveOption> moveOptions = new List<MoveOption>();

        foreach (MoveAction moveAction in moveActions)
            moveOptions.Add(new MoveOption(piece, piecePos, moveAction.targetPos, evaluateMove(piece, moveAction)));

        return moveOptions;
    }
}
