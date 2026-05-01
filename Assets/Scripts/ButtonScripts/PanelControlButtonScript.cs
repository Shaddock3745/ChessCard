using UnityEngine;

public class PanelControlButtonScript : MonoBehaviour
{
    public void closeHoverObjectdataPanel()
    {
        UiManager.hideHoveredObjectDataPanel();
    }

    public void showLogPanel()
    {
        UiManager.showLogPanel();
    }

    public void closeLogPanel()
    {
        UiManager.hideLogPanel();
    }
}
