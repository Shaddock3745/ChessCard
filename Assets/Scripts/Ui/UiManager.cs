using System.Linq;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    private static UiManager instance;

    private static readonly string WHITE_WINS_TEMPLATE = "White Wins", BLACK_WINS_TEMPLATE = "Black Wins";

    [SerializeField] private TMPro.TextMeshProUGUI deckCount, waitingCount, discardCount, manaCount, holdCount;
    [SerializeField] private GameObject hoveredObjectDataPanel, logPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMPro.TextMeshProUGUI gameOverText;
    [SerializeField] private GameObject changeTurnButton, showLogPanelButton, holdCardButton;
    [SerializeField] private TMPro.TextMeshProUGUI hoveredObjectName, hoveredObjectDescription;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            hideGameOverPanel();
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        MatchDataManager.OnLocalCardCountChange += updateCardCount;
        MatchDataManager.OnLocalManaCountChange += updateManaCount;
        GameManager.OnTurnStart += updateButtonActive;
        GameManager.OnGameOver += showGameOverPanel;
        GameManager.OnResetGame += resetAllUi;
    }

    private void OnDisable()
    {
        MatchDataManager.OnLocalCardCountChange -= updateCardCount;
        MatchDataManager.OnLocalManaCountChange -= updateManaCount;
        GameManager.OnTurnStart -= updateButtonActive;
        GameManager.OnGameOver -= showGameOverPanel;
        GameManager.OnResetGame -= resetAllUi;
    }

    private static void showGameOverPanel(bool isWhiteWin)
    {
        instance.gameOverText.text = isWhiteWin ? WHITE_WINS_TEMPLATE : BLACK_WINS_TEMPLATE;
        instance.gameOverPanel.SetActive(true);
    }

    private static void hideGameOverPanel()
    {
        instance.gameOverPanel.SetActive(false);
    }

    private static void updateCardCount()
    {
        instance.deckCount.text = MatchDataManager.getPlayerDeckCardCount(true).ToString();
        instance.holdCount.text = MatchDataManager.getPlayerHoldingCardCount(true).ToString();
        instance.discardCount.text = MatchDataManager.getPlayerDiscardCardCount(true).ToString();
        instance.waitingCount.text = MatchDataManager.getPlayerWaitingCardCount(true).ToString();
    }

    private static void updateManaCount()
    {
        instance.manaCount.text = MatchDataManager.getPlayerMana(true).ToString();
    }

    private static void resetAllUi()
    {
        updateManaCount();
        updateCardCount();
        hideHoveredObjectDataPanel();
    }

    private static void updateButtonActive(int currentTurnCount, bool isWhiteTurn)
    {
        if (GameManager.IsLocalWhite() == isWhiteTurn)
        {
            showHoldCardButton();
            showChangeTurnButton();
        }
        else
        {
            hideChangeTurnButton();
            hideHoldCardButton();
        }
    }

    private static void showHoveredObjectDataPanel(string name, string description)
    {
        instance.hoveredObjectName.text = name;
        instance.hoveredObjectDescription.text = description;
        instance.hoveredObjectDataPanel.SetActive(true);
    }

    public static void hideHoveredObjectDataPanel()
    {
        instance.hoveredObjectDataPanel.SetActive(false);
    }

    public static void processHoveredObjectData(ChessPiece piece, bool isOnBoard)
    {
        if (piece != null)
        {
            string description = piece.getDescription();

            if (isOnBoard)
                description += MatchDataManager.canMoveChess(true, piece, GameManager.getCurrentTurnCount()) ? "\n\n(可移動)" : "";

            showHoveredObjectDataPanel(piece.getName(), description);
        }
    }

    public static void processHoveredObjectData(Card card, bool isInHand)
    {
        if (card != null)
        {
            string description = card.getDescription();

            if (isInHand)
                description += MatchDataManager.canUseCard(true, card, GameManager.getCurrentTurnCount()) ? "\n\n(可使用)" : "\n\n(條件不符)";

            showHoveredObjectDataPanel(card.getName(), description);
        }
    }

    private static void showHoldCardButton()
    {
        instance.holdCardButton.SetActive(true);
    }

    private static void hideHoldCardButton()
    {
        instance.holdCardButton.SetActive(false);
    }

    private static void showChangeTurnButton()
    {
        instance.changeTurnButton.SetActive(true);
    }

    private static void hideChangeTurnButton()
    {
        instance.changeTurnButton.SetActive(false);
    }

    public static void showLogPanel()
    {
        instance.logPanel.SetActive(true);
        instance.showLogPanelButton.SetActive(false);
    }

    public static void hideLogPanel()
    {
        instance.logPanel.SetActive(false);
        instance.showLogPanelButton.SetActive(true);
    }
}
