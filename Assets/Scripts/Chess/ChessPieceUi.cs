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
    private Sequence iconSequence; // 用來存儲輪播動畫序列

    public void initChessUi(ChessPiece data, Sprite sprite)
    {
        this.data = data;
        spriteRenderer.sprite = sprite;
        gameObject.name = (data.isWhite() ? "White_" : "Black_") + data.getChessType().ToString();
    }

    public void UpdateStatusFx(int currentTurnCount)
    {
        iconSequence?.Kill();

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
        {
            // 使用 DOTween 建立無限輪播序列
            iconSequence = DOTween.Sequence();
            foreach (var icon in activeIcons)
            {
                iconSequence.Append(icon.DOFade(STATUS_MAX_ALPHA, STATUS_FADE_DURATION));
                iconSequence.AppendInterval(STATUS_HOLD_DURATION);
                iconSequence.Append(icon.DOFade(0f, STATUS_FADE_DURATION));
            }
            iconSequence.SetLoops(-1); // -1 代表無限迴圈
        }
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
