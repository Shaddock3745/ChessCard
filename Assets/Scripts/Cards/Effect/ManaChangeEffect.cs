using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ManaChangeEffect : CardEffect
{
    private bool isEnemy;
    private int delta;

    public ManaChangeEffect(bool isEnemy, int delta)
    {
        this.isEnemy = isEnemy;
        this.delta = delta;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        MatchDataManager.changePlayerMana(isEnemy != isLocal, delta);
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
