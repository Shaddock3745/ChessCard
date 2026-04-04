using System.Threading.Tasks;
using UnityEngine;

public class ChessMoveOption : AiOption
{
    private Vector2Int currentPos, targetPos;
    private float score;

    public ChessMoveOption(Vector2Int currentPos, Vector2Int targetPos, float score)
    {
        this.currentPos = currentPos;
        this.targetPos = targetPos;
        this.score = score;
    }
    public float getScore() => score;

    public async Task execute()
    {
        if (BoardManager.isEmpty(targetPos))
            await BoardManager.movePieceWithAnimation(currentPos, targetPos);
        else
            await BoardManager.attackPieceWithAnimation(currentPos, targetPos);
    }

}