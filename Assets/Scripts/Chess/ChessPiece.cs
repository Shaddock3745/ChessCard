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
    private MovementType defaultMovementType;
    private ChessStatusContainer statusContainer = new ChessStatusContainer();

    private static readonly ChessType[] ALL_TYPES = { ChessType.Pawn, ChessType.Rook, ChessType.Knight, ChessType.Bishop, ChessType.Queen, ChessType.King };
    private static readonly ChessType[] ALL_TYPES_EXCEPT_KING = { ChessType.Pawn, ChessType.Rook, ChessType.Knight, ChessType.Bishop, ChessType.Queen };

    public static ChessType[] getAllChessTypes(bool exceptKing) => exceptKing ? ALL_TYPES_EXCEPT_KING : ALL_TYPES;

    public ChessPiece(ChessType chessType, bool isWhite, Vector2Int currentPosition)
    {
        this.chessType = chessType;
        this.isWhiteChess = isWhite;
        this.currentPosition = currentPosition;

        defaultMovementType = chessType switch
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

    public int getMoveCost(int currentTurnCount)
    {
        return 1 + statusContainer.getExtraMoveCost(currentTurnCount);
    }

    public string getName()
    {
        return getChessColorName(isWhiteChess) + getChessTypeName(chessType);
    }

    public string getDescription()
    {
        return defaultMovementType.getDescription();
    }

    public List<MoveAction> getAvailableMoves(bool canAttack, int currentTurnCount)
    {
        MovementType overrideMovementType = statusContainer.getMovementOverride(currentTurnCount);

        if (overrideMovementType != null)
            return overrideMovementType.getAvailableMoves(currentPosition, isWhiteChess, canAttack);

        return defaultMovementType.getAvailableMoves(currentPosition, isWhiteChess, canAttack);
    }

    public bool canAttack(int currentTurnCount)
    {
        return statusContainer.canMove(currentTurnCount) && statusContainer.canAttack(currentTurnCount);
    }

    public bool canBeAttacked(int currentTurnCount)
    {
        return statusContainer.canBeAttacked(currentTurnCount);
    }

    public bool canMove(int currentTurnCount)
    {
        return statusContainer.canMove(currentTurnCount);
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

    public void onDeath()
    {
        statusContainer.Dispose();
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
