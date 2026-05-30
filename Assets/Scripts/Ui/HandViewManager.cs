using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class HandViewManager : MonoBehaviour
{
    [SerializeField] private Canvas playerHandCardCanvas, enemyHandCardCanvas, selectingCardCanvas;
    [SerializeField] private GameObject cardPrefab, cardBackPrefab;

    private static HandViewManager instance;
    private static readonly float SHIFT_X = 60f, LIFT_Y = 50f, OFFSET_Y = 30f;
    private static readonly float USING_CARD_X = -500f, USING_CARD_Y = 600f;
    private static readonly Vector2 usingCardPos = new Vector2(USING_CARD_X, USING_CARD_Y);
    private static readonly float HIDDEN_Y = -400f;
    private static readonly float CARD_WIDTH = 240f;
    private static readonly float MAX_SPACING = CARD_WIDTH * 1.125f;
    private static readonly float DRAW_ANIMATION_DURATION = 0.5f, SORT_ANIMATION_DURATION = 0.5f, FLUSH_ANIMATION_DURATION = 0.5f, USE_ANIMATION_DURATION = 0.5f, HIDE_ANIMATION_DURATION = 0.5f, DISAPEAR_ANIMATION_DURATION = 0.2f, HOLD_ANIMATION_DURATION = 0.2f;
    private static readonly float FLUSH_DELAY = 0.02f, DRAW_DELAY = 0.08f;
    private static readonly float SELECTING_SPACING_X = 260f;
    private static readonly float SELECTING_SPACING_Y = 350f;
    private static readonly float SELECTING_OFFSET_Y = 800f;
    private static readonly int SELECTING_COLUMNS = 5;
    private static readonly float SELECTING_SCALE_NORMAL = 1f;
    private static readonly float SELECTING_SCALE_HOVER = 1.2f;

    private Sequence playerDrawCardAnimationSequence, enemyDrawCardAnimationSequence;
    private CardUi usingCard = null;
    private int usingCardIndex = -1, hoveredHandCardIndex = -1, hoveredSelectingCardIndex = -1;
    private List<CardUi> playerCardUiList = new List<CardUi>(), selectingCardUiList = new List<CardUi>();
    private List<GameObject> enemyCardBackList = new List<GameObject>();
    private bool isPlayerAnimating = false, isEnemyAnimating = false;
    private float currentDrawDelay = 0f;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public static bool isPlayingAnimation() => instance.isPlayerAnimating || instance.isEnemyAnimating;

    public static void recalculateCost()
    {
        foreach (CardUi cardUi in instance.playerCardUiList)
            cardUi.recalculateCost(MatchDataManager.isCardNonCost(true, GameManager.getCurrentTurnCount()), MatchDataManager.getExtraCardCost(true, GameManager.getCurrentTurnCount()));
    }

    public static async Task playerDrawCard(Card card)
    {
        CardUi cardUi = Instantiate(instance.cardPrefab, instance.playerHandCardCanvas.transform).GetComponent<CardUi>();
        cardUi.initCard(card);
        instance.playerCardUiList.Add(cardUi);

        RectTransform rect = cardUi.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(-Screen.width / 2f, 0f);
        rect.localScale = Vector3.zero;

        if (instance.playerDrawCardAnimationSequence == null || !instance.playerDrawCardAnimationSequence.IsActive() || instance.playerDrawCardAnimationSequence.IsComplete())
        {
            instance.isPlayerAnimating = true;
            instance.currentDrawDelay = 0f;
            instance.playerDrawCardAnimationSequence = DOTween.Sequence().OnComplete(async () =>
            {
                await instance.sortPlayerHand();
                instance.isPlayerAnimating = false;
            });
        }

        float startTime = instance.currentDrawDelay;

        instance.playerDrawCardAnimationSequence.Insert(startTime, rect.DOAnchorPos(new Vector2(0, OFFSET_Y), DRAW_ANIMATION_DURATION).SetEase(Ease.OutBack));
        instance.playerDrawCardAnimationSequence.Insert(startTime, rect.DOScale(Vector3.one, DRAW_ANIMATION_DURATION));

        instance.currentDrawDelay += DRAW_DELAY;
        await instance.playerDrawCardAnimationSequence.AsyncWaitForCompletion();
    }

    public static async Task enemyDrawCard()
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
            instance.enemyDrawCardAnimationSequence = DOTween.Sequence().OnComplete(async () =>
            {
                await instance.sortEnemyHand();
                instance.isEnemyAnimating = false;
            });
        }

        float startTime = instance.currentDrawDelay;

        instance.enemyDrawCardAnimationSequence.Insert(startTime, rect.DOAnchorPos(new Vector2(0, -OFFSET_Y), DRAW_ANIMATION_DURATION).SetEase(Ease.OutBack));
        instance.enemyDrawCardAnimationSequence.Insert(startTime, rect.DOScale(Vector3.one, DRAW_ANIMATION_DURATION));

        instance.currentDrawDelay += DRAW_DELAY;
        await instance.enemyDrawCardAnimationSequence.AsyncWaitForCompletion();
    }

    private async Task sortPlayerHand()
    {
        int n = playerCardUiList.Count;
        if (n == 0) return;

        float maxWidth = Screen.width / 3f, startX = 0f, spacing = 0f;
        if (n > 1)
        {
            spacing = Mathf.Min(MAX_SPACING, maxWidth / n);
            startX = -(spacing * (n - 1) / 2f);
        }

        List<Task> animTasks = new List<Task>();

        for (int i = 0; i < n; i++)
        {
            float targetX = startX + spacing * i;
            float targetY = OFFSET_Y;

            if (hoveredHandCardIndex != -1)
                if (i < hoveredHandCardIndex) targetX -= SHIFT_X;
                else if (i > hoveredHandCardIndex) targetX += SHIFT_X;
                else
                {
                    targetY += LIFT_Y;
                    playerCardUiList[i].transform.SetAsLastSibling();
                }
            else
                playerCardUiList[i].transform.SetSiblingIndex(i);

            RectTransform rect = playerCardUiList[i].GetComponent<RectTransform>();
            rect.DOKill();
            animTasks.Add(rect.DOAnchorPos(new Vector2(targetX, targetY), SORT_ANIMATION_DURATION).SetEase(Ease.OutCubic).AsyncWaitForCompletion());
        }

        await Task.WhenAll(animTasks);
    }

    private async Task sortEnemyHand()
    {
        int n = enemyCardBackList.Count;
        if (n == 0) return;

        float maxWidth = Screen.width / 3f, startX = 0f, spacing = 0f;
        if (n > 1)
        {
            spacing = Mathf.Min(MAX_SPACING, maxWidth / n);
            startX = -(spacing * (n - 1) / 2f);
        }

        List<Task> animTasks = new List<Task>();

        for (int i = 0; i < n; i++)
        {
            float targetX = startX + spacing * i;
            RectTransform rect = enemyCardBackList[i].GetComponent<RectTransform>();
            rect.DOKill();
            animTasks.Add(rect.DOAnchorPos(new Vector2(targetX, -OFFSET_Y), SORT_ANIMATION_DURATION).SetEase(Ease.OutCubic).AsyncWaitForCompletion());
        }

        await Task.WhenAll(animTasks);
    }

    public static async Task hoverHandCard(CardUi cardUi)
    {
        if (!instance.isPlayerAnimating)
        {
            instance.hoveredHandCardIndex = instance.playerCardUiList.IndexOf(cardUi);
            await instance.sortPlayerHand();
        }
    }

    public static async Task playerUseCard(CardUi cardUi)
    {
        instance.usingCard = cardUi;
        instance.usingCardIndex = instance.playerCardUiList.IndexOf(cardUi);

        instance.playerCardUiList.Remove(cardUi);
        instance.hoveredHandCardIndex = -1;
        cardUi.transform.SetAsLastSibling();

        RectTransform rect = cardUi.GetComponent<RectTransform>();
        rect.DOKill();
        rect.anchoredPosition = usingCardPos;

        await instance.sortPlayerHand();
    }

    public static async Task enemyUseCard(Card card)
    {
        if (card == null || instance.enemyCardBackList.Count <= 0)
            return;

        GameObject cardBackToConsume = instance.enemyCardBackList[0];
        instance.enemyCardBackList.RemoveAt(0);

        RectTransform rect = cardBackToConsume.GetComponent<RectTransform>();
        rect.DOKill();

        await rect.DOScale(Vector3.zero, USE_ANIMATION_DURATION).AsyncWaitForCompletion();

        Destroy(cardBackToConsume);
        await instance.sortEnemyHand();

        UiManager.processHoveredObjectData(card, false);
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

    public static async Task rearrangeUsingCard()
    {
        if (instance.usingCard != null)
        {
            instance.usingCard.gameObject.SetActive(true);
            instance.playerCardUiList.Insert(instance.usingCardIndex, instance.usingCard);
            instance.usingCard = null;
            instance.usingCardIndex = -1;
            await instance.sortPlayerHand();
        }
    }

    public static async Task rearrangeHandCardHover()
    {
        instance.hoveredHandCardIndex = -1;
        await instance.sortPlayerHand();
    }

    public static async Task flush(bool isPlayer)
    {
        if (isPlayer) await instance.flushPlayerHand();
        else await instance.flushEnemyHand();
    }

    private async Task flushPlayerHand()
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

        await seq.AsyncWaitForCompletion();

        playerCardUiList.Clear();
        foreach (Transform child in playerHandCardCanvas.transform)
        {
            child.DOKill();
            Destroy(child.gameObject);
        }
        isPlayerAnimating = false;
    }

    private async Task flushEnemyHand()
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

        await seq.AsyncWaitForCompletion();

        enemyCardBackList.Clear();
        foreach (Transform child in enemyHandCardCanvas.transform)
        {
            child.DOKill();
            Destroy(child.gameObject);
        }
        isEnemyAnimating = false;
    }

    public static async Task hidePlayerHandCards()
    {
        if (instance.playerCardUiList.Count == 0) return;

        instance.isPlayerAnimating = true;
        List<Task> animTasks = new List<Task>();

        foreach (var cardUi in instance.playerCardUiList)
        {
            RectTransform rect = cardUi.GetComponent<RectTransform>();
            rect.DOKill();
            animTasks.Add(rect.DOAnchorPosY(HIDDEN_Y, HIDE_ANIMATION_DURATION).SetEase(Ease.InBack).AsyncWaitForCompletion());
        }

        await Task.WhenAll(animTasks);
        instance.isPlayerAnimating = false;
    }

    public static async Task showPlayHandCards()
    {
        if (instance.playerCardUiList.Count == 0) return;

        instance.isPlayerAnimating = true;
        await instance.sortPlayerHand();
        instance.isPlayerAnimating = false;
    }

    public static async Task showSelectingCardList(List<Card> cards, params int[] excludedCardIds)
    {
        instance.isPlayerAnimating = true;
        BoardManager.hideBoard();
        instance.hoveredSelectingCardIndex = -1;

        foreach (var card in cards)
        {
            if (excludedCardIds != null && excludedCardIds.Contains(card.getId())) continue;

            CardUi cardUi = Instantiate(instance.cardPrefab, instance.selectingCardCanvas.transform).GetComponent<CardUi>();
            cardUi.initCard(card);
            instance.selectingCardUiList.Add(cardUi);

            RectTransform rect = cardUi.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.zero;
        }

        await instance.sortSelectingCardList();
        instance.isPlayerAnimating = false;
    }

    public static async Task hideSelectingCardList()
    {
        if (instance.selectingCardUiList.Count == 0) return;

        instance.isPlayerAnimating = true;
        List<Task> animTasks = new List<Task>();

        foreach (var cardUi in instance.selectingCardUiList)
        {
            RectTransform rect = cardUi.GetComponent<RectTransform>();
            rect.DOKill();
            animTasks.Add(rect.DOScale(Vector3.zero, DISAPEAR_ANIMATION_DURATION).SetEase(Ease.InBack).AsyncWaitForCompletion());
        }

        await Task.WhenAll(animTasks);

        foreach (var cardUi in instance.selectingCardUiList) Destroy(cardUi.gameObject);

        BoardManager.showBoard();
        instance.selectingCardUiList.Clear();
        instance.isPlayerAnimating = false;
    }

    private async Task sortSelectingCardList()
    {
        int n = selectingCardUiList.Count;
        if (n == 0) return;

        int totalRows = (n - 1) / SELECTING_COLUMNS + 1;
        float startY = (totalRows - 1) * SELECTING_SPACING_Y / 2f + SELECTING_OFFSET_Y;

        List<Task> animTasks = new List<Task>();

        for (int i = 0; i < n; i++)
        {
            CardUi cardUi = selectingCardUiList[i];

            int row = i / SELECTING_COLUMNS;
            int col = i % SELECTING_COLUMNS;
            int cardsInThisRow = Mathf.Min(SELECTING_COLUMNS, n - row * SELECTING_COLUMNS);

            float startX = -(SELECTING_SPACING_X * (cardsInThisRow - 1) / 2f);
            float targetX = startX + col * SELECTING_SPACING_X;
            float targetY = startY - row * SELECTING_SPACING_Y;

            float targetScale = (i == hoveredSelectingCardIndex) ? SELECTING_SCALE_HOVER : SELECTING_SCALE_NORMAL;

            if (i == hoveredSelectingCardIndex) cardUi.transform.SetAsLastSibling();
            else cardUi.transform.SetSiblingIndex(i);

            RectTransform rect = cardUi.GetComponent<RectTransform>();
            rect.DOKill();

            animTasks.Add(rect.DOAnchorPos(new Vector2(targetX, targetY), SORT_ANIMATION_DURATION).SetEase(Ease.OutCubic).AsyncWaitForCompletion());
            animTasks.Add(rect.DOScale(Vector3.one * targetScale, SORT_ANIMATION_DURATION).SetEase(Ease.OutCubic).AsyncWaitForCompletion());
        }

        await Task.WhenAll(animTasks);
        InputManager.tryDetectHover();
    }

    public static async Task playerHoldCard(Card card)
    {
        CardUi cardUi = instance.playerCardUiList.Find(c => c.getCard() == card);

        instance.playerCardUiList.Remove(cardUi);
        instance.hoveredHandCardIndex = -1;
        cardUi.transform.SetAsLastSibling();

        RectTransform rect = cardUi.GetComponent<RectTransform>();
        rect.DOKill();

        await rect.DOScale(Vector3.zero, HOLD_ANIMATION_DURATION).AsyncWaitForCompletion();

        Destroy(cardUi.gameObject);

        await instance.sortPlayerHand();
    }

    public static async Task enemyHoldCard()
    {
        if (instance.enemyCardBackList.Count <= 0)
            return;

        GameObject cardBackToConsume = instance.enemyCardBackList[0];
        instance.enemyCardBackList.RemoveAt(0);

        RectTransform rect = cardBackToConsume.GetComponent<RectTransform>();
        rect.DOKill();

        await rect.DOScale(Vector3.zero, HOLD_ANIMATION_DURATION).AsyncWaitForCompletion();

        Destroy(cardBackToConsume);
        await instance.sortEnemyHand();
    }

    public static async Task playerRetrieveCard(Card card)
    {
        CardUi cardUi = Instantiate(instance.cardPrefab, instance.playerHandCardCanvas.transform).GetComponent<CardUi>();
        cardUi.initCard(card);
        instance.playerCardUiList.Add(cardUi);

        RectTransform rect = cardUi.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(Screen.width / 2f, -Screen.height / 2f);
        rect.localScale = Vector3.zero;

        instance.isPlayerAnimating = true;

        await Task.WhenAll(
            rect.DOAnchorPos(new Vector2(0, OFFSET_Y), DRAW_ANIMATION_DURATION).SetEase(Ease.OutBack).AsyncWaitForCompletion(),
            rect.DOScale(Vector3.one, DRAW_ANIMATION_DURATION).AsyncWaitForCompletion()
        );

        await instance.sortPlayerHand();
        instance.isPlayerAnimating = false;
    }

    public static async Task enemyRetrieveCard()
    {
        GameObject cardBack = GameObject.Instantiate(instance.cardBackPrefab, instance.enemyHandCardCanvas.transform);
        instance.enemyCardBackList.Add(cardBack);

        RectTransform rect = cardBack.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(Screen.width / 2f, Screen.height / 2f);
        rect.localScale = Vector3.zero;

        instance.isEnemyAnimating = true;

        await Task.WhenAll(
            rect.DOAnchorPos(new Vector2(0, -OFFSET_Y), DRAW_ANIMATION_DURATION).SetEase(Ease.OutBack).AsyncWaitForCompletion(),
            rect.DOScale(Vector3.one, DRAW_ANIMATION_DURATION).AsyncWaitForCompletion()
        );

        await instance.sortEnemyHand();
        instance.isEnemyAnimating = false;
    }

    public static async Task playerRechargeCard(Card card)
    {
        CardUi targetCardUi = instance.playerCardUiList.Find(c => c.getCard() == card);
        if (targetCardUi == null) return;

        instance.isPlayerAnimating = true;
        instance.playerCardUiList.Remove(targetCardUi);

        RectTransform rect = targetCardUi.GetComponent<RectTransform>();
        rect.DOKill();

        await Task.WhenAll(
            rect.DOAnchorPos(new Vector2(-Screen.width / 2f, -Screen.height / 2f), DISAPEAR_ANIMATION_DURATION).SetEase(Ease.InBack).AsyncWaitForCompletion(),
            rect.DOScale(Vector3.zero, DISAPEAR_ANIMATION_DURATION).SetEase(Ease.InBack).AsyncWaitForCompletion()
        );

        Destroy(targetCardUi.gameObject);
        await instance.sortPlayerHand();
        instance.isPlayerAnimating = false;
    }

    public static async Task enemyRechargeCard()
    {
        if (instance.enemyCardBackList.Count <= 0) return;

        instance.isEnemyAnimating = true;

        GameObject cardBackToConsume = instance.enemyCardBackList[0];
        instance.enemyCardBackList.RemoveAt(0);

        RectTransform rect = cardBackToConsume.GetComponent<RectTransform>();
        rect.DOKill();

        await Task.WhenAll(
            rect.DOAnchorPos(new Vector2(-Screen.width / 2f, Screen.height / 2f), DISAPEAR_ANIMATION_DURATION).SetEase(Ease.InBack).AsyncWaitForCompletion(),
            rect.DOScale(Vector3.zero, DISAPEAR_ANIMATION_DURATION).SetEase(Ease.InBack).AsyncWaitForCompletion()
        );

        Destroy(cardBackToConsume);
        await instance.sortEnemyHand();
        instance.isEnemyAnimating = false;
    }

    public static async Task hoverSelectingCard(CardUi cardUi)
    {
        if (!instance.isPlayerAnimating)
        {
            instance.hoveredSelectingCardIndex = instance.selectingCardUiList.IndexOf(cardUi);
            await instance.sortSelectingCardList();
        }
    }

    public static async Task rearrangeSelectingCardHover()
    {
        instance.hoveredSelectingCardIndex = -1;
        await instance.sortSelectingCardList();
    }
}
