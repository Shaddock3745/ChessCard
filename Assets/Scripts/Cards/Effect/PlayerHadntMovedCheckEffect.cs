using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerHadntMovedCheckEffect : CardEffect
{
    private bool isEnemy;

    public PlayerHadntMovedCheckEffect(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return !MatchDataManager.hadMovedThisTurn(isEnemy != isLocal);
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}
