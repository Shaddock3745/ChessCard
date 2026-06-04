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
    // 加入 BoardState 參數，阻絕讀取全域真實棋盤
    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList);
    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList);
    public string getDescription();
    public float getAiValue();
}

public class PawnMovement : MovementType
{
    public float getAiValue() => 10f;

    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList)
    {
        int direction = isWhite ? 1 : -1;
        Vector2Int temp;

        // Move forward
        temp = new Vector2Int(currentPos.x, currentPos.y + direction);

        if (BoardManager.isPositionOnBoard(temp) && state.getChessPieceAtPosition(temp) == null)
        {
            bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });

            // First move can be two squares
            if ((isWhite && currentPos.y == 1) || (!isWhite && currentPos.y == 6))
            {
                temp = new Vector2Int(currentPos.x, currentPos.y + 2 * direction);

                if (BoardManager.isPositionOnBoard(temp) && state.getChessPieceAtPosition(temp) == null)
                    bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
            }
        }

        // Attack diagonally
        temp = new Vector2Int(currentPos.x + 1, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp))
        {
            ChessPiece piece = state.getChessPieceAtPosition(temp);
            if (piece != null && piece.isWhite() != isWhite)
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
        }

        temp = new Vector2Int(currentPos.x - 1, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp))
        {
            ChessPiece piece = state.getChessPieceAtPosition(temp);
            if (piece != null && piece.isWhite() != isWhite)
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
        }
    }

    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList)
    {
        int direction = isWhite ? 1 : -1;
        Vector2Int leftProtectPos = new Vector2Int(targetPos.x - 1, targetPos.y + direction);
        Vector2Int rightProtectPos = new Vector2Int(targetPos.x + 1, targetPos.y + direction);

        if (BoardManager.isPositionOnBoard(leftProtectPos))
        {
            ChessPiece piece = state.getChessPieceAtPosition(leftProtectPos);
            if (piece != null && piece.isWhite() == isWhite)
                bufferList.Add(piece);
        }

        if (BoardManager.isPositionOnBoard(rightProtectPos))
        {
            ChessPiece piece = state.getChessPieceAtPosition(rightProtectPos);
            if (piece != null && piece.isWhite() == isWhite)
                bufferList.Add(piece);
        }
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

public class FrontalBreakthroughPawnMovement : MovementType
{
    public float getAiValue() => 15f;

    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList)
    {
        int direction = isWhite ? 1 : -1;
        Vector2Int temp;

        temp = new Vector2Int(currentPos.x, currentPos.y + direction);

        if (BoardManager.isPositionOnBoard(temp))
        {
            ChessPiece piece = state.getChessPieceAtPosition(temp);
            if (piece == null)
            {
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });

                if ((isWhite && currentPos.y == 1) || (!isWhite && currentPos.y == 6))
                {
                    Vector2Int temp2 = new Vector2Int(currentPos.x, currentPos.y + 2 * direction);

                    if (BoardManager.isPositionOnBoard(temp2) && state.getChessPieceAtPosition(temp2) == null)
                        bufferList.Add(new MoveAction { targetPos = temp2, actionType = ActionType.Move });
                }
            }
            else if (piece.isWhite() != isWhite)
            {
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
            }
        }

        temp = new Vector2Int(currentPos.x + 1, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp))
        {
            ChessPiece piece = state.getChessPieceAtPosition(temp);
            if (piece != null && piece.isWhite() != isWhite)
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
        }

        temp = new Vector2Int(currentPos.x - 1, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp))
        {
            ChessPiece piece = state.getChessPieceAtPosition(temp);
            if (piece != null && piece.isWhite() != isWhite)
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
        }
    }

    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList)
    {
        int direction = isWhite ? 1 : -1;

        Vector2Int[] protectPosArray = new Vector2Int[]
        {
            new Vector2Int(targetPos.x - 1, targetPos.y + direction),
            new Vector2Int(targetPos.x + 1, targetPos.y + direction),
            new Vector2Int(targetPos.x, targetPos.y + direction)
        };

        foreach (var protectPos in protectPosArray)
        {
            if (BoardManager.isPositionOnBoard(protectPos))
            {
                ChessPiece piece = state.getChessPieceAtPosition(protectPos);
                if (piece != null && piece.isWhite() == isWhite)
                    bufferList.Add(piece);
            }
        }
    }

    public string getDescription()
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return "除了原有的移動與斜向攻擊外，該士兵現在可以攻擊正前方的敵方棋子。";
            case Language.ENGLISH:
                return "In addition to normal movement and diagonal attacks, this pawn can now attack enemy pieces directly in front of it.";
            case Language.JAPANESE:
                return "通常の移動と斜め攻撃に加え、このポーンは正面の敵駒を攻撃できるようになります。";
            default:
                return "";
        }
    }
}

public class BuffedPawnMovement : MovementType
{
    public float getAiValue() => 20f;

    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList)
    {
        int direction = isWhite ? 1 : -1;
        Vector2Int temp;

        temp = new Vector2Int(currentPos.x, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp) && state.getChessPieceAtPosition(temp) == null)
        {
            bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });

            Vector2Int temp2 = new Vector2Int(currentPos.x, currentPos.y + 2 * direction);
            if (BoardManager.isPositionOnBoard(temp2) && state.getChessPieceAtPosition(temp2) == null)
            {
                bufferList.Add(new MoveAction { targetPos = temp2, actionType = ActionType.Move });
            }
        }

        Vector2Int[] sides = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(-1, 0) };
        foreach (var side in sides)
        {
            temp = currentPos + side;
            if (BoardManager.isPositionOnBoard(temp) && state.getChessPieceAtPosition(temp) == null)
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
        }

        temp = new Vector2Int(currentPos.x, currentPos.y - direction);
        if (BoardManager.isPositionOnBoard(temp) && state.getChessPieceAtPosition(temp) == null)
            bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });

        temp = new Vector2Int(currentPos.x + 1, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp))
        {
            ChessPiece piece = state.getChessPieceAtPosition(temp);
            if (piece != null && piece.isWhite() != isWhite)
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
        }

        temp = new Vector2Int(currentPos.x - 1, currentPos.y + direction);
        if (BoardManager.isPositionOnBoard(temp))
        {
            ChessPiece piece = state.getChessPieceAtPosition(temp);
            if (piece != null && piece.isWhite() != isWhite)
                bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
        }
    }

    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList)
    {
        int direction = isWhite ? 1 : -1;
        Vector2Int[] protectPosArray = new Vector2Int[]
        {
            new Vector2Int(targetPos.x - 1, targetPos.y + direction),
            new Vector2Int(targetPos.x + 1, targetPos.y + direction)
        };

        foreach (var protectPos in protectPosArray)
        {
            if (BoardManager.isPositionOnBoard(protectPos))
            {
                ChessPiece piece = state.getChessPieceAtPosition(protectPos);
                if (piece != null && piece.isWhite() == isWhite)
                    bufferList.Add(piece);
            }
        }
    }

    public string getDescription()
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE:
                return "亢奮狀態的士兵可橫向或向後移動一格，或是無條件往前移動兩格。仍保持斜向前攻擊的能力。";
            case Language.ENGLISH:
                return "An Adrenaline-buffed pawn can move 1 square sideways or backwards, or move 2 squares forward unconditionally. It retains its diagonal forward attack.";
            case Language.JAPANESE:
                return "興奮状態のポーンは、横または後ろに1マス、あるいは無条件で前に2マス動けます。斜め前への攻撃能力は維持されます。";
            default:
                return "";
        }
    }
}

public class RookMovement : MovementType
{
    public float getAiValue() => 50f;

    private static readonly Vector2Int[] rookMoveDirections = new Vector2Int[]
    {
            Vector2Int.right,
            Vector2Int.left,
            Vector2Int.up,
            Vector2Int.down
    };

    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList)
    {
        foreach (var dir in rookMoveDirections)
        {
            Vector2Int temp = currentPos;

            while (true)
            {
                temp += dir;

                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                ChessPiece piece = state.getChessPieceAtPosition(temp);

                if (piece == null)
                {
                    bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                }
                else
                {
                    if (piece.isWhite() != isWhite)
                        bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });

                    break;
                }
            }
        }
    }

    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList)
    {
        foreach (var dir in rookMoveDirections)
        {
            Vector2Int temp = targetPos;

            while (true)
            {
                temp += dir;
                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                ChessPiece piece = state.getChessPieceAtPosition(temp);
                if (piece != null)
                {
                    if (piece.isWhite() == isWhite)
                        bufferList.Add(piece);

                    break;
                }
            }
        }
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
    public float getAiValue() => 30f;

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

    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList)
    {
        foreach (var move in knightMoves)
        {
            Vector2Int temp = currentPos + move;

            if (BoardManager.isPositionOnBoard(temp))
            {
                ChessPiece piece = state.getChessPieceAtPosition(temp);
                if (piece == null)
                    bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                else if (piece.isWhite() != isWhite)
                    bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
            }
        }
    }

    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList)
    {
        foreach (var move in knightMoves)
        {
            Vector2Int temp = targetPos + move;

            if (BoardManager.isPositionOnBoard(temp))
            {
                ChessPiece piece = state.getChessPieceAtPosition(temp);
                if (piece != null && piece.isWhite() == isWhite)
                    bufferList.Add(piece);
            }
        }
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
    public float getAiValue() => 30f;

    private static readonly Vector2Int[] bishopMoveDirections = new Vector2Int[]
    {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
    };

    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList)
    {
        foreach (var dir in bishopMoveDirections)
        {
            Vector2Int temp = currentPos;

            while (true)
            {
                temp += dir;

                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                ChessPiece piece = state.getChessPieceAtPosition(temp);

                if (piece == null)
                {
                    bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                }
                else
                {
                    if (piece.isWhite() != isWhite)
                        bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });

                    break;
                }
            }
        }
    }

    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList)
    {
        foreach (var dir in bishopMoveDirections)
        {
            Vector2Int temp = targetPos;

            while (true)
            {
                temp += dir;

                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                ChessPiece piece = state.getChessPieceAtPosition(temp);
                if (piece != null)
                {
                    if (piece.isWhite() == isWhite)
                        bufferList.Add(piece);

                    break;
                }
            }
        }
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
    public float getAiValue() => 90f;

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

    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList)
    {
        foreach (var dir in queenMoveDirections)
        {
            Vector2Int temp = currentPos;

            while (true)
            {
                temp += dir;

                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                ChessPiece piece = state.getChessPieceAtPosition(temp);

                if (piece == null)
                {
                    bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                }
                else
                {
                    if (piece.isWhite() != isWhite)
                        bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });

                    break;
                }
            }
        }
    }

    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList)
    {
        foreach (var dir in queenMoveDirections)
        {
            Vector2Int temp = targetPos;

            while (true)
            {
                temp += dir;

                if (!BoardManager.isPositionOnBoard(temp))
                    break;

                ChessPiece piece = state.getChessPieceAtPosition(temp);
                if (piece != null)
                {
                    if (piece.isWhite() == isWhite)
                        bufferList.Add(piece);

                    break;
                }
            }
        }
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
    public float getAiValue() => 20f;

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

    public void getAvailableMoves(BoardState state, Vector2Int currentPos, bool isWhite, List<MoveAction> bufferList)
    {
        foreach (var move in kingMoves)
        {
            Vector2Int temp = currentPos + move;

            if (BoardManager.isPositionOnBoard(temp))
            {
                ChessPiece piece = state.getChessPieceAtPosition(temp);
                if (piece == null)
                    bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Move });
                else if (piece.isWhite() != isWhite)
                    bufferList.Add(new MoveAction { targetPos = temp, actionType = ActionType.Attack });
            }
        }
    }

    public void getProtectablePieces(BoardState state, Vector2Int targetPos, bool isWhite, List<ChessPiece> bufferList)
    {
        foreach (var move in kingMoves)
        {
            Vector2Int temp = targetPos + move;

            if (BoardManager.isPositionOnBoard(temp))
            {
                ChessPiece piece = state.getChessPieceAtPosition(temp);
                if (piece != null && piece.isWhite() == isWhite)
                    bufferList.Add(piece);
            }
        }
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