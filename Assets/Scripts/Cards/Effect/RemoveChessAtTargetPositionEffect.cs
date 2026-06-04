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
            foreach (Vector2Int pos in targets.getTargetPositions())
                await BoardManager.removePieceWithAnimation(pos);
        else
        {
            await BoardManager.showTargetWithAnimation(targets.getTargetPositions());

            List<Task> task = new List<Task>();

            foreach (Vector2Int pos in targets.getTargetPositions())
                task.Add(BoardManager.removePieceWithAnimation(pos));

            await Task.WhenAll(task);

            BoardManager.removeAllHint();
        }

        InputManager.tryDetectHover();
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章。作為 Pipeline 後端，不自行產生選項
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ DFS 虛擬執行實作：直接在虛擬盤面上將目標位置的棋子移除
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions() == null || targets.getTargetPositions().Count == 0)
            return;

        foreach (Vector2Int pos in targets.getTargetPositions())
        {
            // 呼叫虛擬盤面的移除方法
            clonedState.removeChessPieceAtPosition(pos);
        }
    }
}