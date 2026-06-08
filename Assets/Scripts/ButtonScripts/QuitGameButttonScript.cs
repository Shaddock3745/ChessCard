using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitGameButttonScript : MonoBehaviour
{
    [SerializeField] private string startSceneName = "StartScene";

    public void onClick()
    {
        SceneManager.LoadScene(startSceneName);
    }
}
