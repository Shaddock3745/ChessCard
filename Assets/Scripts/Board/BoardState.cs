using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoardState
{
    private readonly bool isVirtual;
    private ChessPiece[,] chessPieces;

    public BoardState(bool isVirtual = false)
    {
        initializeBoard();
        this.isVirtual = isVirtual;
    }

    public BoardState Clone()
    {
        BoardState clone = new BoardState(true);

        for (int x = 0; x < chessPieces.GetLength(0); x++)
            for (int y = 0; y < chessPieces.GetLength(1); y++)
                if (this.chessPieces[x, y] != null)
                    clone.chessPieces[x, y] = this.chessPieces[x, y].Clone();

        return clone;
    }

    public bool IsVirtual() => isVirtual;

    private void initializeBoard()
    {
        chessPieces = new ChessPiece[8, 8];
    }

    public void setChessPieceAtPosition(int x, int y, ChessPiece piece)
    {
        chessPieces[x, y] = piece;
    }

    public void setChessPieceAtPosition(Vector2Int position, ChessPiece piece)
    {
        chessPieces[position.x, position.y] = piece;
    }

    public ChessPiece getChessPieceAtPosition(int x, int y)
    {
        return chessPieces[x, y];
    }

    public ChessPiece getChessPieceAtPosition(Vector2Int position)
    {
        return chessPieces[position.x, position.y];
    }

    public ChessPiece getKing(bool isWhite)
    {


        for (int x = 0; x < chessPieces.GetLength(0); x++)
            for (int y = 0; y < chessPieces.GetLength(1); y++)
            {
                ChessPiece piece = chessPieces[x, y];

                if (piece != null && piece.getChessType() == ChessType.King && piece.isWhite() == isWhite)
                    return piece;
            }

        return null;
    }

    public Vector2Int getPositionOfKing(bool isWhite)
    {
        for (int x = 0; x < chessPieces.GetLength(0); x++)
            for (int y = 0; y < chessPieces.GetLength(1); y++)
            {
                ChessPiece piece = chessPieces[x, y];

                if (piece != null && piece.getChessType() == ChessType.King && piece.isWhite() == isWhite)
                    return new Vector2Int(x, y);
            }

        return new Vector2Int(0, 0);
    }

    public void moveChessPiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece pieceFrom = getChessPieceAtPosition(from);
        ChessPiece pieceTo = getChessPieceAtPosition(to);

        if (pieceFrom != null)
        {
            setChessPieceAtPosition(to, pieceFrom);
            setChessPieceAtPosition(from, null);

            if (pieceTo != null)
                pieceTo.onDeath();

            pieceFrom.setPosition(to);
        }

    }

    public void removeChessPieceAtPosition(int x, int y)
    {
        ChessPiece target = chessPieces[x, y];

        if (target != null)
            target.onDeath();

        chessPieces[x, y] = null;
    }

    public void removeChessPieceAtPosition(Vector2Int position)
    {
        ChessPiece target = chessPieces[position.x, position.y];

        if (target != null)
            target.onDeath();

        chessPieces[position.x, position.y] = null;
    }

    public void swapTwoPosition(Vector2Int first, Vector2Int second)
    {
        ChessPiece firstChess = chessPieces[first.x, first.y];
        ChessPiece secondChess = chessPieces[second.x, second.y];
        setChessPieceAtPosition(second, firstChess);
        setChessPieceAtPosition(first, secondChess);

        if (firstChess != null)
            firstChess.setPosition(second);

        if (secondChess != null)
            secondChess.setPosition(first);
    }

    public int getChessAmount(bool isWhite, ChessType type)
    {
        int count = 0;

        for (int x = 0; x < chessPieces.GetLength(0); x++)
            for (int y = 0; y < chessPieces.GetLength(1); y++)
            {
                ChessPiece piece = chessPieces[x, y];

                if (piece != null && piece.getChessType() == type && piece.isWhite() == isWhite)
                    count++;
            }

        return count;
    }

    public int getChessAmount(bool isWhite, params ChessType[] type)
    {
        return getChessAmount(isWhite, new List<ChessType>(type));
    }

    public int getChessAmount(bool isWhite, List<ChessType> vaildType)
    {
        int count = 0;

        for (int x = 0; x < chessPieces.GetLength(0); x++)
            for (int y = 0; y < chessPieces.GetLength(1); y++)
            {
                ChessPiece piece = chessPieces[x, y];

                if (piece != null && vaildType.Contains(piece.getChessType()) && piece.isWhite() == isWhite)
                    count++;
            }

        return count;
    }

    public bool isPositionOnBoard(Vector2Int position)
    {
        return position.x >= 0 && position.x < chessPieces.GetLength(0) && position.y >= 0 && position.y < chessPieces.GetLength(1);
    }
}
