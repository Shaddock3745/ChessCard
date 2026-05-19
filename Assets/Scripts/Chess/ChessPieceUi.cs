using DG.Tweening;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChessPieceUi : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteRenderer cantMoveIcon, cantAttackIcon, cantBeAttackedIcon;

    private static readonly float MOVING_ANIMATION_DURATION = 0.5f;
    private static readonly float CHESS_FADE_DURATION = 0.5f;
    private static readonly float STATUS_FADE_DURATION = 0.5f, STATUS_HOLD_DURATION = 0.8f;
    private static readonly float STATUS_MAX_ALPHA = 0.9f;

    private ChessPiece data;
    private Coroutine currentFxCoroutine;

    public void initChessUi(ChessPiece data, Sprite sprite)
    {
        this.data = data;
        spriteRenderer.sprite = sprite;
        gameObject.name = (data.isWhite() ? "White_" : "Black_") + data.getChessType().ToString();
    }

    public void UpdateStatusFx(int currentTurnCount)
    {
        StopAllCoroutines();
        currentFxCoroutine = null;

        setIconAlpha(cantMoveIcon, 0f);
        setIconAlpha(cantAttackIcon, 0f);
        setIconAlpha(cantBeAttackedIcon, 0f);

        List<SpriteRenderer> activeIcons = new List<SpriteRenderer>();

        if (!data.canAttack(currentTurnCount))
            activeIcons.Add(cantAttackIcon);

        if (!data.canMove(currentTurnCount))
            activeIcons.Add(cantMoveIcon);

        if (!data.canBeAttacked(currentTurnCount))
            activeIcons.Add(cantBeAttackedIcon);

        if (activeIcons.Count > 0)
            currentFxCoroutine = StartCoroutine(playIconSequence(activeIcons));
    }

    private IEnumerator playIconSequence(List<SpriteRenderer> icons)
    {
        int currentIndex = 0;

        while (true)
        {
            SpriteRenderer currentIcon = icons[currentIndex];
            yield return StartCoroutine(fadeIconAlpha(currentIcon, 0f, STATUS_MAX_ALPHA, STATUS_FADE_DURATION));
            yield return new WaitForSeconds(STATUS_HOLD_DURATION);
            yield return StartCoroutine(fadeIconAlpha(currentIcon, STATUS_MAX_ALPHA, 0f, STATUS_FADE_DURATION));
            currentIndex = (currentIndex + 1) % icons.Count;
        }
    }

    private IEnumerator fadeIconAlpha(SpriteRenderer icon, float start, float end, float duration)
    {
        Color c = icon.color;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, elapsed / duration);
            icon.color = c;
            yield return null;
        }

        c.a = end;
        icon.color = c;
    }

    private void setIconAlpha(SpriteRenderer icon, float alpha)
    {
        Color c = icon.color;
        c.a = alpha;
        icon.color = c;
    }

    public ChessPiece getChess() => data;

    public void moveToPoint(Vector3 point)
    {
        transform.position = point;
    }

    public async Task moveToPointWithAnimation(Vector3 point)
    {
        await transform.DOMove(point, MOVING_ANIMATION_DURATION)
            .SetEase(Ease.InOutQuad).AsyncWaitForCompletion();
    }

    public void destroy()
    {
        Destroy(gameObject);
    }

    public async Task destroyWithAnimation()
    {
        spriteRenderer.DOKill();
        await spriteRenderer.DOFade(0f, CHESS_FADE_DURATION).AsyncWaitForCompletion();
        destroy();
    }

    public async Task fadeIn()
    {
        spriteRenderer.DOKill();
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;
        await spriteRenderer.DOFade(1f, CHESS_FADE_DURATION).AsyncWaitForCompletion();
    }

    public void BringToFrontOf(ChessPieceUi target)
    {
        if (this.spriteRenderer.sortingOrder <= target.spriteRenderer.sortingOrder)
            this.spriteRenderer.sortingOrder = target.spriteRenderer.sortingOrder + 1;
    }
}
