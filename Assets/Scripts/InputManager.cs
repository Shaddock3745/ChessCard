using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [SerializeField] private InputActionReference clickReference;
    [SerializeField] private InputActionReference positionReference;

    private static InputManager instance;

    private ChessPiece selectedPiece = null;
    private bool isMouseHeld = false;

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
    }


    private void onMouseHeldStarted(InputAction.CallbackContext context)
    {
        isMouseHeld = true;
        Ray ray = Camera.main.ScreenPointToRay(positionReference.action.ReadValue<Vector2>());

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            if (hit.collider.TryGetComponent<ChessPiece>(out ChessPiece piece))
            {
                selectedPiece = piece;
                Debug.Log("Selected piece: " + piece.name);
            }
    }
    
    private void onMouseHeldEnded(InputAction.CallbackContext context)
    {
        isMouseHeld = false;
    }

}
