using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;

public class ChessPieceUi : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    private static readonly float MOVING_ANIMATION_DURATION = 0.5f;
    private static readonly float FADE_DURATION = 0.3f;

    private ChessPiece data;

    public void initChessUi(ChessPiece data, Sprite sprite)
    {
        this.data = data;
        spriteRenderer.sprite = sprite;
        gameObject.name = (data.isWhite() ? "White_" : "Black_") + data.getChessType().ToString();
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
        await spriteRenderer.DOFade(0f, FADE_DURATION).AsyncWaitForCompletion();
        destroy();
    }

    public async Task fadeIn()
    {
        spriteRenderer.DOKill();
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;
        await spriteRenderer.DOFade(1f, FADE_DURATION).AsyncWaitForCompletion();
    }

    public void BringToFrontOf(ChessPieceUi target)
    {
        if (this.spriteRenderer.sortingOrder <= target.spriteRenderer.sortingOrder)
        {
            this.spriteRenderer.sortingOrder = target.spriteRenderer.sortingOrder + 1;
        }
    }
}
