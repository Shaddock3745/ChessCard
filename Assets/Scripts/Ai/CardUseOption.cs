using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardUseOption : AiOption
{
    private float scroe;
    private Card card;
    private List<Vector2Int> targetPositions;
    private List<Card> targetCards;

    public CardUseOption(Card card, float scroe, List<Vector2Int> positions = null, List<Card> cards = null)
    {
        this.card = card;
        this.scroe = scroe;
        targetPositions = positions ?? new List<Vector2Int>();
        targetCards = cards ?? new List<Card>();
    }

    public float getScore() => scroe;
    public async Task execute(PlayerData ownerData)
    {
        if (ownerData.IsLocal())
            await CardEffectManager.processLocalAiCard(card, targetPositions, targetCards);
        else
            await CardEffectManager.processEnemyAiCard(card, targetPositions, targetCards);
    }

    public string getCardname() => card.getName();
}
