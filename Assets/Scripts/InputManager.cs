using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionReference clickReference;
    [SerializeField] private InputActionReference positionReference;

    private static InputManager instance;
    private static readonly float MOVING_RELIEF = 10f;

    private ChessPiece selectedChess = null;
    private BoardTile currentTile = null, lastTile = null, firstTile = null;
    private bool isHeldingChess = false;
    private bool isHeldingCard = false;

    void Awake()
    {
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isHeldingChess)
        {
            movePieceWithMouse();           
        }
    }

    private void movePieceWithMouse()
    {
        Vector2 mousePos = positionReference.action.ReadValue<Vector2>();

        selectedChess.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, MOVING_RELIEF));

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Board")))
            if (hit.collider.TryGetComponent<BoardTile>(out BoardTile tile))
            {
                if (currentTile != tile)
                {
                    lastTile = currentTile;
                    currentTile = tile;

                    if (tile.isMoveableTile() || tile.isAttackableTile())
                        tile.showTarget();

                    if (lastTile.isAttackableTile())
                        lastTile.showAttackable();
                    else if(lastTile.isMoveableTile())
                        lastTile.showMoveable();
                }
            }
    }


    private void onMouseHeldStarted(InputAction.CallbackContext context)
    {
        tryToCatchChess();
    }

    private void onMouseHeldEnded(InputAction.CallbackContext context)
    {
        if (isHeldingChess)
        {
            relaseChess();
            clearHint();
        }
    }

    private void tryToCatchChess()
    {
        Ray ray = Camera.main.ScreenPointToRay(positionReference.action.ReadValue<Vector2>());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Board")))
            if (hit.collider.TryGetComponent<BoardTile>(out BoardTile tile))
            {
                firstTile = currentTile = tile;
                isHeldingChess = true;
                selectedChess = BoardManager.getChessPieceAtTile(tile);
                selectedChess.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

                showHint(selectedChess);
            }
    }

    private void showHint(ChessPiece piece)
    {
        var tempList = piece.getAvailableMoves(new Vector2Int(firstTile.getX(),firstTile.getY()));

        foreach(var move in tempList)
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

    private void clearHint()
    {
        BoardManager.removeAllHint();
    }

    private void relaseChess()
    {
        if (currentTile.isMoveableTile())
            BoardManager.movePiece(new Vector2Int(firstTile.getX(), firstTile.getY()), new Vector2Int(currentTile.getX(), currentTile.getY()));
        else if(currentTile.isAttackableTile())
            BoardManager.attackPiece(new Vector2Int(firstTile.getX(), firstTile.getY()), new Vector2Int(currentTile.getX(), currentTile.getY()));
        else
            BoardManager.moveCanceled(selectedChess, new Vector2Int(firstTile.getX(), firstTile.getY()));

        isHeldingChess = false;
    } 

}
