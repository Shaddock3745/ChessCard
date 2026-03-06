using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CardUi : MonoBehaviour
{
    private Card card;
    private RectTransform rectTransform;

    [SerializeField] private TMPro.TextMeshProUGUI nameText;
    [SerializeField] private TMPro.TextMeshProUGUI costText;
    [SerializeField] private Image illusory;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void setCard(Card card)
    {
        this.card = card;
        nameText.text = card.getName();
        costText.text = card.getCost().ToString();
        illusory.gameObject.SetActive(card.isExileOnUseCard());
    }

    public Card getCard() => card;

    public void moveToLocalPosition(Vector2 localPosition)
    {
        DOKill();
        rectTransform.localPosition = localPosition;
    }

    public int DOKill()
    {
        return rectTransform.DOKill();
    }
}
