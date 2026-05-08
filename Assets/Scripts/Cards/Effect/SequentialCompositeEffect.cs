using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SequentialCompositeEffect : CardEffect
{
    private CardEffect firstEffect;
    private Func<EffectTargetPayload, CardEffect> secondEffectFactory;

    public SequentialCompositeEffect(CardEffect firstEffect, Func<EffectTargetPayload, CardEffect> secondEffectFactory)
    {
        this.firstEffect = firstEffect;
        this.secondEffectFactory = secondEffectFactory;
    }

    public bool canActivate(bool isLocal, Card card)
    {
        List<CardUseOption> firstOptions = firstEffect.getUseOptions(isLocal, card, MatchDataManager.getPlayerMana(isLocal));

        if (firstOptions == null)
            return false;

        foreach (CardUseOption option in firstOptions)
        {
            EffectTargetPayload firstPayload = option.getTargets();
            CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);

            if(secondEffect.canActivate(isLocal, card))
                return true;
        }

        return false;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        CompositePayload compositePayload = targets as CompositePayload;

        if(compositePayload == null || compositePayload.getPayLoads().Count < 2)
            throw new System.ArgumentException("Expected CompositePayload");

        EffectTargetPayload firstPayload = compositePayload.getPayLoad(0);
        EffectTargetPayload secondPayload = compositePayload.getPayLoad(1);
        CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);

        await firstEffect.executeEffect(isLocal, card, firstPayload);
        await secondEffect.executeEffect(isLocal, card, secondPayload);
    }

    public async Task<EffectTargetPayload> gatherTargets(List<EffectTargetPayload> previousPayloads = null)
    {
        EffectTargetPayload firstPayload = await firstEffect.gatherTargets(previousPayloads);

        if (firstPayload == null)
            return null;

        CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);

        List<EffectTargetPayload> accumulatedPayloads = new List<EffectTargetPayload>();

        if (previousPayloads != null)
            accumulatedPayloads.AddRange(previousPayloads);

        accumulatedPayloads.Add(firstPayload);

        EffectTargetPayload secondPayload = await secondEffect.gatherTargets(accumulatedPayloads);

        CompositePayload compositePayload = new CompositePayload();
        compositePayload.addPayload(firstPayload);
        compositePayload.addPayload(secondPayload);

        return compositePayload;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        return await gatherTargets(null);
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        List<CardUseOption> finalOptions = new List<CardUseOption>();

        List<CardUseOption> firstOptions = firstEffect.getUseOptions(isLocal, card, currentMana);

        if(firstOptions == null)
            return finalOptions;

        foreach (CardUseOption option in firstOptions)
        {
            EffectTargetPayload firstPayload = option.getTargets();
            float firstScore = option.getScore();
            CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);
            List<CardUseOption> secondOptions = secondEffect.getUseOptions(isLocal, card, currentMana);

            if (secondOptions == null)
                continue;

            foreach (CardUseOption secondOption in secondOptions)
            {
                EffectTargetPayload secondPayload = secondOption.getTargets();
                CompositePayload compositePayload = new CompositePayload();
                compositePayload.addPayload(firstPayload);
                compositePayload.addPayload(secondPayload);

                finalOptions.Add(new CardUseOption(card, firstScore + secondOption.getScore(), compositePayload));
            }
        }

        return finalOptions;
    }
}
