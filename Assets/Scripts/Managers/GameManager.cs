using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI nowTurn;

    private static GameManager instance;

    private bool isWhiteTurn = true;
    private bool isPlayerWhite = true; // This should be set based on player selection

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

    public static async void ChangeTurn()
    {
        instance.isWhiteTurn = !instance.isWhiteTurn;
        instance.nowTurn.text = instance.isWhiteTurn ? "White": "Black"; 
        Debug.Log("Turn changed. It is now " + (instance.isWhiteTurn ? "White's" : "Black") + " turn.");

        if (!IsPlayerTurn())
            await AIManager.doBestMove(instance.isWhiteTurn);
    }

    public static bool IsPlayerTurn()
    {
        return instance.isWhiteTurn == instance.isPlayerWhite;
    }
}
