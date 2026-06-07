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
        return CardEffectManager.isRandomEffect(card.getId());
    }

    public async Task execute(bool isLocal)
    {
        // 確實從真實手牌中扣除資源，防止 0 費卡無限循環
        Card realCard = MatchDataManager.getPlayerData(isLocal).getHandList().Find(c => c.getUniqueId() == card.getUniqueId());
        if (realCard != null)
        {
            MatchDataManager.getPlayerData(isLocal).useCard(realCard, GameManager.getCurrentTurnCount());
        }

        if (isLocal)
            await CardEffectManager.processLocalAiCard(card, targets);
        else
            await CardEffectManager.processEnemyAiCard(card, targets);
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, int currentTurnCount)
    {
        Card virtualCard = clonedAlly.getHandList().Find(c => c.getUniqueId() == card.getUniqueId());
        if (virtualCard == null) return;

        clonedAlly.useCard(virtualCard, currentTurnCount);

        CardEffectManager.getCardEffect(virtualCard).executeVirtually(clonedState, clonedAlly, clonedEnemy, targets, currentTurnCount);
    }

    public string getCardname() => card.getName();
    public EffectTargetPayload getTargets() => targets;
}