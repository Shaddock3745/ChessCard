using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuManager : MonoBehaviour
{
    [SerializeField] private string startSceneName = "StartScene";
    [SerializeField] private string gameSceneName = "GameScene";

    public void backToStartMenu()
    {
        SceneManager.LoadScene(startSceneName);
    }

    public void startNewGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
