using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AddChessStatusEffect : CardEffect
{
    public bool canActivate(bool isLocal, Card card)
    {
        throw new System.NotImplementedException();
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        throw new System.NotImplementedException();
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        throw new System.NotImplementedException();
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        throw new System.NotImplementedException();
    }
}
