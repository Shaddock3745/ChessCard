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

        await BoardManager.swapTwoPositionWithAnimation(firstPostion, secondPostion);
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章。作為 Pipeline 後端效果，不自行產生選項
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ DFS 虛擬執行實作：利用 BoardState 的 API 直接對虛擬盤面進行位置互換
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null) return;

        // 透過 factory 拆解出管線傳遞過來的兩個互換座標
        var (firstPosition, secondPosition) = factory.Invoke(targets);

        // 呼叫虛擬盤面的互換方法，後續交由 DFS 結算互換後的全局戰略價值
        clonedState.swapTwoPosition(firstPosition, secondPosition);
    }
}