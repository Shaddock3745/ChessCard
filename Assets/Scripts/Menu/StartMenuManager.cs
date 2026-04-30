using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuManager : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startGame()
    {
        // Load the main game scene
        SceneManager.LoadScene(gameSceneName);
    } 

    public void quitGame()
    {
        Application.Quit();
    }
}
