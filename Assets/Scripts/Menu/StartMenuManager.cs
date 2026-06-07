using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    public void startGame()
    {
        SceneManager.LoadScene(gameSceneName);
    } 

    public void deckSetting()
    {
         
    }

    public void quitGame()
    {
        Application.Quit();
    }
}
