using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardUseOption : AiOption
{
    private float score;
    private Card card;
    EffectTargetPayload targets;

    public CardUseOption(Card card, float scroe, EffectTargetPayload targets = null)
    {
        this.card = card;
        this.score = scroe;
        this.targets = targets ?? EffectTargetPayload.empty();
    }

    public float getScore() => score;

    public async Task execute(PlayerData ownerData)
    {
        if (ownerData.IsLocal())
            await CardEffectManager.processLocalAiCard(card, targets);
        else
            await CardEffectManager.processEnemyAiCard(card, targets);
    }

    public string getCardname() => card.getName();

    public EffectTargetPayload getTargets() => targets;
}
