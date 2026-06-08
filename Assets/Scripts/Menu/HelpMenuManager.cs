using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpMenuManager : MonoBehaviour
{
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField] PanelGroupController panelGroupController;
    [SerializeField] GameObject nextButton, previousButton;

    public void onNextButtonClick()
    {
        previousButton.SetActive(true);
        panelGroupController.next();

        if (panelGroupController.isEnd())
            nextButton.SetActive(false);
    }

    public void onPreviousButtonClick()
    {
        nextButton.SetActive(true);
        panelGroupController.previous();

        if (panelGroupController.isStart())
            previousButton.SetActive(false);
    }

    public void onQuitButtonClick()
    {
        SceneManager.LoadScene(startSceneName);
    }
}
