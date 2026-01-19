using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private static BoardManager instance;

    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] PieceData[] pieceDatas;

    private static readonly float tileSize = 0.96f;
    private static readonly float startOffeset = -3.36f;
    private ChessPiece[,] chessPieces = new ChessPiece[8, 8];
    private BoardTile[,] boardTiles = new BoardTile[8, 8];

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        setBoard();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void setBoard()
    {
        for (int i = 0; i < 8; i++)
        {
            spawnPawn(i, 1, true); // White Pawns
            spawnPawn(i, 6, false); // Black Pawns

            for (int j = 0; j < 8; j++)
                setTile(i, j);
        }

        spawnRooK(0, 0, true); // White Rook
        spawnRooK(7, 0, true); // White Rook
        spawnRooK(0, 7, false); // Black Rook
        spawnRooK(7, 7, false); // Black Rook

        spawnKnight(1, 0, true); // White Knight
        spawnKnight(6, 0, true); // White Knight
        spawnKnight(1, 7, false); // Black Knight
        spawnKnight(6, 7, false); // Black Knight

        spawnBishop(2, 0, true); // White Bishop
        spawnBishop(5, 0, true); // White Bishop
        spawnBishop(2, 7, false); // Black Bishop
        spawnBishop(5, 7, false); // Black Bishop

        spawnQueen(3, 0, true); // White Queen
        spawnQueen(3, 7, false); // Black Queen

        spawnKing(4, 0, true); // White King
        spawnKing(4, 7, false); // Black King
    }

    private void setTile(int indexX, int indexY)
    {
        float x = startOffeset + (tileSize * indexX);
        float y = startOffeset + (tileSize * indexY);

        GameObject gameObject = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
        gameObject.transform.SetParent(this.transform);

        BoardTile tile = gameObject.GetComponent<BoardTile>();
        boardTiles[indexX, indexY] = tile;
        tile.initPos(indexX, indexY);
    }

    private void spawnPawn(int indexX, int indexY, bool isWhite)
    {
        if (isWhite)
            spawnPiece(pieceDatas[0], indexX, indexY);
        else
            spawnPiece(pieceDatas[6], indexX, indexY);
    }

    private void spawnRooK(int indexX, int indexY, bool isWhite)
    {
        if (isWhite)
            spawnPiece(pieceDatas[1], indexX, indexY);
        else
            spawnPiece(pieceDatas[7], indexX, indexY);
    }

    private void spawnKnight(int indexX, int indexY, bool isWhite)
    {
        if (isWhite)
            spawnPiece(pieceDatas[2], indexX, indexY);
        else
            spawnPiece(pieceDatas[8], indexX, indexY);
    }

    private void spawnBishop(int indexX, int indexY, bool isWhite)
    {
        if (isWhite)
            spawnPiece(pieceDatas[3], indexX, indexY);
        else
            spawnPiece(pieceDatas[9], indexX, indexY);
    }

    private void spawnQueen(int indexX, int indexY, bool isWhite)
    {
        if (isWhite)
            spawnPiece(pieceDatas[4], indexX, indexY);
        else
            spawnPiece(pieceDatas[10], indexX, indexY);
    }

    private void spawnKing(int indexX, int indexY, bool isWhite)
    {
        if (isWhite)
            spawnPiece(pieceDatas[5], indexX, indexY);
        else
            spawnPiece(pieceDatas[11], indexX, indexY);
    }

    private void spawnPiece(PieceData data, int indexX, int indexY)
    {
        float x = startOffeset + (tileSize * indexX);
        float y = startOffeset + (tileSize * indexY);

        GameObject gameObject = Instantiate(piecePrefab, new Vector3(x, y, 0), Quaternion.identity);
        gameObject.transform.SetParent(this.transform);

        ChessPiece chessPiece = gameObject.GetComponent<ChessPiece>();
        chessPiece.SetPieceData(data);
        chessPieces[indexX, indexY] = chessPiece;
    }

    public static ChessPiece getChessPieceAtPosition(int x, int y)
    {
        return instance.chessPieces[x, y];
    }

    public static ChessPiece getChessPieceAtTile(BoardTile tile)
    {
        return instance.chessPieces[tile.getX(), tile.getY()];
    }

    public static bool isChessPieceHere(int x, int y)
    {
        return instance.chessPieces[x, y] != null;
    }

    public static bool isEmpty(Vector2Int position)
    {
        return instance.chessPieces[position.x, position.y] == null;
    }

    public static bool isPositionOnBoard(Vector2Int position)
    {
        return position.x >= 0 && position.x < 8 && position.y >= 0 && position.y < 8;
    }

    public static bool isWhiteAtPosition(Vector2Int position)
    {
        return isEmpty(position) ? false : getChessPieceAtPosition(position.x, position.y).isWhite();
    }

    public static void moveCanceled(ChessPiece movingPiece , Vector2Int firstPos)
    {
        float x = startOffeset + (tileSize * firstPos.x);
        float y = startOffeset + (tileSize * firstPos.y);
        movingPiece.transform.position = new Vector3(x, y, 0);
    }

    public static void movePiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        instance.chessPieces[to.x, to.y] = movingPiece;
        instance.chessPieces[from.x, from.y] = null;
        float x = startOffeset + (tileSize * to.x);
        float y = startOffeset + (tileSize * to.y);
        movingPiece.transform.position = new Vector3(x, y, 0);
    }

    public static void attackPiece(Vector2Int from, Vector2Int to)
    {
        // unsolved

        ChessPiece targetPiece = getChessPieceAtPosition(to.x, to.y);
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        instance.chessPieces[to.x, to.y] = movingPiece;
        instance.chessPieces[from.x, from.y] = null;
        float x = startOffeset + (tileSize * to.x);
        float y = startOffeset + (tileSize * to.y);
        movingPiece.transform.position = new Vector3(x, y, 0);
        Destroy(targetPiece.gameObject);
    }

    public static void showMoveable(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].showMoveable();
    }

    public static void showAttackable(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].showAttackable();
    }

    public static void showTarget(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].showTarget();
    }

    public static void removeHint(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].removeHint();
    }

    public static void removeAllHint()
    {
        for(int i = 0;i<8;i++)
            for(int j = 0;j<8;j++)
                instance.boardTiles[i,j].removeHint();
    }
}
