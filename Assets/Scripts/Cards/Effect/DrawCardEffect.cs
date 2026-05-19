using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DrawCardEffect : CardEffect
{
    private bool isEnemy;
    private int amount;

    public DrawCardEffect(bool isEnemy, int amount)
    {
        this.isEnemy = isEnemy;
        this.amount = amount;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int count = MatchDataManager.getPlayerDeckCardCount(!isEnemy) + MatchDataManager.getPlayerWaitingCardCount(!isEnemy);

        for (int i = 0; i < amount && count-- > 0; i++)
            MatchDataManager.drawCard(!isEnemy);

        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana >= card.getCost())
            options.Add(new CardUseOption(card, 1));

        return options;
    }
}
