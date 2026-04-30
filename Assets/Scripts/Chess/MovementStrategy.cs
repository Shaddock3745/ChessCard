using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    Move = 0,
    Attack = 1,
}

public struct MoveAction
{
    public Vector2Int targetPos;
    public ActionType actionType;
}


public interface MovementType
{
    public List<MoveAction> getAvailableMoves(Vector2Int currentPos, bool isWhite);

    public List<ChessPiece> getProtectablePieces(Vector2Int targetPos, bool isWhite);

    public string getDescription();
}

public class PawnMovement : MovementType
{
    public List<MoveAction> getAvailableMoves(Vector2Int currentPos, bool isWhite)
    {
        List<MoveAction> moves = new List<MoveAction>();
        int direction = isWhite ? 1 : -1;
        Vector2Int temp;

        // Move forward
        temp = new Vector2Int(currentPos.x, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp) && BoardManager.isEmpty(temp))
        {
            moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });

            // First move can be two squares
            if ((isWhite && currentPos.y == 1) || (!isWhite && currentPos.y == 6))
            {
                temp = new Vector2Int(currentPos.x, currentPos.y + 2 * direction);

                if (BoardManager.isPositionOnBoard(temp) && BoardManager.isEmpty(temp))
                    moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
            }
        }

        // Attack diagonally
        temp = new Vector2Int(currentPos.x + 1, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp) && !BoardManager.isEmpty(temp) && (isWhite ^ BoardManager.isWhiteAtPosition(temp)))
            moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });

        temp = new Vector2Int(currentPos.x - 1, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp) && !BoardManager.isEmpty(temp) && (isWhite ^ BoardManager.isWhiteAtPosition(temp)))
            moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });

        return moves;
    }

    public List<ChessPiece> getProtectablePieces(Vector2Int targetPos, bool isWhite)
    {
        List<ChessPiece> protectablePieces = new List<ChessPiece>();

        // Pawns can protect pieces diagonally in front of them
        int direction = isWhite ? 1 : -1;
        Vector2Int leftProtectPos = new Vector2Int(targetPos.x - 1, targetPos.y + direction);
        Vector2Int rightProtectPos = new Vector2Int(targetPos.x + 1, targetPos.y + direction);

        if (BoardManager.isPositionOnBoard(leftProtectPos) && !BoardManager.isEmpty(leftProtectPos) && BoardManager.isWhiteAtPosition(leftProtectPos) == isWhite)
            protectablePieces.Add(BoardManager.getChessPieceAtPosition(leftProtectPos));
        if (BoardManager.isPositionOnBoard(rightProtectPos) && !BoardManager.isEmpty(rightProtectPos) && BoardManager.isWhiteAtPosition(rightProtectPos) == isWhite)
            protectablePieces.Add(BoardManager.getChessPieceAtPosition(rightProtectPos));

        return protectablePieces;
    }

    public string getDescription()
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return "士兵只能向前移動一格，第一步可以選擇移動兩格。士兵只能斜向前攻擊對方棋子。";
            case Language.ENGLISH:
                return "Pawns can only move forward one square, with the option to move two squares on their first move. Pawns can only attack diagonally forward.";
            case Language.JAPANESE:
                return "ポーンは前方に1マスしか動けませんが、最初の一手では2マス動くこともできます。ポーンは斜め前にしか攻撃できません。";
            default:
                return "";
        }
    }
}

public class RookMovement : MovementType
{
    private static readonly Vector2Int[] rookMoveDirections = new Vector2Int[]
    {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
    };

    public List<MoveAction> getAvailableMoves(Vector2Int currentPos, bool isWhite)
    {
        List<MoveAction> moves = new List<MoveAction>();

        foreach (var dir in rookMoveDirections)
        {
            Vector2Int temp = currentPos;
            while (true)
            {
                temp += dir;
                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                if (BoardManager.isEmpty(temp))
                    moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                else
                {
                    if (isWhite ^ BoardManager.isWhiteAtPosition(temp))
                        moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });

                    break;
                }
            }
        }

        return moves;
    }

    public List<ChessPiece> getProtectablePieces(Vector2Int targetPos, bool isWhite)
    {
        List<ChessPiece> protectablePieces = new List<ChessPiece>();

        // Rooks can protect pieces in straight lines
        Vector2Int[] directions = new Vector2Int[]
        {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down
        };

        foreach (var dir in directions)
        {
            Vector2Int temp = targetPos;
            while (true)
            {
                temp += dir;
                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                if (!BoardManager.isEmpty(temp))
                {
                    if (BoardManager.isWhiteAtPosition(temp) == isWhite)
                        protectablePieces.Add(BoardManager.getChessPieceAtPosition(temp));
                    break;
                }
            }
        }

        return protectablePieces;
    }

    public string getDescription()
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return "城堡可以水平或垂直移動任意格數，直到被其他棋子阻擋。城堡可以攻擊在同一直線上且沒有其他棋子阻擋的對方棋子。";
            case Language.ENGLISH:
                return "Rooks can move any number of squares horizontally or vertically until blocked by another piece. Rooks can attack opponent pieces that are in the same line and not blocked by other pieces.";
            case Language.JAPANESE:
                return "ルークは水平または垂直に他の駒に阻まれるまで任意のマス数動けます。ルークは同じ列にいて他の駒に阻まれていない相手の駒を攻撃できます。";
            default:
                return "";
        }
    }
}

public class KnightMovement : MovementType
{
    private static readonly Vector2Int[] knightMoves = new Vector2Int[]
    {
            new Vector2Int(1, 2),
            new Vector2Int(1, -2),
            new Vector2Int(-1, 2),
            new Vector2Int(-1, -2),
            new Vector2Int(2, 1),
            new Vector2Int(2, -1),
            new Vector2Int(-2, 1),
            new Vector2Int(-2, -1)
    };

    public List<MoveAction> getAvailableMoves(Vector2Int currentPos, bool isWhite)
    {
        List<MoveAction> moves = new List<MoveAction>();

        foreach (var move in knightMoves)
        {
            Vector2Int temp = currentPos + move;
            if (BoardManager.isPositionOnBoard(temp))
            {
                if (BoardManager.isEmpty(temp))
                    moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                else if (isWhite ^ BoardManager.isWhiteAtPosition(temp))
                    moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
            }
        }
        return moves;
    }

    public List<ChessPiece> getProtectablePieces(Vector2Int targetPos, bool isWhite)
    {
        List<ChessPiece> protectablePieces = new List<ChessPiece>();
        // Knights can protect pieces in an L shape
        Vector2Int[] knightMoves = new Vector2Int[]
        {
                new Vector2Int(1, 2),
                new Vector2Int(1, -2),
                new Vector2Int(-1, 2),
                new Vector2Int(-1, -2),
                new Vector2Int(2, 1),
                new Vector2Int(2, -1),
                new Vector2Int(-2, 1),
                new Vector2Int(-2, -1)
        };

        foreach (var move in knightMoves)
        {
            Vector2Int temp = targetPos + move;
            if (BoardManager.isPositionOnBoard(temp) && !BoardManager.isEmpty(temp) && BoardManager.isWhiteAtPosition(temp) == isWhite)
                protectablePieces.Add(BoardManager.getChessPieceAtPosition(temp));
        }

        return protectablePieces;
    }

    public string getDescription()
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return "騎士以L形移動：水平或垂直移動兩格後再水平或垂直移動一格。騎士可以跳過其他棋子，攻擊在目的地的對方棋子。";
            case Language.ENGLISH:
                return "Knights move in an L shape: two squares horizontally or vertically, then one square perpendicular. Knights can jump over other pieces and attack opponent pieces on the destination square.";
            case Language.JAPANESE:
                return "ナイトはL字型に動きます：水平または垂直に2マス、その後垂直または水平に1マス。ナイトは他の駒を飛び越えることができ、目的地の相手の駒を攻撃できます。";
            default:
                return "";
        }
    }
}

public class BishopMovement : MovementType
{
    private static readonly Vector2Int[] bishopMoveDirections = new Vector2Int[]
    {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
    };
    public List<MoveAction> getAvailableMoves(Vector2Int currentPos, bool isWhite)
    {
        List<MoveAction> moves = new List<MoveAction>();

        foreach (var dir in bishopMoveDirections)
        {
            Vector2Int temp = currentPos;
            while (true)
            {
                temp += dir;
                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                if (BoardManager.isEmpty(temp))
                    moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                else
                {
                    if (isWhite ^ BoardManager.isWhiteAtPosition(temp))
                        moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });

                    break;
                }
            }
        }
        return moves;
    }

    public List<ChessPiece> getProtectablePieces(Vector2Int targetPos, bool isWhite)
    {
        List<ChessPiece> protectablePieces = new List<ChessPiece>();
        // Bishops can protect pieces diagonally
        Vector2Int[] directions = new Vector2Int[]
        {
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1)
        };
        foreach (var dir in directions)
        {
            Vector2Int temp = targetPos;
            while (true)
            {
                temp += dir;
                if (!BoardManager.isPositionOnBoard(temp))
                    break;
                if (!BoardManager.isEmpty(temp))
                {
                    if (BoardManager.isWhiteAtPosition(temp) == isWhite)
                        protectablePieces.Add(BoardManager.getChessPieceAtPosition(temp));
                    break;
                }
            }
        }
        return protectablePieces;
    }

    public string getDescription()
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return "主教可以沿對角線移動任意格數，直到被其他棋子阻擋。主教可以攻擊在同一對角線上且沒有其他棋子阻擋的對方棋子。";
            case Language.ENGLISH:
                return "Bishops can move any number of squares diagonally until blocked by another piece. Bishops can attack opponent pieces that are on the same diagonal and not blocked by other pieces.";
            case Language.JAPANESE:
                return "ビショップは斜めに他の駒に阻まれるまで任意のマス数動けます。ビショップは同じ斜めにいて他の駒に阻まれていない相手の駒を攻撃できます。";
            default:
                return "";
        }
    }
}

public class QueenMovement : MovementType
{
    private static readonly Vector2Int[] queenMoveDirections = new Vector2Int[]
    {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down,
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
    };
    public List<MoveAction> getAvailableMoves(Vector2Int currentPos, bool isWhite)
    {
        List<MoveAction> moves = new List<MoveAction>();
        foreach (var dir in queenMoveDirections)
        {
            Vector2Int temp = currentPos;
            while (true)
            {
                temp += dir;
                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                if (BoardManager.isEmpty(temp))
                    moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                else
                {
                    if (isWhite ^ BoardManager.isWhiteAtPosition(temp))
                        moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });

                    break;
                }
            }
        }
        return moves;
    }

    public List<ChessPiece> getProtectablePieces(Vector2Int targetPos, bool isWhite)
    {
        List<ChessPiece> protectablePieces = new List<ChessPiece>();
        // Queens can protect pieces in straight lines and diagonals
        Vector2Int[] directions = new Vector2Int[]
        {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down,
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1)
        };
        foreach (var dir in directions)
        {
            Vector2Int temp = targetPos;
            while (true)
            {
                temp += dir;
                if (!BoardManager.isPositionOnBoard(temp))
                    break;
                if (!BoardManager.isEmpty(temp))
                {
                    if (BoardManager.isWhiteAtPosition(temp) == isWhite)
                        protectablePieces.Add(BoardManager.getChessPieceAtPosition(temp));
                    break;
                }
            }
        }
        return protectablePieces;
    }

    public string getDescription()
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return "皇后可以水平、垂直或對角線移動任意格數，直到被其他棋子阻擋。皇后可以攻擊在同一直線或對角線上且沒有其他棋子阻擋的對方棋子。";
            case Language.ENGLISH:
                return "Queens can move any number of squares horizontally, vertically, or diagonally until blocked by another piece. Queens can attack opponent pieces that are in the same line or diagonal and not blocked by other pieces.";
            case Language.JAPANESE:
                return "クイーンは水平、垂直、または斜めに他の駒に阻まれるまで任意のマス数動けます。クイーンは同じ列、行、または斜めにいて他の駒に阻まれていない相手の駒を攻撃できます。";
            default:
                return "";
        }
    }
}

public class KingMovement : MovementType
{
    private static readonly Vector2Int[] kingMoves = new Vector2Int[]
    {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down,
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
    };
    public List<MoveAction> getAvailableMoves(Vector2Int currentPos, bool isWhite)
    {
        List<MoveAction> moves = new List<MoveAction>();
        foreach (var move in kingMoves)
        {
            Vector2Int temp = currentPos + move;
            if (BoardManager.isPositionOnBoard(temp))
            {
                if (BoardManager.isEmpty(temp))
                    moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                else if (isWhite ^ BoardManager.isWhiteAtPosition(temp))
                    moves.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
            }
        }
        return moves;
    }

    public List<ChessPiece> getProtectablePieces(Vector2Int targetPos, bool isWhite)
    {
        List<ChessPiece> protectablePieces = new List<ChessPiece>();
        // Kings can protect pieces in adjacent squares
        Vector2Int[] kingMoves = new Vector2Int[]
        {
                Vector2Int.right,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.down,
                new Vector2Int(1, 1),
                new Vector2Int(1, -1),
                new Vector2Int(-1, 1),
                new Vector2Int(-1, -1)
        };
        foreach (var move in kingMoves)
        {
            Vector2Int temp = targetPos + move;
            if (BoardManager.isPositionOnBoard(temp) && !BoardManager.isEmpty(temp) && BoardManager.isWhiteAtPosition(temp) == isWhite)
                protectablePieces.Add(BoardManager.getChessPieceAtPosition(temp));
        }
        return protectablePieces;
    }

    public string getDescription()
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return "國王可以移動到任何相鄰的格子（水平、垂直或對角線）。國王可以攻擊在相鄰格子的對方棋子。";
            case Language.ENGLISH:
                return "Kings can move to any adjacent square (horizontally, vertically, or diagonally). Kings can attack opponent pieces in adjacent squares.";
            case Language.JAPANESE:
                return "キングは隣接するマス（水平、垂直、または斜め）に移動できます。キングは隣接するマスの相手の駒を攻撃できます。";
            default:
                return "";
        }
    }
}

