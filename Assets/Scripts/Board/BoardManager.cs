using System;
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
    [SerializeField] private GameObject roarPrefab;
    [SerializeField] private PieceSpriteLibrary pieceSpriteLibrary;

    private static readonly float OUTER_ALPHA_WHEN_MOUSE_INNER = 1f, OUTER_ALPHA_WHEN_MOUSE_NOT_INNER = 0.3f;
    private static readonly float TILE_SIZE = 0.64f;
    private static readonly float START_OFFSET = -2.24f;
    private const int BOARD_SIZE = 8;

    // 依棋盤位置算靜止層級：越靠近畫面下方(較小 y)越前面；同列再用 x 當決勝，確保每格唯一、不會平手
    private static int sortingOrderAt(int gridX, int gridY) => (BOARD_SIZE - 1 - gridY) * BOARD_SIZE + gridX;
    private static int sortingOrderAt(Vector2Int pos) => sortingOrderAt(pos.x, pos.y);

    private BoardTile[,] boardTiles;
    private Dictionary<ChessPiece, ChessPieceUi> pieceToUiMap = new Dictionary<ChessPiece, ChessPieceUi>();
    private BoardState boardState;
    private bool isAnimating = false;

    public static Action<ChessPiece, Vector2Int, Vector2Int> OnChessMove;
    public static Action<ChessPiece, ChessPiece, Vector2Int, Vector2Int> OnChessAttack;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        MatchDataManager.OnPlayerUseCard += updateBoardUiWhenPlayerUseCard;
    }

    private void OnDisable()
    {
        MatchDataManager.OnPlayerUseCard -= updateBoardUiWhenPlayerUseCard;
    }

    public static void initBoard()
    {
        foreach (var pieceUiItem in instance.pieceToUiMap)
            Destroy(pieceUiItem.Value.gameObject);

        instance.boardTiles = new BoardTile[8, 8];
        instance.pieceToUiMap = new Dictionary<ChessPiece, ChessPieceUi>();
        instance.boardState = new BoardState();

        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                instance.setTile(i, j);

        instance.spawnPiece(ChessType.King, true, 4, 0); // White King
        instance.spawnPiece(ChessType.King, false, 4, 7); // Black King
    }

    public static bool isPlayingAnimation() => instance.isAnimating;

    public static BoardState getRealBoardState() => instance.boardState;

    public static BoardState getCloneBoardState() => instance.boardState.Clone();

    public static void hideBoard()
    {
        instance.chessBoard.SetActive(false);
    }

    public static void showBoard()
    {
        instance.chessBoard.SetActive(true);
    }

    private void setTile(int indexX, int indexY)
    {
        float x = START_OFFSET + (TILE_SIZE * indexX);
        float y = START_OFFSET + (TILE_SIZE * indexY);

        BoardTile tile = GameObject.Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<BoardTile>();
        tile.transform.SetParent(chessBoard.transform);
        tile.GetComponent<BoxCollider2D>().size = new Vector2(TILE_SIZE, TILE_SIZE);
        setBoardTileAtPosition(indexX, indexY, tile);
    }

    private void spawnPiece(ChessType chessType, bool isWhite, int indexX, int indexY)
    {
        Vector2Int position = new Vector2Int(indexX, indexY);
        ChessPiece piece = new ChessPiece(chessType, isWhite, position);
        boardState.setChessPieceAtPosition(indexX, indexY, piece);

        float x = START_OFFSET + (TILE_SIZE * indexX);
        float y = START_OFFSET + (TILE_SIZE * indexY);

        ChessPieceUi chessPieceUi = GameObject.Instantiate(piecePrefab, new Vector3(x, y, 0), Quaternion.identity).GetComponent<ChessPieceUi>();
        chessPieceUi.GetComponent<BoxCollider2D>().size = new Vector2(TILE_SIZE, TILE_SIZE);
        chessPieceUi.initChessUi(piece, pieceSpriteLibrary.getSprite(chessType, isWhite));
        chessPieceUi.moveToPoint(new Vector3(x, y, 0));
        chessPieceUi.SetSortingOrder(sortingOrderAt(indexX, indexY)); // 給予依位置的基礎層級，避免與其他棋子重疊時順序未定義
        chessPieceUi.transform.SetParent(chessBoard.transform);

        pieceToUiMap.Add(piece, chessPieceUi);
    }

    public static int getChessAmount(bool isWhite, params ChessType[] type)
    {
        return instance.boardState.getChessAmount(isWhite, type);
    }

    public static int getChessAmount(bool isWhite, ChessType type)
    {
        return instance.boardState.getChessAmount(isWhite, type);
    }

    public static int getChessAmount(bool isWhite, List<ChessType> vaildTypes)
    {
        return instance.boardState.getChessAmount(isWhite, vaildTypes);
    }

    public static bool chessAmountReachLimit(ChessType type, bool isWhite)
    {
        switch (type)
        {
            case ChessType.Pawn:
                return getChessAmount(isWhite, ChessType.Pawn) >= 8;
            case ChessType.Rook:
                return getChessAmount(isWhite, ChessType.Rook) >= 2;
            case ChessType.Knight:
                return getChessAmount(isWhite, ChessType.Knight) >= 2;
            case ChessType.Bishop:
                return getChessAmount(isWhite, ChessType.Bishop) >= 2;
            case ChessType.Queen:
                return getChessAmount(isWhite, ChessType.Queen) >= 1;
            case ChessType.King:
                return getChessAmount(isWhite, ChessType.King) >= 1;
            default:
                return false;
        }
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
        instance.isAnimating = true;
        await GetChessPieceUiAtPosition(position).fadeIn();
        instance.isAnimating = false;
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
        ChessPiece piece = instance.boardState.getChessPieceAtPosition(tile.getPosition());

        if (piece != null)
            return instance.pieceToUiMap[piece];
        else
            return null;
    }

    public static ChessPiece getKing(bool isWhite)
    {
        return instance.boardState.getKing(isWhite);
    }

    public static Vector2Int getPositionOfKing(bool isWhite)
    {
        return instance.boardState.getPositionOfKing(isWhite);
    }

    public static ChessPieceUi getUiOfChess(ChessPiece piece)
    {
        if (piece == null || piece.IsVirtual())
            return null;

        return instance.pieceToUiMap[piece];
    }

    private static void setBoardTileAtPosition(int x, int y, BoardTile tile)
    {
        instance.boardTiles[x, y] = tile;
        tile.initPos(x, y);
    }

    public static BoardTile getTileAtPosition(int x, int y)
    {
        return instance.boardTiles[x, y];
    }

    public static BoardTile getTileAtPosition(Vector2Int position)
    {
        return instance.boardTiles[position.x, position.y];
    }

    public static List<BoardTile> getTilesOfChesses(ChessType chessType, bool isWhite)
    {
        List<BoardTile> tiles = new List<BoardTile>();

        for (int x = 0; x < instance.boardTiles.GetLength(0); x++)
            for (int y = 0; y < instance.boardTiles.GetLength(1); y++)
            {
                ChessPiece piece = getChessPieceAtPosition(x, y);

                if (piece != null && piece.getChessType() == chessType && piece.isWhite() == isWhite)
                    tiles.Add(instance.boardTiles[x, y]);
            }

        return tiles;
    }

    public static List<BoardTile> getTilesOfPromotablePawn(bool isWhite)
    {
        List<BoardTile> tiles = new List<BoardTile>();

        int y = isWhite ? instance.boardTiles.GetLength(1) - 1 : 0;

        for (int x = 0; x < instance.boardTiles.GetLength(0); x++)
        {
            ChessPiece piece = getChessPieceAtPosition(x, y);

            if (piece != null && piece.getChessType() == ChessType.Pawn && piece.isWhite() == isWhite)
                tiles.Add(instance.boardTiles[x, y]);
        }

        return tiles;
    }

    public static List<ChessPiece> getAllChessPieces(bool isWhite, params ChessType[] chessTypes)
    {
        return getAllChessPieces(isWhite, new List<ChessType>(chessTypes));
    }

    public static List<ChessPiece> getAllChessPieces(bool isWhite, List<ChessType> vaildTypes)
    {
        return instance.boardState.getAllChessPieces(isWhite, vaildTypes);
    }

    public static bool isEmpty(Vector2Int position)
    {
        return getChessPieceAtPosition(position) == null;
    }

    public static bool isPositionOnBoard(Vector2Int position)
    {
        return instance.boardState.isPositionOnBoard(position);
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
        InputManager.tryDetectHover();
    }

    private static void innerMovePiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        instance.boardState.moveChessPiece(from, to);

        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);

        ChessPieceUi movingUi = getUiOfChess(movingPiece);
        movingUi.moveToPoint(new Vector3(x, y, 0));
        movingUi.SetSortingOrder(sortingOrderAt(to));
    }

    public static void movePiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        innerMovePiece(from, to);
        movingPiece.onMoveEnd(from, to, GameManager.getCurrentTurnCount());
        OnChessMove(movingPiece, from, to);
        InputManager.tryDetectHover();
    }

    private static async Task innerMovePieceWithAnimation(Vector2Int from, Vector2Int to)
    {
        if (from == to)
            return;

        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);

        ChessPieceUi movingUi = getUiOfChess(movingPiece);
        instance.isAnimating = true;
        movingUi.BringToFront(); // 滑行期間置頂，避免被途經的棋子蓋住
        await movingUi.moveToPointWithAnimation(new Vector3(x, y, 0));
        movingUi.SetSortingOrder(sortingOrderAt(to)); // 更新到新位置的基礎層級
        movingUi.RestoreSortingOrder();
        instance.isAnimating = false;
        instance.boardState.moveChessPiece(from, to);
    }

    public static async Task movePieceWithAnimation(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        await innerMovePieceWithAnimation(from, to);
        movingPiece.onMoveEnd(from, to, GameManager.getCurrentTurnCount());
        OnChessMove(movingPiece, from, to);
        InputManager.tryDetectHover();
    }

    public static async Task movePieceWithAnimationByEffect(Vector2Int from, Vector2Int to)
    {
        ChessPiece movingPiece = getChessPieceAtPosition(from.x, from.y);
        await innerMovePieceWithAnimation(from, to);
        movingPiece.onMoveEnd(from, to, GameManager.getCurrentTurnCount());
        InputManager.tryDetectHover();
    }

    private static async Task showRoarAnimation(Vector2Int position, Vector2Int direction)
    {
        float x = START_OFFSET + (TILE_SIZE * position.x);
        float y = START_OFFSET + (TILE_SIZE * position.y);
        RoarObject roar = GameObject.Instantiate(instance.roarPrefab, new Vector3(x, y, 0), Quaternion.LookRotation(Vector3.forward, (Vector2)direction)).GetComponent<RoarObject>();

        instance.isAnimating = true;
        await roar.show();
        instance.isAnimating = false;
    }

    public static async Task pieceGotRoared(Vector2Int position, Vector2Int direction, bool isWhiteRoaring)
    {
        if (!isPositionOnBoard(position))
            return;

        Task roarTask = showRoarAnimation(position, direction);
        Vector2Int finalPosition = position + direction;

        instance.isAnimating = true;

        if (!isEmpty(position) && (isWhiteRoaring ^ isWhiteAtPosition(position)) && isPositionOnBoard(finalPosition) && isEmpty(finalPosition))
            await Task.WhenAll(roarTask, innerMovePieceWithAnimation(position, finalPosition));
        else
            await roarTask;

        instance.isAnimating = false;
    }

    public static bool gameOverCheck(ChessPiece attackTarget)
    {
        if (attackTarget != null && attackTarget.getChessType() == ChessType.King)
        {
            GameManager.triggerOnGameOver(!attackTarget.isWhite());
            return true;
        }

        return false;
    }

    public static void attackPiece(Vector2Int from, Vector2Int to)
    {
        ChessPiece attacker = getChessPieceAtPosition(from);
        ChessPiece target = getChessPieceAtPosition(to);
        instance.boardState.moveChessPiece(from, to);

        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);
        ChessPieceUi attackerUi = getUiOfChess(attacker);
        attackerUi.moveToPoint(new Vector3(x, y, 0));
        attackerUi.SetSortingOrder(sortingOrderAt(to));

        getUiOfChess(target).destroy();
        instance.pieceToUiMap.Remove(target);

        int currentTurnCount = GameManager.getCurrentTurnCount();
        attacker.onMoveEnd(from, to, currentTurnCount);
        attacker.onAttackEnd(from, target, currentTurnCount);

        OnChessAttack(attacker, target, from, to);
        InputManager.tryDetectHover();

        gameOverCheck(target);
    }

    public static async Task attackPieceWithAnimation(Vector2Int from, Vector2Int to)
    {
        ChessPiece attacker = getChessPieceAtPosition(from);
        ChessPiece target = getChessPieceAtPosition(to);
        instance.boardState.moveChessPiece(from, to);

        ChessPieceUi attackerUi = getUiOfChess(attacker);
        ChessPieceUi victimUi = getUiOfChess(target);

        float x = START_OFFSET + (TILE_SIZE * to.x);
        float y = START_OFFSET + (TILE_SIZE * to.y);

        attackerUi.BringToFront();

        Task moveTask = attackerUi.moveToPointWithAnimation(new Vector3(x, y, 0));
        Task destroyTask = victimUi.destroyWithAnimation();

        instance.isAnimating = true;
        await Task.WhenAll(moveTask, destroyTask);
        instance.isAnimating = false;
        attackerUi.SetSortingOrder(sortingOrderAt(to)); // 更新到新位置的基礎層級
        attackerUi.RestoreSortingOrder();
        instance.pieceToUiMap.Remove(target);

        int currentTurnCount = GameManager.getCurrentTurnCount();
        attacker.onMoveEnd(from, to, currentTurnCount);
        attacker.onAttackEnd(from, target, currentTurnCount);

        OnChessAttack(attacker, target, from, to);
        InputManager.tryDetectHover();

        gameOverCheck(target);
    }

    public static void removePiece(Vector2Int position)
    {
        ChessPiece piece = getChessPieceAtPosition(position);
        instance.boardState.removeChessPieceAtPosition(position);
        getUiOfChess(piece).destroy();
        instance.pieceToUiMap.Remove(piece);

        InputManager.tryDetectHover();
    }

    public static async Task removePieceWithAnimation(Vector2Int position)
    {
        ChessPiece piece = getChessPieceAtPosition(position);
        instance.boardState.removeChessPieceAtPosition(position);
        instance.isAnimating = true;
        await getUiOfChess(piece).destroyWithAnimation();
        instance.isAnimating = false;
        instance.pieceToUiMap.Remove(piece);

        InputManager.tryDetectHover();
    }

    public static void swapTwoPosition(Vector2Int first, Vector2Int second)
    {
        ChessPieceUi firstUi = getUiOfChess(getChessPieceAtPosition(first));
        ChessPieceUi secondUi = getUiOfChess(getChessPieceAtPosition(second));
        instance.boardState.swapTwoPosition(first, second);

        float firstX = START_OFFSET + (TILE_SIZE * first.x), firstY = START_OFFSET + (TILE_SIZE * first.y);
        float secondX = START_OFFSET + (TILE_SIZE * second.x), secondY = START_OFFSET + (TILE_SIZE * second.y);

        if (firstUi != null)
        {
            firstUi.moveToPoint(new Vector3(secondX, secondY, 0)); // 原本在 first 的棋子 → 移到 second
            firstUi.SetSortingOrder(sortingOrderAt(second));
        }

        if (secondUi != null)
        {
            secondUi.moveToPoint(new Vector3(firstX, firstY, 0)); // 原本在 second 的棋子 → 移到 first
            secondUi.SetSortingOrder(sortingOrderAt(first));
        }

        InputManager.tryDetectHover();
    }

    public static async Task swapTwoPositionWithAnimation(Vector2Int first, Vector2Int second)
    {
        ChessPieceUi firstUi = getUiOfChess(getChessPieceAtPosition(first));
        ChessPieceUi secondUi = getUiOfChess(getChessPieceAtPosition(second));
        instance.boardState.swapTwoPosition(first, second);

        float firstX = START_OFFSET + (TILE_SIZE * first.x), firstY = START_OFFSET + (TILE_SIZE * first.y);
        float secondX = START_OFFSET + (TILE_SIZE * second.x), secondY = START_OFFSET + (TILE_SIZE * second.y);
        List<Task> tasks = new List<Task>();

        if (firstUi != null)
        {
            firstUi.BringToFront(); // 交換時兩顆都會交錯滑過盤面，先置頂避免被途經棋子蓋住
            tasks.Add(firstUi.moveToPointWithAnimation(new Vector3(secondX, secondY, 0)));
        }

        if (secondUi != null)
        {
            secondUi.BringToFront();
            tasks.Add(secondUi.moveToPointWithAnimation(new Vector3(firstX, firstY, 0)));
        }

        instance.isAnimating = true;
        await Task.WhenAll(tasks);
        instance.isAnimating = false;

        firstUi?.SetSortingOrder(sortingOrderAt(second)); // firstUi 已移到 second 格
        secondUi?.SetSortingOrder(sortingOrderAt(first)); // secondUi 已移到 first 格
        firstUi?.RestoreSortingOrder();
        secondUi?.RestoreSortingOrder();

        InputManager.tryDetectHover();
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

        instance.isAnimating = true;
        await Task.WhenAll(tasks);
        instance.isAnimating = false;
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

    public static void addStatus(Vector2Int position, ChessStatus status)
    {
        ChessPiece targetPiece = getChessPieceAtPosition(position);
        targetPiece.addStatus(status);
        refreshPieceStatusFx(targetPiece);
    }

    public static void addStatus(ChessPiece targetPiece, ChessStatus status)
    {
        targetPiece.addStatus(status);
        refreshPieceStatusFx(targetPiece);
        refreshPieceStatusFx(targetPiece);
    }

    public static void cleanUpChessExpiredStatuses(int currentTotalTurn)
    {
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (getChessPieceAtPosition(i, j) != null)
                {
                    ChessPiece chess = getChessPieceAtPosition(i, j);
                    chess.cleanUpExpiredStatuses(currentTotalTurn);
                    refreshPieceStatusFx(chess);
                }

    }

    public static void refreshAllPiecesStatusFx()
    {
        int currentTurnCount = GameManager.getCurrentTurnCount();

        foreach (var pair in instance.pieceToUiMap)
            pair.Value.UpdateStatusFx(currentTurnCount);
    }

    public static void refreshPieceStatusFx(ChessPiece piece)
    {
        if (!piece.IsVirtual())
            getUiOfChess(piece).UpdateStatusFx(GameManager.getCurrentTurnCount());
    }

    private static void updateBoardUiWhenPlayerUseCard(bool isLocal, Card card)
    {
        refreshAllPiecesStatusFx();
    }
}
