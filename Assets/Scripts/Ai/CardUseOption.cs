using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardUseOption : AiOption
{
    private float score;
    private float quickScore;
    private Card card;
    private EffectTargetPayload targets;

    public CardUseOption(Card card, float score, EffectTargetPayload targets = null)
    {
        this.card = card;
        this.score = score;
        this.targets = targets ?? EffectTargetPayload.empty();
    }

    public float getScore() => score;
    public void setScore(float score) => this.score = score;
    public float getQuickScore() => quickScore;
    public void setQuickScore(float score) => this.quickScore = score;

    public bool isHoldAction() => false;

    public bool isRandomEffect()
    {
        // ★ 透過卡片 ID 向 Manager 查詢它是不是隨機效果
        return CardEffectManager.isRandomEffect(card.getId());
    }

    public async Task execute(bool isLocal)
    {
        if (isLocal)
            await CardEffectManager.processLocalAiCard(card, targets);
        else
            await CardEffectManager.processEnemyAiCard(card, targets);
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, int currentTurnCount)
    {
        // 1. 抓出虛擬分身
        Card virtualCard = clonedAlly.getHandList().Find(c => c.getId() == card.getId());
        if (virtualCard == null) return;

        // 2. 扣除虛擬魔力，並將卡片從手牌移至棄牌區
        clonedAlly.useCard(virtualCard, currentTurnCount);

        // 3. 執行虛擬卡片效果
        // ★ 這裡需要你在 CardEffectManager 補上一個 executeVirtually 的接口，讓它只修改 clonedState 與 clonedAlly/Enemy
        // CardEffectManager.getCardEffect(virtualCard.getId()).executeVirtually(clonedState, clonedAlly, clonedEnemy, targets, currentTurnCount);
    }

    public string getCardname() => card.getName();
    public EffectTargetPayload getTargets() => targets;
}