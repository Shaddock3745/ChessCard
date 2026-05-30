using UnityEngine;

public class ChangeTurnButtonScriptTemp : MonoBehaviour
{
    public void onClick()
    {
        if (GameManager.IsLocalTurn())
            GameManager.ChangeTurn();
    }
}
