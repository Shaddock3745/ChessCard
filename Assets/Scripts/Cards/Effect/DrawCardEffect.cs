using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DrawCardEffect : CardEffect
{
    int amout;

    public DrawCardEffect(int amount)
    {
        amout = amount;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int count = MatchDataManager.getPlayerDeckCardCount(isLocal) + MatchDataManager.getPlayerWaitingCardCount(isLocal);

        for (int i = 0; i < amout && count-- > 0; i++)
            MatchDataManager.drawCard(isLocal);

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
