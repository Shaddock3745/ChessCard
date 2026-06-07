using Unity.VisualScripting;
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
    private static int serialNumber = 0;
    private readonly bool isVirtual;
    private readonly int uniqueId;
    private ChessType chessType;
    private bool isWhiteChess;
    private Vector2Int currentPosition;
    private Vector2Int initialPosition;
    private MovementType defaultMovementType;
    private ChessStatusContainer statusContainer;

    private static readonly ChessType[] ALL_TYPES = { ChessType.Pawn, ChessType.Rook, ChessType.Knight, ChessType.Bishop, ChessType.Queen, ChessType.King };
    private static readonly ChessType[] ALL_TYPES_EXCEPT_KING = { ChessType.Pawn, ChessType.Rook, ChessType.Knight, ChessType.Bishop, ChessType.Queen };

    public static ChessType[] getAllChessTypes(bool exceptKing) => exceptKing ? ALL_TYPES_EXCEPT_KING : ALL_TYPES;

    private static int getNextSerialNumber() => System.Threading.Interlocked.Increment(ref serialNumber);

    public ChessPiece(ChessType chessType, bool isWhite, Vector2Int currentPosition, bool isVirtual = false)
    {
        this.uniqueId = getNextSerialNumber();
        this.chessType = chessType;
        this.isWhiteChess = isWhite;
        this.currentPosition = currentPosition;
        this.isVirtual = isVirtual;
        this.initialPosition = currentPosition;
        this.statusContainer = new ChessStatusContainer();

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

    private ChessPiece(ChessPiece chessPieceToClone)
    {
        this.uniqueId = chessPieceToClone.uniqueId;
        this.chessType = chessPieceToClone.chessType;
        this.isWhiteChess = chessPieceToClone.isWhiteChess;
        this.currentPosition = chessPieceToClone.currentPosition;
        this.initialPosition = chessPieceToClone.initialPosition;
        this.isVirtual = true;
        this.defaultMovementType = chessPieceToClone.defaultMovementType;
        this.statusContainer = chessPieceToClone.statusContainer.Clone();
    }

    public ChessPiece Clone() => new ChessPiece(this);

    public int getUniqueId() => uniqueId;

    public bool IsVirtual() => isVirtual;

    public ChessType getChessType() => chessType;

    public bool isWhite() => isWhiteChess;

    public Vector2Int getPosition() => currentPosition;

    public void setInitialPosition(Vector2Int pos)
    {
        this.initialPosition = pos;
    }

    public Vector2Int getInitialPosition() => initialPosition;

    public void setPosition(Vector2Int newPosition)
    {
        currentPosition = newPosition;
    }

    public int getMoveCost(int currentTurnCount)
    {
        if (statusContainer.isNonCostOnMove(currentTurnCount))
            return 0;
        else
            return 1 + statusContainer.getExtraMoveCost(currentTurnCount);
    }

    public string getName() => getChessColorName(isWhiteChess) + getChessTypeName(chessType);


    public string getDescription() => defaultMovementType.getDescription();


    public bool canAttack(int currentTurnCount) => statusContainer.canAttack(currentTurnCount);


    public bool canAttackSpecificTypeChess(ChessType chessType, int currentTotalTurn) => statusContainer.canAttackSpecificTypeChess(chessType, currentTotalTurn);


    public bool canBeAttacked(int currentTurnCount) => statusContainer.canBeAttacked(currentTurnCount);


    public bool canMove(int currentTurnCount) => statusContainer.canMove(currentTurnCount);


    public int getExtraMoveCount(int currentTurnCount) => statusContainer.getExtraMoveCount(currentTurnCount);


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

    public float getStatusContainerAiValue(bool isLocal, int currentTurnCount) => statusContainer.getStatusContainerAiValue(isLocal, this, currentTurnCount);


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

    public MovementType getMovementType(int currentTurnCount)
    {
        MovementType overrideMovementType = statusContainer.getMovementOverride(currentTurnCount);

        if (overrideMovementType != null)
            return overrideMovementType;

        return defaultMovementType;
    }

    public void onMoveEnd(Vector2Int from, Vector2Int to, int currentTurnCount)
    {
        statusContainer.triggerPostMove(this, from, to, currentTurnCount);
    }

    public void onAttackEnd(Vector2Int from, ChessPiece target, int currentTurnCount)
    {
        statusContainer.triggerPostAttack(this, target, from, currentTurnCount);
    }

    public ChessStatusContainer getStatusContainer() => statusContainer;
}
