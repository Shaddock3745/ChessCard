using UnityEngine;

public class ButtonVisibilityController : MonoBehaviour
{
    public CanvasGroup buttonCanvasGroup;

    private void HideButtonKeepSpace()
    {
        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 0f;
            buttonCanvasGroup.interactable = false;
            buttonCanvasGroup.blocksRaycasts = false;
        }
    }

    private void ShowButton()
    {
        if (buttonCanvasGroup != null)
        {
            buttonCanvasGroup.alpha = 0.3f;
            buttonCanvasGroup.interactable = true;
            buttonCanvasGroup.blocksRaycasts = true;
        }
    }

    public void setVisibility(bool isVisible)
    {
        if (isVisible)
            ShowButton();
        else
            HideButtonKeepSpace();
    }
}
