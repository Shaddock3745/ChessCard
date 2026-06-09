using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CompositeEffect : CardEffect
{
    private List<CardEffect> effects;

    public CompositeEffect(params CardEffect[] effects)
    {
        this.effects = new List<CardEffect>(effects);
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return effects.TrueForAll(effect => effect.canActivate(isLocal, currentTurnCount, card));
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        CompositePayload payload = targets as CompositePayload;

        if (payload == null || payload.getPayLoads().Count != effects.Count)
        {
            Debug.LogError("複合效果的 Payload 型別錯誤或數量不符");
            return;
        }

        for (int i = 0; i < effects.Count; i++)
            await effects[i].executeEffect(isLocal, card, payload.getPayLoad(i));
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        CompositePayload payload = new CompositePayload();

        foreach (var effect in effects)
        {
            var effectPayload = await effect.gatherTargets();

            if (effectPayload == null)
                return null;

            payload.addPayload(effectPayload);
        }

        return payload;
    }

    // ★ 更新簽章：將 BoardState state 一路往下傳遞給所有的子效果
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        if (!canActivate(isLocal, currentTurnCount, card))
            return new List<CardUseOption>();

        List<CardUseOption> options = new List<CardUseOption>();

        if (effects == null || effects.Count == 0)
            return options;

        buildOptionsRecursive(state, 0, new CompositePayload(), options, isLocal, card, currentMana, currentTurnCount);

        return options;
    }

    // ★ 遞迴函式也加上 BoardState state 參數
    private void buildOptionsRecursive(BoardState state, int effectIndex, CompositePayload currentPayload, List<CardUseOption> result, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        if (effectIndex >= effects.Count)
        {
            // 計分一律交給 DFS 推演,這裡帶 0f
            result.Add(new CardUseOption(card, currentPayload));
            return;
        }

        List<CardUseOption> effectOptions = effects[effectIndex].getUseOptions(state, isLocal, card, currentMana, currentTurnCount);

        if (effectOptions.Count == 0)
            return;

        foreach (var option in effectOptions)
        {
            CompositePayload newPayload = new CompositePayload(currentPayload);
            newPayload.addPayload(option.getTargets());

            buildOptionsRecursive(state, effectIndex + 1, newPayload, result, isLocal, card, currentMana, currentTurnCount);
        }
    }

    // ★ DFS 虛擬執行實作：依序呼叫所有子效果的虛擬執行，並分發對應的 Payload
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        CompositePayload payload = targets as CompositePayload;

        if (payload == null || payload.getPayLoads().Count != effects.Count)
        {
            return; // 虛擬推演中若發生例外，直接中斷該分支即可，不拋出 Error
        }

        for (int i = 0; i < effects.Count; i++)
        {
            // 將虛擬盤面、資源與拆解後的 payload 丟給對應的子效果依序執行
            effects[i].executeVirtually(clonedState, clonedAlly, clonedEnemy, payload.getPayLoad(i), currentTurnCount);
        }
    }
    public bool isRandomEffect()
    {
        // 只要子效果中，有任何一個是隨機效果，這個複合效果就是隨機的！
        return effects.Exists(effect => effect.isRandomEffect());
    }
}