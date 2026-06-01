using System;
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

    private int turnCount;
    private bool isWhiteTurn = true;
    private bool isLocalWhite = true;
    private bool isGameOver = false;
    private Language currentLanguage = Language.TRADITIONAL_CHINESE;

    public static event Action<int, bool> OnTurnStart;
    public static event Action<int, bool> OnTurnEnd;
    public static event Action<bool> OnGameOver;
    public static event Action OnResetGame;

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

    public static int getCurrentTurnCount()
    {
        return instance.turnCount;
    }

    private void startFirstTurn()
    {
        isGameOver = false;
        isWhiteTurn = true;
        turnCount = 1;
        OnTurnStart?.Invoke(turnCount, isWhiteTurn);

        if (IsLocalTurn())
            MatchDataManager.turnStart(true);
        else
        {
            MatchDataManager.turnStart(false);
            _ = AiManager.doBestMove(MatchDataManager.getPlayerData(false), getCurrentTurnCount());
        }
    }

    public static async void ChangeTurn()
    {
        bool isLocalTurn = IsLocalTurn();
        OnTurnEnd?.Invoke(instance.turnCount, instance.isWhiteTurn);
        MatchDataManager.turnEnd(isLocalTurn);

        instance.isWhiteTurn = !instance.isWhiteTurn;
        isLocalTurn = IsLocalTurn();
        instance.turnCount++;

        OnTurnStart?.Invoke(instance.turnCount, instance.isWhiteTurn);
        instance.nowTurn.text = instance.isWhiteTurn ? "White" : "Black";
        MatchDataManager.turnStart(isLocalTurn);
        BoardManager.refreshAllPiecesStatusFx();

        if (MatchDataManager.isAI(isLocalTurn))
            await AiManager.doBestMove(MatchDataManager.getPlayerData(isLocalTurn), getCurrentTurnCount());
    }

    private void resetGame()
    {
        turnCount = 0;
    }

    public static bool IsLocalTurn()
    {
        return instance.isWhiteTurn == instance.isLocalWhite;
    }

    public static bool IsLocalWhite()
    {
        return instance.isLocalWhite;
    }

    public static bool IsGameOver()
    {
        return instance.isGameOver;
    }

    public static Language getCurrentLanguage()
    {
        if (instance != null)
            return instance.currentLanguage;
        else
            return Language.ENGLISH;
    }

    public static void triggerOnGameOver(bool isWhiteWin)
    {
        instance.isGameOver = true;
        OnGameOver?.Invoke(isWhiteWin);
    }

    public static void triggerOnResetGame()
    {
        instance.resetGame(); 
        BoardManager.initBoard();
        MatchDataManager.initLocal(PlayerDataManager.initLocalPlayerData(false, true));
        MatchDataManager.initEnemy(AiManager.initAIPlayerData(false, false));
        OnResetGame?.Invoke();
    }
}
