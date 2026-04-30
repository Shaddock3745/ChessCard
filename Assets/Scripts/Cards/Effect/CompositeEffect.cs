using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CompositeEffect : CardEffect
{
    private List<CardEffect> effects;

    public CompositeEffect(params CardEffect[] effects)
    {
        this.effects = new List<CardEffect>(effects);
    }

    public bool canActivate(bool isLocal, Card card)
    {
        return effects.TrueForAll(effect => effect.canActivate(isLocal, card));
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        foreach (var effect in effects)
            await effect.executeEffect(isLocal, card, targets);
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
