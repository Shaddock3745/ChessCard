using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum ActionType
{
    Move = 0,
    Attack = 1
}

public struct MoveAction
{
    public Vector2Int targetPos;
    public ActionType actionType;
}


public interface MovementType
{
    public List<MoveAction> getAvailableMoves(Vector2Int currentPos, bool isWhite);
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
}
