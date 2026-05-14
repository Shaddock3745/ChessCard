using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SwapChessPositionEffect : CardEffect
{
    private Func<EffectTargetPayload, (Vector2Int, Vector2Int)> factory;

    public SwapChessPositionEffect(Func<EffectTargetPayload, (Vector2Int, Vector2Int)> factory)
    {
        this.factory = factory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        var (firstPostion, secondPostion) = factory.Invoke(targets);

        await BoardManager.swapTwoPositionWithAnimation(firstPostion,secondPostion);
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
