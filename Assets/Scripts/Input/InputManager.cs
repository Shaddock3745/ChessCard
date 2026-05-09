using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionReference clickReference, positionReference;
    [SerializeField] private TMPro.TextMeshProUGUI hoveredObjectName;
    [SerializeField] private LayerMask boardLayer;
    [SerializeField] private RectTransform handcardCanvaseRectTramsform;

    private static InputManager instance;
    private static readonly float MOVING_RELIEF = 10f;
    private static readonly int MOVING_SORTING_ORDER = 100;

    private TaskCompletionSource<BoardTile> targetTileSelectionTcs;
    private TaskCompletionSource<Card> targetCardSelectionTcs;
    private GameObject lastHovered = null;
    private ChessPieceUi selectedChess = null;
    private CardUi selectedCard = null;
    private BoardTile currentTile = null, lastTile = null, firstTile = null;
    private bool isSelectingChess = false, isSelectingCard = false;
    private bool isRaisingChess = false, isRaisingCard = false;
    private bool isMouseHolding = false, isMouseInner = false;
    private int originalSortingOrder;
    private bool isAwaitingChessSelection = false;
    private bool isAwaitingCardSelection = false;
    private bool externalCallDetectHoverFlag = false;

    void Awake()
    {
        transform.SetParent(null);

        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        clickReference.action.Enable();
        positionReference.action.Enable();

        clickReference.action.started += onMouseHeldStarted;
        clickReference.action.canceled += onMouseHeldEnded;
    }

    private void OnDisable()
    {
        clickReference.action.started -= onMouseHeldStarted;
        clickReference.action.canceled -= onMouseHeldEnded;

        clickReference.action.Disable();
        positionReference.action.Disable();
    }

    public static async Task<BoardTile> waitForBoardTileSelection(List<BoardTile> validTiles)
    {
        instance.isAwaitingChessSelection = true;
        instance.targetTileSelectionTcs = new TaskCompletionSource<BoardTile>();

        foreach (BoardTile tile in validTiles)
            tile.showSelectable();

        return await instance.targetTileSelectionTcs.Task;
    }

    public static async Task<Card> waitForCardSelection(List<Card> validCards, params int[] excludedIds)
    {
        instance.isAwaitingCardSelection = true;
        instance.targetCardSelectionTcs = new TaskCompletionSource<Card>();

        await HandViewManager.hidePlayerHandCards();
        await HandViewManager.showSelectingCardList(validCards, excludedIds);

        Card resultCard = await instance.targetCardSelectionTcs.Task;

        await HandViewManager.hideSelectingCardList();
        await HandViewManager.showPlayHandCards();

        return resultCard;
    }

    private void selectCard()
    {
        instance.isAwaitingCardSelection = false;
        instance.targetCardSelectionTcs?.TrySetResult(selectedCard.getCard());
    }

    private void cancelCardSelection()
    {
        instance.isAwaitingCardSelection = false;
        instance.targetCardSelectionTcs?.TrySetResult(null);
    }

    private void selectBoardTile()
    {
        instance.isAwaitingChessSelection = false;
        BoardManager.removeAllHint();
        instance.targetTileSelectionTcs?.TrySetResult(currentTile);
    }

    private void cancelBoardTileSelection()
    {
        instance.isAwaitingChessSelection = false;
        BoardManager.removeAllHint();
        instance.targetTileSelectionTcs?.TrySetResult(null);
    }

    private Vector2 getMousePosition()
    {
        return positionReference.action.ReadValue<Vector2>();
    }

    private Vector2 getLocalMousePosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(handcardCanvaseRectTramsform, getMousePosition(), Camera.main, out Vector2 localPoint);
        return localPoint;
    }

    void Update()
    {
        if (GameManager.IsLocalTurn())
        {
            if (isAwaitingChessSelection)
                updateWhenSelecting();
            else if (isMouseHolding)
                if (isRaisingChess)
                    movePieceWithMouse();
                else if (isRaisingCard)
                    moveCardWithMouse();
        }

        if (!isMouseHolding)
        {
            GameObject hoveredObject = detectHover();

            if (externalCallDetectHoverFlag || hoveredObject != lastHovered)
            {
                if (lastHovered != null)
                    handleExit(lastHovered);

                if (hoveredObject != null)
                    handleEnter(hoveredObject);

                lastHovered = hoveredObject;
                externalCallDetectHoverFlag = false;
            }
        }
    }

    private void onMouseHeldStarted(InputAction.CallbackContext context)
    {
        if (isAwaitingCardSelection)
        {
            if (isSelectingCard && selectedCard != null)
                selectCard();
            else
                cancelCardSelection();

            return;
        }
        else if (isAwaitingChessSelection)
        {
            BoardTile tile = getHoveredTile();

            if (tile != null)
                if (tile.isSelectableTile())
                    selectBoardTile();
                else
                    cancelBoardTileSelection();
            else
                cancelBoardTileSelection();

            return;
        }
        else
        {
            isMouseHolding = true;

            if (GameManager.IsLocalTurn())
            {
                if (isSelectingChess && MatchDataManager.canMoveChess(true, selectedChess.getChess(), GameManager.getCurrentTurnCount()))
                    raiseChess();

                if (isSelectingCard && MatchDataManager.canUseCard(true, selectedCard.getCard()))
                    raiseCard();
            }
        }
    }

    private void onMouseHeldEnded(InputAction.CallbackContext context)
    {
        isMouseHolding = false;

        if (isRaisingChess)
            releaseChess();

        if (isRaisingCard)
            releaseCard();
    }

    private void raiseChess()
    {
        originalSortingOrder = selectedChess.GetComponent<SpriteRenderer>().sortingOrder;
        selectedChess.GetComponent<SpriteRenderer>().sortingOrder = MOVING_SORTING_ORDER;
        showHint(selectedChess);
        isRaisingChess = true;
    }

    private void raiseCard()
    {
        BoardManager.showOuter(false);
        isRaisingCard = true;
    }

    private void updateWhenSelecting()
    {
        BoardTile tile = getHoveredTile();

        if (tile != null)
            if (currentTile != tile)
            {
                if (currentTile != null)
                {
                    lastTile = currentTile;
                }

                if (lastTile != null)
                    if (lastTile.isSelectableTile())
                        lastTile.showSelectable();

                if (tile.isSelectableTile())
                    tile.showTarget();

                currentTile = tile;
            }
    }

    private void updateWhenMovingChess()
    {
        BoardTile tile = getHoveredTile();

        if (tile != null)
            if (currentTile != tile)
            {
                if (currentTile != null)
                {
                    lastTile = currentTile;
                }

                if (lastTile != null)
                    if (lastTile.isMoveableTile())
                        lastTile.showMoveable();
                    else if (lastTile.isAttackableTile())
                        lastTile.showAttackable();

                if (tile.isMoveableTile() || tile.isAttackableTile())
                    tile.showTarget();

                currentTile = tile;
            }
    }

    private void updateWhenMovingCard()
    {
        isMouseInner = getHoveredTile() != null;
        BoardManager.showOuter(isMouseInner);
    }

    public BoardTile getHoveredTile()
    {
        Collider2D boardHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(getMousePosition()), boardLayer);

        if (boardHit != null)
        {
            BoardTile tile = boardHit.GetComponent<BoardTile>();

            if (tile != null)
                return tile;
        }
        return null;
    }

    public static void tryDetectHover()
    {
        if (instance != null)
            instance.externalCallDetectHoverFlag = true;
    }

    private GameObject detectHover()
    {
        if (EventSystem.current == null)
            return null;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = getMousePosition() }, raycastResults);

        foreach (RaycastResult result in raycastResults)
            if (result.gameObject.GetComponent<CardUi>() != null)
                return result.gameObject;

        Collider2D boardHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(getMousePosition()), boardLayer);

        if (boardHit != null)
            return boardHit.gameObject;

        return null;
    }

    private void handleEnter(GameObject obj)
    {
        if (obj.TryGetComponent<BoardTile>(out BoardTile tile))
        {
            firstTile = tile;
            selectedChess = BoardManager.GetChessPieceUiAtTile(tile);

            if (selectedChess != null)
            {
                isSelectingChess = true;
                isSelectingCard = false;
                UiManager.processHoveredObjectData(selectedChess.getChess(), true);
            }
            else
            {
                isSelectingChess = isSelectingCard = false;
            }
        }
        else if (obj.TryGetComponent<CardUi>(out CardUi cardUi))
        {
            isSelectingChess = false;
            isSelectingCard = true;
            selectedCard = cardUi;
            UiManager.processHoveredObjectData(cardUi.getCard(), true);

            if (isAwaitingCardSelection)
                _ = HandViewManager.hoverSelectingCard(cardUi);
            else
                _ = HandViewManager.hoverHandCard(cardUi);
        }
        else
        {
            isSelectingChess = isSelectingCard = false;
        }

    }

    private void handleExit(GameObject obj)
    {
        if (isSelectingCard)
            if (isAwaitingCardSelection)
                _ = HandViewManager.rearrangeSelectingCardHover();
            else
                _ = HandViewManager.rearrangeHandCardHover();

        isSelectingChess = isSelectingCard = false;
    }

    private void movePieceWithMouse()
    {
        Vector2 mousePos = getMousePosition();

        selectedChess.moveToPoint(Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, MOVING_RELIEF)));

        updateWhenMovingChess();
    }

    private void moveCardWithMouse()
    {
        Vector2 localMousePos = getLocalMousePosition();

        selectedCard.moveToLocalPosition(localMousePos);

        updateWhenMovingCard();
    }

    private void showHint(ChessPieceUi pieceUi)
    {
        var tempList = pieceUi.getChess().getAvailableMoves(MatchDataManager.canAttackThisTurn(true, GameManager.getCurrentTurnCount()));

        foreach (var move in tempList)
            switch (move.actionType)
            {
                case ActionType.Move:
                    BoardManager.showMoveable(move.targetPos);
                    break;
                case ActionType.Attack:
                    BoardManager.showAttackable(move.targetPos);
                    break;
            }
    }

    private void releaseChess()
    {
        if (currentTile == null)
        {
            if (selectedChess != null && firstTile != null)
                BoardManager.moveCanceled(selectedChess.getChess(), new Vector2Int(firstTile.getX(), firstTile.getY()));
        }
        else if (currentTile.isMoveableTile())
        {
            BoardManager.movePiece(new Vector2Int(firstTile.getX(), firstTile.getY()), new Vector2Int(currentTile.getX(), currentTile.getY()));
            MatchDataManager.doChessMoved(true, selectedChess.getChess(), GameManager.getCurrentTurnCount());
        }
        else if (currentTile.isAttackableTile())
        {
            BoardManager.attackPiece(new Vector2Int(firstTile.getX(), firstTile.getY()), new Vector2Int(currentTile.getX(), currentTile.getY()));
            MatchDataManager.doChessMoved(true, selectedChess.getChess(), GameManager.getCurrentTurnCount());
        }
        else
            BoardManager.moveCanceled(selectedChess.getChess(), new Vector2Int(firstTile.getX(), firstTile.getY()));

        isRaisingChess = false;
        currentTile = lastTile = null;

        BoardManager.removeAllHint();
        selectedChess.GetComponent<SpriteRenderer>().sortingOrder = originalSortingOrder;
    }

    private void releaseCard()
    {
        isRaisingCard = false;
        BoardManager.hideOuter();

        if (isMouseInner)
        {
            _ = HandViewManager.playerUseCard(selectedCard);
            _ = CardEffectManager.processLocalPlayerCard(selectedCard.getCard());
        }
        else
            _ = HandViewManager.rearrangeHandCardHover();
    }
}
