using DG.Tweening;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private static BoardManager instance;

    [SerializeField] private GameObject chessBoard;
    [SerializeField] private GameObject boardOuter;
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] PieceData[] pieceDatas;

    private static readonly float OUTER_ALPHA_WHEN_MOUSE_INNER = 1f, OUTER_ALPHA_WHEN_MOUSE_NOT_INNER = 0.3f;
    private static readonly float TILE_SIZE = 0.64f;
    private static readonly float START_OFFSET = -2.24f;
    private static readonly float MOVING_ANIMATION_DURATION = 0.5f;
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
            for (int j = 0; j < 8; j++)
                setTile(i, j);

        spawnKing(4, 0, true); // White King
        spawnKing(4, 7, false); // Black King
    }

    private void setTile(int indexX, int indexY)
    {
        float x = START_OFFSET + (TILE_SIZE * indexX);
        float y = START_OFFSET + (TILE_SIZE * indexY);

        GameObject gameObject = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
        gameObject.transform.SetParent(chessBoard.transform);
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(TILE_SIZE, TILE_SIZE);

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
        float x = START_OFFSET + (TILE_SIZE * indexX);
        float y = START_OFFSET + (TILE_SIZE * indexY);

        GameObject gameObject = Instantiate(piecePrefab, new Vector3(x, y, 0), Quaternion.identity);
        gameObject.transform.SetParent(chessBoard.transform);
        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(TILE_SIZE, TILE_SIZE);

        ChessPiece chessPiece = gameObject.GetComponent<ChessPiece>();
        chessPiece.SetPieceData(data);
        chessPieces[indexX, indexY] = chessPiece;
    }

    public static List<BoardTile> getSummonableTiles(ChessType type, bool isWhite)
    {
        List<BoardTile> summonableTiles = new List<BoardTile>();

        if (isWhite)
            switch (type)
            {
                case ChessType.Pawn:
                    for (int i = 0; i < 8; i++)
                        if (isEmpty(new Vector2Int(i, 1)))
                            summonableTiles.Add(getTileAtPosition(i, 1));
                    break;
                case ChessType.Rook:
                    if (isEmpty(new Vector2Int(0, 0)))
                        summonableTiles.Add(getTileAtPosition(0, 0));
                    if (isEmpty(new Vector2Int(7, 0)))
                        summonableTiles.Add(getTileAtPosition(7, 0));
                    break;
                case ChessType.Knight:
                    if (isEmpty(new Vector2Int(1, 0)))
                        summonableTiles.Add(getTileAtPosition(1, 0));
                    if (isEmpty(new Vector2Int(6, 0)))
                        summonableTiles.Add(getTileAtPosition(6, 0));
                    break;
                case ChessType.Bishop:
                    if (isEmpty(new Vector2Int(2, 0)))
                        summonableTiles.Add(getTileAtPosition(2, 0));
                    if (isEmpty(new Vector2Int(5, 0)))
                        summonableTiles.Add(getTileAtPosition(5, 0));
                    break;
                case ChessType.Queen:
                    if (isEmpty(new Vector2Int(3, 0)))
                        summonableTiles.Add(getTileAtPosition(3, 0));
                    break;
            }
        else
            switch (type)
            {
                case ChessType.Pawn:
                    for (int i = 0; i < 8; i++)
                        if (isEmpty(new Vector2Int(i, 6)))
                            summonableTiles.Add(getTileAtPosition(i, 6));
                    break;
                case ChessType.Rook:
                    if (isEmpty(new Vector2Int(0, 7)))
                        summonableTiles.Add(getTileAtPosition(0, 7));
                    if (isEmpty(new Vector2Int(7, 7)))
                        summonableTiles.Add(getTileAtPosition(7, 7));
                    break;
                case ChessType.Knight:
                    if (isEmpty(new Vector2Int(1, 7)))
                        summonableTiles.Add(getTileAtPosition(1, 7));
                    if (isEmpty(new Vector2Int(6, 7)))
                        summonableTiles.Add(getTileAtPosition(6, 7));
                    break;
                case ChessType.Bishop:
                    if (isEmpty(new Vector2Int(2, 7)))
                        summonableTiles.Add(getTileAtPosition(2, 7));
                    if (isEmpty(new Vector2Int(5, 7)))
                        summonableTiles.Add(getTileAtPosition(5, 7));
                    break;
                case ChessType.Queen:
                    if (isEmpty(new Vector2Int(3, 7)))
                        summonableTiles.Add(getTileAtPosition(3, 7));
                    break;
            }

        return summonableTiles;
    }

    public static void summonChessPiece(ChessType chessType, bool isWhite, Vector2Int position)
    {
        switch (chessType)
        {
            case ChessType.Pawn:
                instance.spawnPawn(position.x, position.y, isWhite);
                break;
            case ChessType.Rook:
                instance.spawnRooK(position.x, position.y, isWhite);
                break;
            case ChessType.Knight:
                instance.spawnKnight(position.x, position.y, isWhite);
                break;
            case ChessType.Bishop:
                instance.spawnBishop(position.x, position.y, isWhite);
                break;
            case ChessType.Queen:
                instance.spawnQueen(position.x, position.y, isWhite);
                break;
        }
    }

    public static async Task summonChessPieceWithAnimation(ChessType chessType, bool isWhite, Vector2Int position)
    {
        switch (chessType)
        {
            case ChessType.Pawn:
                instance.spawnPawn(position.x, position.y, isWhite);
                break;
            case ChessType.Rook:
                instance.spawnRooK(position.x, position.y, isWhite);
                break;
            case ChessType.Knight:
                instance.spawnKnight(position.x, position.y, isWhite);
                break;
            case ChessType.Bishop:
                instance.spawnBishop(position.x, position.y, isWhite);
                break;
            case ChessType.Queen:
                instance.spawnQueen(position.x, position.y, isWhite);
                break;
        }

        await getChessPieceAtPosition(position).fadeIn();
    }

    public static ChessPiece getChessPieceAtPosition(int x, int y)
    {
        return instance.chessPieces[x, y];
    }

    public static ChessPiece getChessPieceAtPosition(Vector2Int position)
    {
        return instance.chessPieces[position.x, position.y];
    }

    public static ChessPiece getChessPieceAtTile(BoardTile tile)
    {
        return instance.chessPieces[tile.getX(), tile.getY()];
    }

    public static BoardTile getTileAtPosition(Vector2Int position)
    {
        return instance.boardTiles[position.x, position.y];
    }

    public static BoardTile getTileAtPosition(int x, int y)
    {
        return instance.boardTiles[x, y];
    }

    public static List<(ChessPiece, Vector2Int)> getAllChessPieces(bool isWhite)
    {
        List<(ChessPiece, Vector2Int)> pieces = new List<(ChessPiece, Vector2Int)>();

        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (instance.chessPieces[i, j] != null && instance.chessPieces[i, j].isWhite() == isWhite)
                    pieces.Add((getChessPieceAtPosition(i, j), new Vector2Int(i, j)));

        return pieces;
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

    public static void moveCanceled(ChessPiece movingPiece, Vector2Int firstPos)
    {
        float x = START_OFFSET + (TILE_SIZE * firstPos.x);
        float y = START_OFFSET + (TILE_SIZE * firstPos.y);
        movingPiece.moveToPoint(new Vector3(x, y, 0));
    }

    public static void movePiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        instance.chessPieces[to.x, to.y] = movingPiece;
        instance.chessPieces[from.x, from.y] = null;
        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);
        movingPiece.moveToPoint(new Vector3(x, y, 0));
    }

    public static async Task movePieceWithAnimation(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);

        await movingPiece.transform.DOMove(new Vector3(x, y, 0), MOVING_ANIMATION_DURATION)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                instance.chessPieces[to.x, to.y] = movingPiece;
                instance.chessPieces[from.x, from.y] = null;
            })
            .AsyncWaitForCompletion();
    }


    public static void attackPiece(Vector2Int from, Vector2Int to)
    {
        // unsolved

        ChessPiece targetPiece = getChessPieceAtPosition(to.x, to.y);
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        instance.chessPieces[to.x, to.y] = movingPiece;
        instance.chessPieces[from.x, from.y] = null;
        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);
        movingPiece.moveToPoint(new Vector3(x, y, 0));
        Destroy(targetPiece.gameObject);
    }

    public static async Task attackPieceWithAnimation(Vector2Int from, Vector2Int to)
    {
        ChessPiece targetPiece = getChessPieceAtPosition(to.x, to.y);
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);

        SpriteRenderer movingSR = movingPiece.GetComponent<SpriteRenderer>();
        SpriteRenderer targetSR = targetPiece.GetComponent<SpriteRenderer>();

        if (movingSR.sortingOrder < targetSR.sortingOrder)
        {
            int temp = movingSR.sortingOrder;
            movingSR.sortingOrder = targetSR.sortingOrder;
            targetSR.sortingOrder = temp;
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(movingPiece.transform.DOMove(new Vector3(x, y, 0), MOVING_ANIMATION_DURATION).SetEase(Ease.InOutQuad));
        seq.Join(targetSR.DOFade(0, MOVING_ANIMATION_DURATION));

        await seq.OnComplete(() =>
        {
            Destroy(targetPiece.gameObject);
            instance.chessPieces[to.x, to.y] = movingPiece;
            instance.chessPieces[from.x, from.y] = null;
        }).AsyncWaitForCompletion();
    }

    public static void showMoveable(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].showMoveable();
    }

    public static void showMoveable(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
            showMoveable(position);
    }

    public static void showAttackable(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].showAttackable();
    }

    public static void showAttackable(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
            showAttackable(position);
    }

    public static void showSelectable(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].showSelectable();
    }
    public static void showSelectable(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
            showSelectable(position);
    }

    public static void showTarget(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].showTarget();
    }

    public static void showTarget(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
            showTarget(position);
    }
    public static async Task showTargetWithAnimation(List<Vector2Int> positions)
    {
        List<Task> tasks = new List<Task>();

        foreach (Vector2Int position in positions)
            tasks.Add(getTileAtPosition(position).showTargetWithAnimation());

        await Task.WhenAll(tasks);
    }

    public static void removeHint(Vector2Int position)
    {
        instance.boardTiles[position.x, position.y].removeHint();
    }

    public static void removeAllHint()
    {
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                instance.boardTiles[i, j].removeHint();
    }

    public static void showOuter(bool isMouseInner)
    {
        instance.boardOuter.SetActive(true);

        if (isMouseInner)
            instance.boardOuter.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, OUTER_ALPHA_WHEN_MOUSE_INNER);
        else
            instance.boardOuter.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, OUTER_ALPHA_WHEN_MOUSE_NOT_INNER);
    }

    public static void hideOuter()
    {
        instance.boardOuter.SetActive(false);
    }
}
