using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AddPlayerStatusEffect : CardEffect
{
    bool isEnemy;
    PlayerStatus status;

    public AddPlayerStatusEffect(bool isEnemy, PlayerStatus status)
    {
        this.isEnemy = isEnemy;
        this.status = status;
    }

    public bool canActivate(bool isLocal, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        MatchDataManager.addStatus(isEnemy ^ isLocal, status);
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        throw new System.NotImplementedException();
    }
}
