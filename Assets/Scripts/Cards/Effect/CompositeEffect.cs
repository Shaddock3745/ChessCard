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
        CompositePayload payload = targets as CompositePayload;

        if (payload == null || payload.getPayLoads().Count != effects.Count)
        {
            Debug.LogError("複合效果的 Payload 型別錯誤或數量不符");
            return;
        }

        for (int i = 0; i < effects.Count; i++)
            await effects[i].executeEffect(isLocal, card, payload.getPayLoad(i));
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        CompositePayload payload = new CompositePayload();

        foreach (var effect in effects)
        {
            var effectPayload = await effect.gatherTargets();

            if (effectPayload == null)
                return null;

            payload.addPayload(effectPayload);
        }

        return payload;
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        if (currentMana < card.getCost() || !canActivate(isLocal, card))
            return new List<CardUseOption>();

        List<CardUseOption> options = new List<CardUseOption>();

        if (effects == null || effects.Count == 0)
            return options;

        buildOptionsRecursive(0, new CompositePayload(), 0, options, isLocal, card, currentMana);

        return options;
    }

    private void buildOptionsRecursive(int effectIndex, CompositePayload currentPayload, float currentScore, List<CardUseOption> result, bool isLocal, Card card, int currentMana)
    {
        if (effectIndex >= effects.Count)
        {
            result.Add(new CardUseOption(card, currentScore, currentPayload));
            return;
        }

        List<CardUseOption> effectOptions = effects[effectIndex].getUseOptions(isLocal, card, currentMana);

        if (effectOptions.Count == 0)
            return;

        foreach (var option in effectOptions)
        {
            CompositePayload newPayload = new CompositePayload(currentPayload);
            newPayload.addPayload(option.getTargets());
            buildOptionsRecursive(effectIndex + 1, newPayload, currentScore + option.getScore(), result, isLocal, card, currentMana);
        }
    }
}
