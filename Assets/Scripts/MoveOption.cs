using UnityEngine;

public class MoveOption
{
    public ChessPiece piece;
    public Vector2Int targetPos;
    public float score;

    public MoveOption(ChessPiece piece, Vector2Int targetPos, float score)
    {
        this.piece = piece;
        this.targetPos = targetPos;
        this.score = score;
    }
}