using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SequentialCompositeEffect : CardEffect
{
    private CardEffect firstEffect;
    private Func<EffectTargetPayload, CardEffect> secondEffectFactory;

    public SequentialCompositeEffect(CardEffect firstEffect, Func<EffectTargetPayload, CardEffect> secondEffectFactory)
    {
        this.firstEffect = firstEffect;
        this.secondEffectFactory = secondEffectFactory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        // 建立一個代表當前真實盤面的 BoardState 以符合新的 getUseOptions 簽章
        BoardState currentState = new BoardState(false);
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                currentState.setChessPieceAtPosition(x, y, BoardManager.getChessPieceAtPosition(x, y));
            }
        }

        List<CardUseOption> firstOptions = firstEffect.getUseOptions(currentState, isLocal, card, MatchDataManager.getPlayerMana(isLocal), currentTurnCount);

        if (firstOptions == null)
            return false;

        foreach (CardUseOption option in firstOptions)
        {
            EffectTargetPayload firstPayload = option.getTargets();
            CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);

            if (secondEffect.canActivate(isLocal, currentTurnCount, card))
                return true;
        }

        return false;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        CompositePayload compositePayload = targets as CompositePayload;

        if (compositePayload == null || compositePayload.getPayLoads().Count < 2)
            throw new System.ArgumentException("Expected CompositePayload");

        EffectTargetPayload firstPayload = compositePayload.getPayLoad(0);
        EffectTargetPayload secondPayload = compositePayload.getPayLoad(1);
        CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);

        await firstEffect.executeEffect(isLocal, card, firstPayload);
        await secondEffect.executeEffect(isLocal, card, secondPayload);
    }

    public async Task<EffectTargetPayload> gatherTargets(List<EffectTargetPayload> previousPayloads = null)
    {
        EffectTargetPayload firstPayload = await firstEffect.gatherTargets(previousPayloads);

        if (firstPayload == null)
            return null;

        CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);

        List<EffectTargetPayload> accumulatedPayloads = new List<EffectTargetPayload>();

        if (previousPayloads != null)
            accumulatedPayloads.AddRange(previousPayloads);

        accumulatedPayloads.Add(firstPayload);

        EffectTargetPayload secondPayload = await secondEffect.gatherTargets(accumulatedPayloads);

        CompositePayload compositePayload = new CompositePayload();
        compositePayload.addPayload(firstPayload);
        compositePayload.addPayload(secondPayload);

        return compositePayload;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        return await gatherTargets(null);
    }

    // ★ 更新簽章，將 BoardState 往下傳遞給子效果，並將手動累加的分數改為 0f
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> finalOptions = new List<CardUseOption>();

        List<CardUseOption> firstOptions = firstEffect.getUseOptions(state, isLocal, card, currentMana, currentTurnCount);

        if (firstOptions == null)
            return finalOptions;

        foreach (CardUseOption option in firstOptions)
        {
            EffectTargetPayload firstPayload = option.getTargets();
            CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);
            List<CardUseOption> secondOptions = secondEffect.getUseOptions(state, isLocal, card, currentMana, currentTurnCount);

            if (secondOptions == null)
                continue;

            foreach (CardUseOption secondOption in secondOptions)
            {
                EffectTargetPayload secondPayload = secondOption.getTargets();
                CompositePayload compositePayload = new CompositePayload();
                compositePayload.addPayload(firstPayload);
                compositePayload.addPayload(secondPayload);

                // 分數給 0f，由 DFS 虛擬執行完前後兩個效果後，統一進行全盤評估
                finalOptions.Add(new CardUseOption(card, 0f, compositePayload));
            }
        }

        return finalOptions;
    }

    // ★ DFS 虛擬執行實作：依序執行第一個效果、動態生成第二個效果、再執行第二個效果
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        CompositePayload compositePayload = targets as CompositePayload;

        if (compositePayload == null || compositePayload.getPayLoads().Count < 2)
            return;

        EffectTargetPayload firstPayload = compositePayload.getPayLoad(0);
        EffectTargetPayload secondPayload = compositePayload.getPayLoad(1);

        // 1. 虛擬執行第一個效果（這可能會改變 clonedState 的盤面狀態）
        firstEffect.executeVirtually(clonedState, clonedAlly, clonedEnemy, firstPayload, currentTurnCount);

        // 2. 依據第一個效果產生的 payload，動態生成第二個效果
        CardEffect secondEffect = secondEffectFactory.Invoke(firstPayload);

        // 3. 虛擬執行第二個效果
        secondEffect.executeVirtually(clonedState, clonedAlly, clonedEnemy, secondPayload, currentTurnCount);
    }
}