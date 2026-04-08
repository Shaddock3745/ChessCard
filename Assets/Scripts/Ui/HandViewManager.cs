using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class HandViewManager : MonoBehaviour
{
    [SerializeField] private Canvas playerHandCardCanvas, enemyHandCardCanvas;
    [SerializeField] private GameObject cardPrefab, cardBackPrefab;

    private static HandViewManager instance;
    private static readonly float SHIFT_X = 60f, LIFT_Y = 50f, OFFSET_Y = 30f;
    private static readonly float USING_CARD_X = 800f, USING_CARD_Y = 500f;
    private static readonly Vector2 usingCardPos = new Vector2(USING_CARD_X, USING_CARD_Y);
    private static readonly float CARD_WIDTH = 240f;
    private static readonly float MAX_SPACING = CARD_WIDTH * 1.125f;
    private static readonly float DRAW_ANIMATION_DURATION = 0.5f, SORT_ANIMATION_DURATION = 0.5f, FLUSH_ANIMATION_DURATION = 0.5f, USE_ANIMATION_DURATION = 0.2f;
    private static readonly float FLUSH_DELAY = 0.02f, DRAW_DELAY = 0.08f;

    private Sequence playerDrawCardAnimationSequence, enemyDrawCardAnimationSequence;
    private CardUi usingCard = null;
    private int usingCardIndex = -1;
    private List<CardUi> playerCardUiList = new List<CardUi>();
    private List<GameObject> enemyCardBackList = new List<GameObject>();
    private int hoveredCardIndex = -1;
    private bool isPlayerAnimating = false, isEnemyAnimating = false;
    private float currentDrawDelay = 0f;

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

    public static bool isPlayingAnimation() => instance.isPlayerAnimating || instance.isEnemyAnimating;

    public static void playerDrawCard(Card card)
    {
        CardUi cardUi = Instantiate(instance.cardPrefab, instance.playerHandCardCanvas.transform).GetComponent<CardUi>();
        cardUi.GetComponent<CardUi>().setCard(card);
        instance.playerCardUiList.Add(cardUi);

        RectTransform rect = cardUi.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-Screen.width / 2f, 0f);
        rect.localScale = Vector3.zero;

        if (instance.playerDrawCardAnimationSequence == null || !instance.playerDrawCardAnimationSequence.IsActive() || instance.playerDrawCardAnimationSequence.IsComplete())
        {
            instance.isPlayerAnimating = true;
            instance.currentDrawDelay = 0f;
            instance.playerDrawCardAnimationSequence = DOTween.Sequence().OnComplete(() =>
            {
                instance.sortPlayerHand();
                DOVirtual.DelayedCall(SORT_ANIMATION_DURATION, () => instance.isPlayerAnimating = false);
            });
        }

        float startTime = instance.currentDrawDelay;

        instance.playerDrawCardAnimationSequence.Insert(startTime, rect.DOAnchorPos(new Vector2(0, OFFSET_Y), DRAW_ANIMATION_DURATION).SetEase(Ease.OutBack));
        instance.playerDrawCardAnimationSequence.Insert(startTime, rect.DOScale(Vector3.one, DRAW_ANIMATION_DURATION));

        instance.currentDrawDelay += DRAW_DELAY;
    }

    public static void enemyDrawCard()
    {
        GameObject cardBack = GameObject.Instantiate(instance.cardBackPrefab, instance.enemyHandCardCanvas.transform);
        instance.enemyCardBackList.Add(cardBack);

        RectTransform rect = cardBack.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-Screen.width / 2f, 0f);
        rect.localScale = Vector3.zero;

        if (instance.enemyDrawCardAnimationSequence == null || !instance.enemyDrawCardAnimationSequence.IsActive() || instance.enemyDrawCardAnimationSequence.IsComplete())
        {
            instance.isEnemyAnimating = true;
            instance.currentDrawDelay = 0f;
            instance.enemyDrawCardAnimationSequence = DOTween.Sequence().OnComplete(() =>
            {
                instance.sortEnemyHand();
                DOVirtual.DelayedCall(SORT_ANIMATION_DURATION, () => instance.isEnemyAnimating = false);
            });
        }

        float startTime = instance.currentDrawDelay;

        instance.enemyDrawCardAnimationSequence.Insert(startTime, rect.DOAnchorPos(new Vector2(0, -OFFSET_Y), DRAW_ANIMATION_DURATION)).SetEase(Ease.OutBack);
        instance.enemyDrawCardAnimationSequence.Insert(startTime, rect.DOScale(Vector3.one, DRAW_ANIMATION_DURATION));

        instance.currentDrawDelay += DRAW_DELAY;
    }

    private void sortPlayerHand()
    {
        int n = playerCardUiList.Count;

        if (n == 0)
            return;

        float maxWidth = Screen.width / 3f, startX = 0f, spacing = 0f;

        if (n > 1)
        {
            spacing = Mathf.Min(MAX_SPACING, maxWidth / n);
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
                    playerCardUiList[i].transform.SetAsLastSibling();
                }
            else
                playerCardUiList[i].transform.SetSiblingIndex(i);

            playerCardUiList[i].DOKill();
            playerCardUiList[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(targetX, targetY), SORT_ANIMATION_DURATION).SetEase(Ease.OutCubic);
        }
    }

    private void sortEnemyHand()
    {
        int n = enemyCardBackList.Count;

        if (n == 0)
            return;

        float maxWidth = Screen.width / 3f, startX = 0f, spacing = 0f;

        if (n > 1)
        {
            spacing = Mathf.Min(MAX_SPACING, maxWidth / n);
            startX = -(spacing * (n - 1) / 2f);
        }

        for (int i = 0; i < n; i++)
        {
            float targetX = startX + spacing * i;

            RectTransform rect = enemyCardBackList[i].GetComponent<RectTransform>();
            rect.DOKill();
            rect.DOAnchorPos(new Vector2(targetX, -OFFSET_Y), SORT_ANIMATION_DURATION);
        }
    }

    public static void hoverCard(CardUi cardUi)
    {
        if (!instance.isPlayerAnimating)
        {
            instance.hoveredCardIndex = instance.playerCardUiList.IndexOf(cardUi);
            instance.sortPlayerHand();
        }
    }

    public static void playerUseCard(CardUi cardUi)
    {
        if (!instance.isPlayerAnimating)
        {
            instance.usingCard = cardUi;
            instance.usingCardIndex = instance.playerCardUiList.IndexOf(cardUi);

            instance.playerCardUiList.Remove(cardUi);
            instance.hoveredCardIndex = -1;
            cardUi.transform.SetAsLastSibling();
            RectTransform rect = cardUi.GetComponent<RectTransform>();
            rect.DOKill();
            rect.anchoredPosition = usingCardPos;
            instance.sortPlayerHand();
        }
    }

    public static void enemyUseCard(Card card)
    {
        if (instance.enemyCardBackList.Count <= 0)
            return;

        CardUi cardUi = Instantiate(instance.cardPrefab, instance.enemyHandCardCanvas.transform).GetComponent<CardUi>();
        cardUi.GetComponent<CardUi>().setCard(card);
        instance.usingCard = cardUi;
        cardUi.transform.SetAsLastSibling();
        RectTransform rect = cardUi.GetComponent<RectTransform>();
        rect.DOKill();
        rect.anchoredPosition = usingCardPos;

        GameObject cardBackToConsume = instance.enemyCardBackList[0];
        instance.enemyCardBackList.RemoveAt(0);
        cardBackToConsume.GetComponent<RectTransform>().DOKill();

        cardBackToConsume.GetComponent<RectTransform>().DOScale(Vector3.zero, USE_ANIMATION_DURATION).OnComplete(() => Destroy(cardBackToConsume));
        instance.sortEnemyHand();
    }

    public static void destroyUsingCard()
    {
        if (instance.usingCard != null)
        {
            instance.usingCard.GetComponent<RectTransform>().DOKill();
            Destroy(instance.usingCard.gameObject);
            instance.usingCard = null;
            instance.usingCardIndex = -1;
        }
    }

    public static void rearrangeUsingCard()
    {
        if (instance.usingCard != null)
        {
            instance.usingCard.gameObject.SetActive(true);
            instance.playerCardUiList.Insert(instance.usingCardIndex, instance.usingCard);
            instance.usingCard = null;
            instance.usingCardIndex = -1;
            instance.sortPlayerHand();
        }
    }

    public static void rearrangeHover()
    {
        if (!instance.isPlayerAnimating)
        {
            instance.hoveredCardIndex = -1;
            instance.sortPlayerHand();
        }
    }

    public static void flush(bool isPlayer)
    {
        if (isPlayer)
            instance.flushPlayerHand();
        else
            instance.flushEnemyHand();
    }

    private void flushPlayerHand()
    {
        isPlayerAnimating = true;
        instance.playerDrawCardAnimationSequence?.Kill();
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < playerCardUiList.Count; i++)
        {
            RectTransform rect = playerCardUiList[i].GetComponent<RectTransform>();
            rect.DOKill();
            seq.Join(rect.DOAnchorPos(new Vector2(-Screen.width / 2f, OFFSET_Y), FLUSH_ANIMATION_DURATION).SetEase(Ease.OutBack).SetDelay(FLUSH_DELAY * i));
            seq.Join(rect.DOScale(Vector3.zero, FLUSH_ANIMATION_DURATION).SetDelay(FLUSH_DELAY * i));
        }

        playerCardUiList.Clear();

        seq.OnComplete(() =>
        {
            foreach (Transform child in playerHandCardCanvas.transform)
            {
                child.DOKill();
                Destroy(child.gameObject);
            }

            isPlayerAnimating = false;
        });
    }

    private void flushEnemyHand()
    {
        isEnemyAnimating = true;
        instance.enemyDrawCardAnimationSequence?.Kill();
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < enemyCardBackList.Count; i++)
        {
            RectTransform rect = enemyCardBackList[i].GetComponent<RectTransform>();
            rect.DOKill();
            seq.Join(rect.DOAnchorPos(new Vector2(-Screen.width / 2f, -OFFSET_Y), FLUSH_ANIMATION_DURATION).SetEase(Ease.OutBack).SetDelay(FLUSH_DELAY * i));
            seq.Join(rect.DOScale(Vector3.zero, FLUSH_ANIMATION_DURATION).SetDelay(FLUSH_DELAY * i));
        }

        enemyCardBackList.Clear();

        seq.OnComplete(() =>
        {
            foreach (Transform child in enemyHandCardCanvas.transform)
            {
                child.DOKill();
                Destroy(child.gameObject);
            }

            isEnemyAnimating = false;
        });
    }
}
