using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class HandViewManager : MonoBehaviour
{
    [SerializeField] private Canvas handCardCanvas;
    [SerializeField] private GameObject cardPrefab;

    private static HandViewManager instance;
    private static readonly float SHIFT_X = 60f, LIFT_Y = 50f, OFFSET_Y = 30f;
    private static readonly float CARD_WIDTH = 240f;
    private static readonly float MAX_SPACING = CARD_WIDTH * 1.125f;
    private static readonly float DRAW_ANIMATION_DURATION = 0.5f, SORT_ANIMATION_DURATION = 0.5f, FLUSH_ANIMATION_DURATION = 0.5f;

    private Sequence drawCardAnimationSequence;
    private List<CardUi> cardUiList = new List<CardUi>();
    private int hoveredCardIndex = -1;
    private bool isAnimating = false;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
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

    public static void drawCard(Card card)
    {
        CardUi cardUi = Instantiate(instance.cardPrefab, instance.handCardCanvas.transform).GetComponent<CardUi>();
        cardUi.GetComponent<CardUi>().setCard(card);
        instance.cardUiList.Add(cardUi);

        RectTransform rect = cardUi.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-Screen.width / 2f, 0f);
        rect.localScale = Vector3.zero;

        if (instance.drawCardAnimationSequence == null || !instance.drawCardAnimationSequence.IsActive() || instance.drawCardAnimationSequence.IsComplete())
        {
            instance.isAnimating = true;
            instance.drawCardAnimationSequence = DOTween.Sequence().OnComplete(() =>
            {
                instance.sort();
                instance.isAnimating = false;
            });
        }

        instance.drawCardAnimationSequence.Append(rect.DOAnchorPos(new Vector2(0, OFFSET_Y), DRAW_ANIMATION_DURATION).SetEase(Ease.OutBack));
        instance.drawCardAnimationSequence.Join(rect.DOScale(Vector3.one, DRAW_ANIMATION_DURATION));
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
                    targetX -= SHIFT_X;
                else if (i > hoveredCardIndex)
                    targetX += SHIFT_X;
                else
                {
                    targetY += LIFT_Y;
                    cardUiList[i].transform.SetAsLastSibling();
                }
            else
                cardUiList[i].transform.SetSiblingIndex(i);

            cardUiList[i].DOKill();
            cardUiList[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(targetX, targetY), SORT_ANIMATION_DURATION).SetEase(Ease.OutCubic);
        }
    }

    public static void hoverCard(CardUi cardUi)
    {
        if (!instance.isAnimating)
        {
            instance.hoveredCardIndex = instance.cardUiList.IndexOf(cardUi);
            instance.sort();
        }
    }

    public static void useCard(CardUi cardUi)
    {
        if (!instance.isAnimating)
        {
            instance.cardUiList.Remove(cardUi);
            Destroy(cardUi.gameObject);
            instance.hoveredCardIndex = -1;
            instance.sort();
        }
    }

    public static void rearrange()
    {
        if (!instance.isAnimating)
        {
            instance.hoveredCardIndex = -1;
            instance.sort();
        }
    }

    public static void flush()
    {
        instance.isAnimating = true;
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < instance.cardUiList.Count; i++)
        {
            RectTransform rect = instance.cardUiList[i].GetComponent<RectTransform>();
            seq.Join(rect.DOAnchorPos(new Vector2(-Screen.width / 2f, OFFSET_Y), FLUSH_ANIMATION_DURATION).SetEase(Ease.OutBack).SetDelay(0.02f * i));
            seq.Join(rect.DOScale(Vector3.zero, FLUSH_ANIMATION_DURATION).SetDelay(0.02f * i));
        }

        instance.cardUiList.Clear();

        seq.OnComplete(() =>
        {
            foreach (Transform child in instance.handCardCanvas.transform)
                Destroy(child.gameObject);

            instance.isAnimating = false;
        });
    }
}
