using System.Threading.Tasks;

public class CardHoldOption : AiOption
{
    private Card card;
    private float score;
    private float quickScore;

    public CardHoldOption(Card card, float score)
    {
        this.card = card;
        this.score = score;
    }

    public float getScore() => score;
    public void setScore(float score) => this.score = score;
    public float getQuickScore() => quickScore;
    public void setQuickScore(float score) => this.quickScore = score;

    public bool isRandomEffect() => false;
    public bool isHoldAction() => true;

    public Task execute(bool isLocal)
    {
        Card realCard = MatchDataManager.getPlayerData(isLocal).getHandList().Find(c => c.getId() == card.getId());
        if (realCard != null)
        {
            MatchDataManager.holdCard(isLocal, realCard);
        }
        return Task.CompletedTask;
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, int currentTurnCount)
    {
        Card virtualCard = clonedAlly.getHandList().Find(c => c.getId() == card.getId());
        if (virtualCard != null)
        {
            clonedAlly.holdCard(virtualCard);
        }
    }
}