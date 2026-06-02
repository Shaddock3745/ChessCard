using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RemoveChessAtTargetPositionEffect : CardEffect
{
    public RemoveChessAtTargetPositionEffect()
    {
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
        {
            Debug.LogError("移除棋子效果的目標位置錯誤");
            return;
        }

        if (isLocal)
            await BoardManager.removePieceWithAnimation(targets.getTargetPositions()[0]);
        else
        {
            await BoardManager.showTargetWithAnimation(targets.getTargetPositions());
            await BoardManager.removePieceWithAnimation(targets.getTargetPositions()[0]);
            BoardManager.removeAllHint();
        }

        InputManager.tryDetectHover();
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
