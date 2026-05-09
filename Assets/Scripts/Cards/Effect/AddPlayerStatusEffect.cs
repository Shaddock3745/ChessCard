using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AddPlayerStatusEffect : CardEffect
{
    private bool isEnemy;
    private int duration;
    private Func<int, int, PlayerStatus> statusFactory;

    public AddPlayerStatusEffect(bool isEnemy, int duration, Func<int, int, PlayerStatus> statusFactory)
    {
        this.isEnemy = isEnemy;
        this.duration = duration;
        this.statusFactory = statusFactory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int currentTurnCount = GameManager.getCurrentTurnCount();
        MatchDataManager.addStatus(isEnemy ^ isLocal, statusFactory.Invoke(currentTurnCount, currentTurnCount + duration));
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption> { new CardUseOption(card, 1, EffectTargetPayload.empty()) };
    }
}
