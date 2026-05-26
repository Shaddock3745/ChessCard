using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PipelineMovementEffect : CardEffect
{
    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count < 2) 
            return;

        Vector2Int startPos = targets.getTargetPositions()[0];
        Vector2Int endPos = targets.getTargetPositions()[1];

        if (BoardManager.getChessPieceAtPosition(startPos) != null)
            await BoardManager.movePieceWithAnimation(startPos, endPos);
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
