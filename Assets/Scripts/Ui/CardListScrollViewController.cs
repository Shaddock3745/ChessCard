using UnityEngine;
using UnityEngine.UI;

public class CardListScrollViewController : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private RectTransform contentTransform;
    [SerializeField] private RectTransform scrollViewRectTransform;
    [SerializeField] private ScrollRect scrollRect;

    [SerializeField] private float minHeight = 200f;
    [SerializeField] private float maxHeight = 600f;

    public DeckEditItem spawnDeckEditItem(int cardId, string cardName, int currentCardNumber = 0)
    {
        DeckEditItem newItem = Instantiate(itemPrefab, contentTransform).GetComponent<DeckEditItem>();
        newItem.initData(cardId, cardName, currentCardNumber);
        UpdateLayoutAndScrollbar();

        return newItem;
    }

    private void UpdateLayoutAndScrollbar()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
        scrollViewRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Clamp(contentTransform.rect.height, minHeight, maxHeight));

        if (scrollRect != null)
        {
            scrollRect.CalculateLayoutInputVertical();
            scrollRect.SetLayoutVertical();
        }
    }
}

