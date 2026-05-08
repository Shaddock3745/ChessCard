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
    private ChessStatusContainer statusContainer = new ChessStatusContainer();

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
        return getChessColorName(isWhiteChess) + getChessTypeName(chessType);
    }

    public string getDescription()
    {
        return movementType.getDescription();
    }

    public List<MoveAction> getAvailableMoves()
    {
        return movementType.getAvailableMoves(currentPosition, isWhiteChess);
    }

    private static string getChessColorName(bool isWhite)
    {
        return GameManager.getCurrentLanguage() switch
        {
            Language.TRADITIONAL_CHINESE => isWhite ? "白" : "黑",
            Language.ENGLISH => isWhite ? "White" : "Black",
            Language.JAPANESE => isWhite ? "白" : "黒",
            _ => isWhite ? "White" : "Black"
        };
    }

    public void addStatus(ChessStatus status)
    {
        statusContainer.addStatus(status);
    }

    public void cleanUpExpiredStatuses(int currentTotalTurn) 
    {
        statusContainer.cleanUpExpiredStatuses(currentTotalTurn);
    }

    private static string getChessTypeName(ChessType type)
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return type switch
                {
                    ChessType.Pawn => "士兵",
                    ChessType.Rook => "城堡",
                    ChessType.Knight => "騎士",
                    ChessType.Bishop => "主教",
                    ChessType.Queen => "皇后",
                    ChessType.King => "國王",
                    _ => "未知棋子"
                };
            case Language.ENGLISH:
                return type switch
                {
                    ChessType.Pawn => "Pawn",
                    ChessType.Rook => "Rook",
                    ChessType.Knight => "Knight",
                    ChessType.Bishop => "Bishop",
                    ChessType.Queen => "Queen",
                    ChessType.King => "King",
                    _ => "Unknown Piece"
                };
            case Language.JAPANESE:
                return type switch
                {
                    ChessType.Pawn => "ポーン",
                    ChessType.Rook => "ルーク",
                    ChessType.Knight => "ナイト",
                    ChessType.Bishop => "ビショップ",
                    ChessType.Queen => "クイーン",
                    ChessType.King => "キング",
                    _ => "不明な駒"
                };
            default:
                return type.ToString();
        }
    }
}
