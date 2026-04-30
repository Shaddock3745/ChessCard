using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface CardEffect
{
    public bool canActivate(bool isLocal, Card card);

    public Task<EffectTargetPayload> gatherTargets();

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana);

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets);
}
