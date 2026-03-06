using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class HandViewManager : MonoBehaviour
{
    [SerializeField] private Canvas handCardCanvas;
    [SerializeField] private GameObject cardPrefab;

    private static HandViewManager instance;
    private static readonly float SHIFT_X = 60f, LIFT_Y = 50f, OFFSET_Y = 30f;
    private static readonly float CARD_WIDTH = 240f;
    private static readonly float MAX_SPACING = CARD_WIDTH * 1.125f;

    private List<CardUi> cardUiList = new List<CardUi>();
    private int hoveredCardIndex = -1;

    private void Awake()
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

    public static void drawCard(Card card)
    {
        CardUi cardUi = Instantiate(instance.cardPrefab, instance.handCardCanvas.transform).GetComponent<CardUi>();
        cardUi.GetComponent<CardUi>().setCard(card);
        instance.cardUiList.Add(cardUi);

        RectTransform rect = cardUi.GetComponent<RectTransform>();

        float startX = -Screen.width / 2f, startY = 0f;

        rect.anchoredPosition = new Vector2(startX, startY);
        rect.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOAnchorPos(new Vector2(0, OFFSET_Y), 0.5f).SetEase(Ease.OutBack));
        seq.Join(rect.DOScale(Vector3.one, 0.5f));

        seq.OnComplete(() => instance.sort());
    }

    private void sort()
    {
        int n = cardUiList.Count;

        if (n == 0)
            return;

        float MAX_WIDTH = Screen.width / 3f, startX = 0, spacing = 0;

        if (n > 1)
        {
            spacing = Mathf.Min(MAX_SPACING, MAX_WIDTH / n);
            startX = -(spacing * (n - 1) / 2f);
        }

        for (int i = 0; i < n; i++)
        {
            float targetX = startX + spacing * i;
            float targetY = OFFSET_Y;

            if (hoveredCardIndex != -1)
                if (i < hoveredCardIndex)
                    targetX-= SHIFT_X;
                else if (i > hoveredCardIndex)
                    targetX+= SHIFT_X;
                else
                {
                    targetY+= LIFT_Y;
                    cardUiList[i].transform.SetAsLastSibling();
                }
            else
                cardUiList[i].transform.SetSiblingIndex(i);

            cardUiList[i].DOKill();
            cardUiList[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(targetX, targetY), 0.5f).SetEase(Ease.OutCubic);
        }
    }

    public static void hoverCard(CardUi cardUi)
    {
        instance.hoveredCardIndex = instance.cardUiList.IndexOf(cardUi);
        instance.sort();
    }

    public static void rearrange()
    {
        instance.hoveredCardIndex = -1;
        instance.sort();
    }

    public static void flush()
    {
        float targetX = -Screen.width / 2f;

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < instance.cardUiList.Count; i++)
        {
            seq.Join(instance.cardUiList[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(targetX, OFFSET_Y), 0.5f).SetEase(Ease.OutBack).SetDelay(0.02f * i));
        }

        instance.cardUiList.Clear();

        seq.OnComplete(() =>
        {
            foreach (Transform child in instance.handCardCanvas.transform)
                Destroy(child.gameObject);
        });
    }
}
