using System.Threading.Tasks;

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
            MatchDataManager.holdCard(true, card);
        else
            MatchDataManager.holdCard(false, card);

        return Task.CompletedTask;
    }

    public float getScore() => score;
}
