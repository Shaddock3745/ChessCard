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
            await BoardManager.movePieceWithAnimationByEffect(startPos, endPos);
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，管線後端不自行產生選項
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ DFS 虛擬執行實作：直接在虛擬盤面上呼叫移動方法
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions() == null || targets.getTargetPositions().Count < 2)
            return;

        Vector2Int startPos = targets.getTargetPositions()[0];
        Vector2Int endPos = targets.getTargetPositions()[1];

        if (clonedState.getChessPieceAtPosition(startPos) != null)
        {
            clonedState.moveChessPiece(startPos, endPos);
        }
    }
}