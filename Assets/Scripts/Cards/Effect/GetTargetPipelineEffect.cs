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
        if (!targetGatherer.canActivate(isLocal, currentTurnCount, card))
            return false;

        foreach (CardEffect effect in executeEffects)
            if (!effect.canActivate(isLocal, currentTurnCount, card))
                return false;

        return true;
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

    // ★ 更新簽章，傳遞 BoardState 給 Gatherer，並完全拔除手動算分邏輯
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        // 直接由第一個效果 (Gatherer) 產生帶有 Payload 且分數為 0f 的所有選項
        // 原本依賴 PayloadScoreEvaluator 的計分迴圈已徹底拔除，交給 DFS 處理
        return targetGatherer.getUseOptions(state, isLocal, card, currentMana, currentTurnCount);
    }

    // ★ DFS 虛擬執行實作：依序叫後方的執行效果們對指定的 payload 做虛擬操作
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        foreach (CardEffect effect in executeEffects)
        {
            effect.executeVirtually(clonedState, clonedAlly, clonedEnemy, targets, currentTurnCount);
        }
    }
}