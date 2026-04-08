using UnityEngine;

public class BoardState
{
    private ChessPiece[,] chessPieces;
    private BoardTile[,] boardTiles;

    public BoardState()
    {
        initializeBoard();
    }

    private void initializeBoard()
    {
        chessPieces = new ChessPiece[8, 8];
        boardTiles = new BoardTile[8, 8];
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

    public void setBoardTileAtPosition(int x, int y, BoardTile tile)
    {
        boardTiles[x, y] = tile;
        tile.initPos(x, y);
    }

    public BoardTile getBoardTileAtPosition(int x, int y)
    {
        return boardTiles[x, y];
    }

    public BoardTile getBoardTileAtPosition(Vector2Int position)
    {
        return boardTiles[position.x, position.y];
    }

    public void moveChessPiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece pieceToMove = getChessPieceAtPosition(from);

        if (pieceToMove != null)
        {
            setChessPieceAtPosition(to, pieceToMove);
            setChessPieceAtPosition(from, null);
        }
    }

    public void removeChessPieceAtPosition(int x, int y)
    {
        chessPieces[x, y] = null;
    }

    public void removeChessPieceAtPosition(Vector2Int position)
    {
        chessPieces[position.x, position.y] = null;
    }
}
