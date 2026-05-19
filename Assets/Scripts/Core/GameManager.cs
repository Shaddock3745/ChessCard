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
    private Language currentLanguage = Language.TRADITIONAL_CHINESE;

    public static event Action<int> OnTurnStart;
    public static event Action<int> OnTurnEnd;

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
        isWhiteTurn = true;
        turnCount = 1;
        OnTurnStart?.Invoke(turnCount);

        if (IsLocalTurn())
        {
            MatchDataManager.turnStart(true);
            UiManager.showChangeTurnButton();
        }
        else
        {
            MatchDataManager.turnStart(false);
            _ = AiManager.doBestMove(MatchDataManager.getPlayerData(false), getCurrentTurnCount());
        }
    }

    public static async void ChangeTurn()
    {
        bool isLocalTurn = IsLocalTurn();
        OnTurnEnd?.Invoke(instance.turnCount);
        MatchDataManager.turnEnd(isLocalTurn);

        instance.isWhiteTurn = !instance.isWhiteTurn;
        isLocalTurn = IsLocalTurn();
        instance.turnCount++;

        OnTurnStart?.Invoke(instance.turnCount);
        instance.nowTurn.text = instance.isWhiteTurn ? "White" : "Black";
        MatchDataManager.turnStart(isLocalTurn);
        BoardManager.refreshAllPiecesStatusFx();

        if (isLocalTurn)
            UiManager.showChangeTurnButton();
        else
            UiManager.hideChangeTurnButton();

        if (MatchDataManager.isAI(isLocalTurn))
            await AiManager.doBestMove(MatchDataManager.getPlayerData(isLocalTurn), getCurrentTurnCount());
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
            return Language.ENGLISH;
    }
}
