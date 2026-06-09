using System.Threading.Tasks;

public class CardHoldOption : AiOption
{
    private Card card;

    public CardHoldOption(Card card)
    {
        this.card = card;
    }

    public bool isRandomEffect() => false;
    public bool isHoldAction() => true;

    public Task execute(bool isLocal)
    {
        Card realCard = MatchDataManager.getPlayerData(isLocal).getHandList().Find(c => c.getUniqueId() == card.getUniqueId());

        if (realCard != null)
            MatchDataManager.holdCard(isLocal, realCard);

        return Task.CompletedTask;
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, int currentTurnCount)
    {
        Card virtualCard = clonedAlly.getHandList().Find(c => c.getUniqueId() == card.getUniqueId());

        if (virtualCard != null)
            clonedAlly.holdCard(virtualCard);
    }
}