using UnityEngine;

public class CalculateScoreButtonScript : MonoBehaviour
{

    public void onClick()
    {
        Debug.Log("Current Score: " + AiEvaluator.EvaluateBoardState(BoardManager.getRealBoardState(), MatchDataManager.getPlayerData(true), MatchDataManager.getPlayerData(false), GameManager.getCurrentTurnCount()));
    }
}
