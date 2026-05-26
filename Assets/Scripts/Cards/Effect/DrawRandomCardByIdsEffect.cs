using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DrawRandomCardByIdsEffect : CardEffect
{
    private bool isEnemy;
    private int[] validCardIds;

    public DrawRandomCardByIdsEffect(bool isEnemy, params int[] validCardIds)
    {
        this.isEnemy = isEnemy;
        this.validCardIds = validCardIds;
    }
    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return MatchDataManager.hasValidCardInDeckOrWaiting(isLocal, validCardIds);
    }
    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        MatchDataManager.drawRandomCardFromDeck(isEnemy != isLocal, validCardIds);
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
