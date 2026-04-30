using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardUseOption : AiOption
{
    private float scroe;
    private Card card;
    EffectTargetPayload targets;

    public CardUseOption(Card card, float scroe, List<Vector2Int> positions = null, List<Card> cards = null)
    {
        this.card = card;
        this.scroe = scroe;
        this.targets = new EffectTargetPayload(positions, cards);
    }

    public float getScore() => scroe;
    public async Task execute(PlayerData ownerData)
    {

        if (ownerData.IsLocal())
            await CardEffectManager.processLocalAiCard(card, targets);
        else
            await CardEffectManager.processEnemyAiCard(card, targets);
    }

    public string getCardname() => card.getName();
}
