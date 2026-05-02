using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LogTextInteract : MonoBehaviour, IPointerMoveHandler, IPointerExitHandler
{
    private TextMeshProUGUI text;
    private string currentHoveredLinkId = "";
    private Camera currentCamera;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();

        if (text.canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            currentCamera = null;
        else
            currentCamera = text.canvas.worldCamera;
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, currentCamera);

        if (linkIndex != -1)
        {
            string linkId = text.textInfo.linkInfo[linkIndex].GetLinkID();

            if (linkId != currentHoveredLinkId)
            {
                currentHoveredLinkId = linkId;

                if (LogManager.getLinkMap().TryGetValue(linkId, out object data))
                    if (data is Card card)
                        UiManager.processHoveredObjectData(card, false);
                    else if (data is ChessPiece chess)
                        UiManager.processHoveredObjectData(chess, false);
            }
        }
        else
            clearHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        clearHover();
    }

    private void clearHover()
    {
        if (currentHoveredLinkId != "")
            currentHoveredLinkId = "";
    }
}
