using UnityEngine;

public enum ChessType
{
    Pawn = 0,
    Rook = 1,
    Knight = 2,
    Bishop = 3,
    Queen = 4,
    King = 5
}

[CreateAssetMenu(fileName = "NewPiece", menuName = "Chess/PieceData")]
public class PieceData: ScriptableObject
{    

    [SerializeField] private Sprite pieceSprite;
    [SerializeField] private bool isWhiteChess;
    [SerializeField] private ChessType chessType;

    public Sprite getPieceSprite() => pieceSprite;
    public bool isWhite() => isWhiteChess;
    public ChessType getChessType() => chessType;
}
