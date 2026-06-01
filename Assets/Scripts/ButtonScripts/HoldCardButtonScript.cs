using System.Threading.Tasks;
using UnityEngine;

public class HoldCardButtonScript : MonoBehaviour
{
    public async void onClick()
    {
        if (GameManager.IsLocalTurn() && MatchDataManager.canHoldCard(true))
        {
            Card card = await InputManager.waitForCardSelection(MatchDataManager.getPlayerHandCardList(true));

            MatchDataManager.holdCard(true, card);
        }
    }
}
