using UnityEngine;

public class MoveOption
{
    public ChessPiece piece;
    public Vector2Int currentPos, targetPos;
    public float score;

    public MoveOption(ChessPiece piece, Vector2Int currentPos, Vector2Int targetPos, float score)
    {
        this.piece = piece;
        this.currentPos = currentPos;
        this.targetPos = targetPos;
        this.score = score;
    }
}