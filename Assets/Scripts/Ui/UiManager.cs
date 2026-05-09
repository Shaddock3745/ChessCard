using System.Linq;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    private static UiManager instance;

    [SerializeField] private TMPro.TextMeshProUGUI deckCount, waitingCount, discardCount, manaCount;
    [SerializeField] private GameObject hoveredObjectDataPanel, logPanel;
    [SerializeField] private GameObject changeTurnButton, showLogPanelButton;
    [SerializeField] private TMPro.TextMeshProUGUI hoveredObjectName, hoveredObjectDescription;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    public static void updateDeckCount(int count)
    {
        instance.deckCount.text = count.ToString();
    }

    public static void updateWaitingCount(int count)
    {
        instance.waitingCount.text = count.ToString();
    }

    public static void updateDiscardCount(int count)
    {
        instance.discardCount.text = count.ToString();
    }

    public static void updateManaCount(int count)
    {
        instance.manaCount.text = count.ToString();
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


    public static void showChangeTurnButton()
    {
        instance.changeTurnButton.SetActive(true);
    }

    public static void hideChangeTurnButton()
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
