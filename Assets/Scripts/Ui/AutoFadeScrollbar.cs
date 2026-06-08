using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class AutoFadeScrollbar : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    private CanvasGroup canvasGroup;
    private Tween fadeTween;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScroll);
        }
    }

    private void OnScroll(Vector2 value)
    {
        fadeTween?.Kill();

        canvasGroup.alpha = 1f;

        fadeTween = canvasGroup.DOFade(0f, 0.5f).SetDelay(1f);
    }

    void OnDestroy()
    {
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.RemoveListener(OnScroll);
        }
        fadeTween?.Kill();
    }
}