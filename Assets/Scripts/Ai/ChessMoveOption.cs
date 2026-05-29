using System.Threading.Tasks;
using UnityEngine;

public class ChessMoveOption : AiOption
{
    private ChessPiece piece;
    private Vector2Int currentPos, targetPos;
    private float score;

    public ChessMoveOption(ChessPiece piece, Vector2Int currentPos, Vector2Int targetPos, float score)
    {
        this.piece = piece;
        this.currentPos = currentPos;
        this.targetPos = targetPos;
        this.score = score;
    }
    public float getScore() => score;

    public async Task execute(PlayerData ownerData)
    {
        if (BoardManager.isEmpty(targetPos))
        {
            ownerData.doChessMoved(piece, currentPos, targetPos, ActionType.Move, GameManager.getCurrentTurnCount());
            await BoardManager.movePieceWithAnimation(currentPos, targetPos);
        }
        else
        {
            ownerData.doChessMoved(piece, currentPos, targetPos, ActionType.Attack, GameManager.getCurrentTurnCount());
            await BoardManager.attackPieceWithAnimation(currentPos, targetPos);
        }
    }

    public string getChessName() => piece.getName();
}