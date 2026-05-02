using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MaterializeCardEffect : CardEffect
{
    private int cardId;
    private int amount;
    private bool noConsumption;

    public MaterializeCardEffect(int cardId, int amount, bool noConsumption)
    {
        this.cardId = cardId;
        this.amount = amount;
        this.noConsumption = noConsumption;
    }


    public bool canActivate(bool isLocal, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        for (int i = 0; i < amount; i++)
        {
            if (isLocal)
                MatchDataManager.materializeCard(true, cardId, noConsumption);
            else
                MatchDataManager.materializeCard(false, cardId, noConsumption);
        }

        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana >= card.getCost())
            options.Add(new CardUseOption(card, 1));

        return options;
    }
}
