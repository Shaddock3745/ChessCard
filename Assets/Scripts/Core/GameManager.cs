using UnityEngine;
using UnityEngine.InputSystem;

public enum Language
{
    TRADITIONAL_CHINESE = 0,
    ENGLISH = 1,
    JAPANESE = 2
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI nowTurn;

    private static GameManager instance;

    private bool isWhiteTurn = true;
    private bool isLocalWhite = true; // This should be set based on local selection
    private Language currentLanguage = Language.TRADITIONAL_CHINESE; // Default language

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
        BoardManager.initBoard();

        MatchDataManager.initLocal(PlayerDataManager.initLocalPlayerData(false, true));
        MatchDataManager.initEnemy(AiManager.initAIPlayerData(false, false));

        startFirstTurn();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void startFirstTurn()
    {
        isWhiteTurn = true;

        if (IsLocalTurn())
        {
            MatchDataManager.turnStart(true);
            UiManager.showChangeTurnButton();
        }
        else
        {
            MatchDataManager.turnStart(false);
            _ = AiManager.doBestMove(MatchDataManager.getPlayerData(false));
        }
    }

    public static async void ChangeTurn()
    {
        instance.isWhiteTurn = !instance.isWhiteTurn;
        instance.nowTurn.text = instance.isWhiteTurn ? "White" : "Black";

        if (IsLocalTurn())
        {
            MatchDataManager.turnEnd(false);
            MatchDataManager.turnStart(true);
            UiManager.showChangeTurnButton();

            if (MatchDataManager.isAI(true))
                await AiManager.doBestMove(MatchDataManager.getPlayerData(true));
        }
        else
        {
            MatchDataManager.turnEnd(true);
            MatchDataManager.turnStart(false);
            UiManager.hideChangeTurnButton();

            if (MatchDataManager.isAI(false))
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

    public static Language getCurrentLanguage()
    {
        if (instance != null)
            return instance.currentLanguage;
        else
            return Language.ENGLISH; // Default language if instance is null
    }
}
