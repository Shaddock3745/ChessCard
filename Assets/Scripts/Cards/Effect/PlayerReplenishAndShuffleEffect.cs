using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerReplenishAndShuffleEffect : CardEffect
{
    private bool isEnemy;

    public PlayerReplenishAndShuffleEffect(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        bool replenishLocal = isLocal != isEnemy;
        MatchDataManager.replenish(replenishLocal);
        MatchDataManager.shuffleDeck(replenishLocal);
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
