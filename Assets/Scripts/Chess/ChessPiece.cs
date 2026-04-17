using System.Collections.Generic;
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

public class ChessPiece
{
    private ChessType chessType;
    private bool isWhiteChess;
    private Vector2Int currentPosition;
    private MovementType movementType;
    private int extraMoveCost = 0;

    public ChessPiece(ChessType chessType, bool isWhite, Vector2Int currentPosition)
    {
        this.chessType = chessType;
        this.isWhiteChess = isWhite;
        this.currentPosition = currentPosition;

        movementType = chessType switch
        {
            ChessType.Pawn => new PawnMovement(),
            ChessType.Rook => new RookMovement(),
            ChessType.Bishop => new BishopMovement(),
            ChessType.Knight => new KnightMovement(),
            ChessType.Queen => new QueenMovement(),
            ChessType.King => new KingMovement(),
            _ => null
        };
    }

    public ChessType getChessType() => chessType;

    public bool isWhite() => isWhiteChess;

    public Vector2Int getPosition() => currentPosition;

    public void setPosition(Vector2Int newPosition)
    {
        currentPosition = newPosition;
    }

    public int getExtraMoveCost() => extraMoveCost;

    public void setExtraMoveCost(int cost)
    {
        extraMoveCost = cost;
    }

    public string getName()
    {
        return (isWhiteChess ? "White " : "Black ") + chessType.ToString();
    }

    public List<MoveAction> getAvailableMoves()
    {
        return movementType.getAvailableMoves(currentPosition, isWhiteChess);
    }
}
