using System.Collections.Generic;
using System.Threading.Tasks;

public class RedrawEffect : CardEffect
{
    private bool isEnemy;

    public RedrawEffect(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return MatchDataManager.getPlayerHandCardCount(isEnemy != isLocal) > 0;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        bool isPlayerLocal = isEnemy != isLocal;
        List<Card> currentHand = new List<Card>(MatchDataManager.getPlayerData(isPlayerLocal).getHandList());
        int count = currentHand.Count;

        if (count == 0)
            return;

        foreach (Card c in currentHand)
            MatchDataManager.rechargeCard(isPlayerLocal, c);

        MatchDataManager.replenish(isPlayerLocal);
        await Task.Delay(500);

        for (int i = 0; i < count; i++)
            MatchDataManager.drawCard(isPlayerLocal);
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}