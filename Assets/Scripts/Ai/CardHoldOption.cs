using System.Threading.Tasks;
using UnityEngine;

public class CardHoldOption : AiOption
{
    private Card card;
    private float score;

    public CardHoldOption(Card card, float score)
    {
        this.card = card;
        this.score = score;
    }

    public Task execute(PlayerData ownerData)
    {
        if (ownerData.IsLocal())
        {
            MatchDataManager.holdCard(true, card);
            LogManager.rigisterLogData(new CardHoldLog(ownerData.IsWhite()));
        }
        else
        {
            MatchDataManager.holdCard(false, card);
            LogManager.rigisterLogData(new CardHoldLog(ownerData.IsWhite()));
        }

        return Task.CompletedTask;
    }

    public float getScore() => score;
}
