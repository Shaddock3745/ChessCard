using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckEditItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMPro.TextMeshProUGUI cardNameText, cardNumberText;
    [SerializeField] private ButtonVisibilityController addButton, minusButton;
    [SerializeField] private Image backgroundImage;

    private static readonly int MIN_CARD_NUMBER = 0, MAX_CARD_NUMBER = 3;
    private static readonly float HIGHTLIGHT_ALPHA = 0.8f, OVERLOOK_ALPHA = 0.4f;

    private int cardId;
    private string cardName;
    private int currentCardNumber;

    public void initData(int cardId, string cardName, int currentCardNumber)
    {
        this.cardId = cardId;
        this.cardName = cardName;
        this.currentCardNumber = currentCardNumber;

        this.cardNameText.text = cardName;
        updateView();
        overlook();
    }

    public void highlight()
    {
        Color c = backgroundImage.color;
        c.a = HIGHTLIGHT_ALPHA;
        backgroundImage.color = c;
    }

    public void overlook()
    {
        Color c = backgroundImage.color;
        c.a = OVERLOOK_ALPHA;
        backgroundImage.color = c;
    }

    private void updateView()
    {
        this.cardNumberText.text = currentCardNumber.ToString();

        minusButton.setVisibility(currentCardNumber != MIN_CARD_NUMBER);
        addButton.setVisibility(currentCardNumber != MAX_CARD_NUMBER);
    }

    public void onAddButtonClick()
    {
        if (++currentCardNumber >= MAX_CARD_NUMBER)
            currentCardNumber = MAX_CARD_NUMBER;

        updateView();
        DeckEditMenuManager.onCardNumberChange(cardId, currentCardNumber);
    }

    public void onMinusButtonClick()
    {
        if (--currentCardNumber <= MIN_CARD_NUMBER)
            currentCardNumber = MIN_CARD_NUMBER;

        updateView();
        DeckEditMenuManager.onCardNumberChange(cardId, currentCardNumber);
    }

    public void setCurrentCardNumber(int currentCardNumber)
    {
        this.currentCardNumber = currentCardNumber;
        updateView();
    }

    public int getCardId() => cardId;

    public string getCardName() => cardName;

    public int getCurrentCardNumber() => currentCardNumber;

    public void OnPointerEnter(PointerEventData eventData)
    {
        DeckEditMenuManager.hoverEnter(this.cardId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DeckEditMenuManager.hoverExit(this.cardId);
    }
}
