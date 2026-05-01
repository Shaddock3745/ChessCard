using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Audio.ProcessorInstance;

public class BoardManager : MonoBehaviour
{
    private static BoardManager instance;

    [SerializeField] private GameObject chessBoard;
    [SerializeField] private GameObject boardOuter;
    [SerializeField] private GameObject piecePrefab;
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private PieceSpriteLibrary pieceSpriteLibrary;

    private static readonly float OUTER_ALPHA_WHEN_MOUSE_INNER = 1f, OUTER_ALPHA_WHEN_MOUSE_NOT_INNER = 0.3f;
    private static readonly float TILE_SIZE = 0.64f;
    private static readonly float START_OFFSET = -2.24f;
    private Dictionary<ChessPiece, ChessPieceUi> pieceToUiMap = new Dictionary<ChessPiece, ChessPieceUi>();
    private BoardState boardState;

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

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void initBoard()
    {
        instance.boardState = new BoardState();

        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                instance.setTile(i, j);

        instance.spawnPiece(ChessType.King, true, 4, 0); // White King
        instance.spawnPiece(ChessType.King, false, 4, 7); // Black King
    }

    private void setTile(int indexX, int indexY)
    {
        float x = START_OFFSET + (TILE_SIZE * indexX);
        float y = START_OFFSET + (TILE_SIZE * indexY);

        BoardTile tile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<BoardTile>();
        tile.transform.SetParent(chessBoard.transform);
        tile.GetComponent<BoxCollider2D>().size = new Vector2(TILE_SIZE, TILE_SIZE);
        boardState.setBoardTileAtPosition(indexX, indexY, tile);
    }

    private void spawnPiece(ChessType chessType, bool isWhite, int indexX, int indexY)
    {
        ChessPiece piece = new ChessPiece(chessType, isWhite, new Vector2Int(indexX, indexY));
        boardState.setChessPieceAtPosition(indexX, indexY, piece);

        float x = START_OFFSET + (TILE_SIZE * indexX);
        float y = START_OFFSET + (TILE_SIZE * indexY);

        ChessPieceUi chessPieceUi = GameObject.Instantiate(piecePrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<ChessPieceUi>();
        chessPieceUi.GetComponent<BoxCollider2D>().size = new Vector2(TILE_SIZE, TILE_SIZE);
        chessPieceUi.initChessUi(piece, pieceSpriteLibrary.getSprite(chessType, isWhite));
        chessPieceUi.moveToPoint(new Vector3(x, y, 0));
        chessPieceUi.transform.SetParent(chessBoard.transform);

        pieceToUiMap.Add(piece, chessPieceUi);
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
        instance.spawnPiece(chessType, isWhite, position.x, position.y);
    }

    public static async Task summonChessPieceWithAnimation(ChessType chessType, bool isWhite, Vector2Int position)
    {
        summonChessPiece(chessType, isWhite, position);
        await GetChessPieceUiAtPosition(position).fadeIn();
    }

    public static ChessPiece getChessPieceAtPosition(int x, int y)
    {
        return instance.boardState.getChessPieceAtPosition(x, y);
    }

    public static ChessPiece getChessPieceAtPosition(Vector2Int position)
    {
        return instance.boardState.getChessPieceAtPosition(position);
    }

    public static ChessPiece getChessPieceAtTile(BoardTile tile)
    {
        return instance.boardState.getChessPieceAtPosition(tile.getX(), tile.getY());
    }

    public static ChessPieceUi GetChessPieceUiAtPosition(Vector2Int position)
    {
        ChessPiece piece = instance.boardState.getChessPieceAtPosition(position);

        if (piece != null)
            return instance.pieceToUiMap[piece];
        else
            return null;
    }

    public static ChessPieceUi GetChessPieceUiAtPosition(int x, int y)
    {
        ChessPiece piece = instance.boardState.getChessPieceAtPosition(x, y);

        if (piece != null)
            return instance.pieceToUiMap[piece];
        else
            return null;
    }

    public static ChessPieceUi GetChessPieceUiAtTile(BoardTile tile)
    {
        ChessPiece piece = instance.boardState.getChessPieceAtPosition(tile.getPos());

        if (piece != null)
            return instance.pieceToUiMap[piece];
        else
            return null;
    }

    public static ChessPieceUi getUiOfChess(ChessPiece piece)
    {
        return instance.pieceToUiMap[piece];
    }

    public static BoardTile getTileAtPosition(Vector2Int position)
    {
        return instance.boardState.getBoardTileAtPosition(position);
    }

    public static BoardTile getTileAtPosition(int x, int y)
    {
        return instance.boardState.getBoardTileAtPosition(x, y);
    }

    public static List<(ChessPiece, Vector2Int)> getAllChessPieces(bool isWhite)
    {
        List<(ChessPiece, Vector2Int)> pieces = new List<(ChessPiece, Vector2Int)>();

        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (getChessPieceAtPosition(i, j) != null && getChessPieceAtPosition(i, j).isWhite() == isWhite)
                    pieces.Add((getChessPieceAtPosition(i, j), new Vector2Int(i, j)));

        return pieces;
    }

    public static bool isEmpty(Vector2Int position)
    {
        return getChessPieceAtPosition(position) == null;
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
        getUiOfChess(movingPiece).moveToPoint(new Vector3(x, y, 0));
    }

    public static void movePiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        instance.boardState.moveChessPiece(from, to);
        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);
        getUiOfChess(movingPiece).moveToPoint(new Vector3(x, y, 0));

        LogManager.createLog(new ChessMoveLog(movingPiece, from, to));
    }

    public static async Task movePieceWithAnimation(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);

        await getUiOfChess(movingPiece).moveToPointWithAnimation(new Vector3(x, y, 0));
        instance.boardState.moveChessPiece(from, to);

        LogManager.createLog(new ChessMoveLog(movingPiece, from, to));
    }

    public static void attackPiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece attacker = getChessPieceAtPosition(from);
        ChessPiece victim = getChessPieceAtPosition(to);
        instance.boardState.moveChessPiece(from, to);

        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);
        getUiOfChess(attacker).moveToPoint(new Vector3(x, y, 0));
        getUiOfChess(victim).destroy();
        instance.pieceToUiMap.Remove(victim);

        LogManager.createLog(new ChessAttackLog(attacker, victim, from, to));
    }

    public static async Task attackPieceWithAnimation(Vector2Int from, Vector2Int to)
    {
        ChessPiece attacker = getChessPieceAtPosition(from);
        ChessPiece victim = getChessPieceAtPosition(to);
        instance.boardState.moveChessPiece(from, to);

        ChessPieceUi attackerUi = getUiOfChess(attacker);
        ChessPieceUi victimUi = getUiOfChess(victim);

        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);

        attackerUi.BringToFrontOf(victimUi);

        Task moveTask = attackerUi.moveToPointWithAnimation(new Vector3(x, y, 0));
        Task destroyTask = victimUi.destroyWithAnimation();

        await Task.WhenAll(moveTask, destroyTask);
        instance.pieceToUiMap.Remove(victim);

        LogManager.createLog(new ChessAttackLog(attacker, victim, from, to));
    }

    public static void showMoveable(Vector2Int position)
    {
        getTileAtPosition(position).showMoveable();
    }

    public static void showMoveable(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
            showMoveable(position);
    }

    public static void showAttackable(Vector2Int position)
    {
        getTileAtPosition(position).showAttackable();
    }

    public static void showAttackable(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
            showAttackable(position);
    }

    public static void showSelectable(Vector2Int position)
    {
        getTileAtPosition(position).showSelectable();
    }
    public static void showSelectable(List<Vector2Int> positions)
    {
        foreach (Vector2Int position in positions)
            showSelectable(position);
    }

    public static void showTarget(Vector2Int position)
    {
        getTileAtPosition(position).showTarget();
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
        getTileAtPosition(position).removeHint();
    }

    public static void removeAllHint()
    {
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                getTileAtPosition(i, j).removeHint();
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
