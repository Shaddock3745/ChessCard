using UnityEngine;

public class ChangeTurnButtonScriptTemp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClick()
    {
        //GameManager.ChangeTurn();

        MoveOption bestMove = AIManager.getBestMove(false);
        if (bestMove != null)
        {
            Debug.Log("Best move: " + bestMove.piece.name + " to " + bestMove.targetPos);
        }
    }
}
