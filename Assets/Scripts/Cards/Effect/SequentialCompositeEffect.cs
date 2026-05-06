using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SequentialCompositeEffect : CardEffect
{
    public bool canActivate(bool isLocal, Card card)
    {
        throw new System.NotImplementedException();
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        throw new System.NotImplementedException();
    }

    public Task<EffectTargetPayload> gatherTargets(List<EffectTargetPayload> previousPayloads = null)
    {
        throw new System.NotImplementedException();
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return gatherTargets(null);
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        throw new System.NotImplementedException();
    }
}
