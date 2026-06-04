using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MaterializeCardEffect : CardEffect
{
    private bool isEnemy;
    private int cardId;
    private int amount;
    private bool noConsumption;

    public MaterializeCardEffect(bool isEnemy, int cardId, int amount, bool noConsumption)
    {
        this.isEnemy = isEnemy;
        this.cardId = cardId;
        this.amount = amount;
        this.noConsumption = noConsumption;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        for (int i = 0; i < amount; i++)
            MatchDataManager.materializeCard(isLocal != isEnemy, cardId, noConsumption);

        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，拔除手動算分，給予 0f
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (canActivate(isLocal, currentTurnCount, card))
        {
            options.Add(new CardUseOption(card, 0f, EffectTargetPayload.empty()));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：在虛擬玩家資料中無中生有印出特定卡片
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;

        for (int i = 0; i < amount; i++)
        {
            // 呼叫你在 PlayerData 中寫好的 materializeCard 方法
            // 這樣 DFS 在下一步推演時，就能在虛擬手牌裡看到這張新卡並嘗試使用它！
            targetPlayer.materializeCard(cardId, noConsumption);
        }
    }
}