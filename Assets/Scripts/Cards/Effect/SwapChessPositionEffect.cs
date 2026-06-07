using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SwapChessPositionEffect : CardEffect
{
    private Func<BoardState, bool, EffectTargetPayload, (Vector2Int, Vector2Int)> factory;

    public SwapChessPositionEffect(Func<BoardState, bool, EffectTargetPayload, (Vector2Int, Vector2Int)> factory)
    {
        this.factory = factory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card) => true;

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        // ★ 真實路徑:用真實盤面快照 + 真實施法方顏色決定座標
        BoardState realView = BoardManager.getCloneBoardState();
        bool casterIsWhite = isLocal == GameManager.IsLocalWhite();
        var (first, second) = factory.Invoke(realView, casterIsWhite, targets);
        await BoardManager.swapTwoPositionWithAnimation(first, second);
    }

    public Task<EffectTargetPayload> gatherTargets() => Task.FromResult(EffectTargetPayload.empty());

    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
        => new List<CardUseOption>();

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        // ★ 虛擬路徑:一律用 clonedState / clonedAlly,絕不碰 BoardManager / GameManager
        var (first, second) = factory.Invoke(clonedState, clonedAlly.IsWhite(), targets);
        clonedState.swapTwoPosition(first, second);
    }
}