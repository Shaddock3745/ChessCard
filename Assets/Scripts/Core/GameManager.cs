using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI nowTurn;

    private static GameManager instance;

    private bool isWhiteTurn = true;
    private bool isLocalWhite = true; // This should be set based on local selection

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            QualitySettings.vSyncCount = 1;
        }
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
        instance.nowTurn.text = instance.isWhiteTurn ? "White" : "Black";

        if (IsLocalTurn())
        {
            MatchDataManager.turnEnd(false);
            MatchDataManager.turnStart(true);

            if(MatchDataManager.isAI(true))
                await AiManager.doBestMove(MatchDataManager.getPlayerData(true));
        }
        else
        {
            MatchDataManager.turnEnd(true);
            MatchDataManager.turnStart(false);

            if(MatchDataManager.isAI(false))
                await AiManager.doBestMove(MatchDataManager.getPlayerData(false));
        }
    }

    public static bool IsLocalTurn()
    {
        return instance.isWhiteTurn == instance.isLocalWhite;
    }

    public static bool IsLocalWhite()
    {
        return instance.isLocalWhite;
    }
}
