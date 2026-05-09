using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface CardEffect
{
    public bool canActivate(bool isLocal, int currentTurnCount, Card card);

    public Task<EffectTargetPayload> gatherTargets();

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount);

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets);

    Task<EffectTargetPayload> gatherTargets(List<EffectTargetPayload> previousPayloads)
    {
        return gatherTargets();
    }
}
