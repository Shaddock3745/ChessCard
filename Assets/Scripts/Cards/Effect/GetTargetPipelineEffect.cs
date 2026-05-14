using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GetTargetPipelineEffect : CardEffect
{
    private CardEffect targetGatherer;
    private CardEffect[] executeEffects;

    public GetTargetPipelineEffect(CardEffect targetGatherer, params CardEffect[] executeEffects)
    {
        this.targetGatherer = targetGatherer;
        this.executeEffects = executeEffects;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return targetGatherer.canActivate(isLocal, currentTurnCount, card);
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        foreach (CardEffect effect in executeEffects)
            await effect.executeEffect(isLocal, card, targets);
    }

    public async Task<EffectTargetPayload> gatherTargets(List<EffectTargetPayload> previousPayloads = null)
    {
        return await targetGatherer.gatherTargets(previousPayloads);
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        return await gatherTargets(null);
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}
