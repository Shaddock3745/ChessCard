using System.Collections.Generic;
using UnityEngine;

public class EffectTargetPayload
{
    private List<Vector2Int> targetPositions;
    private List<Card> targetCards;

    private static readonly EffectTargetPayload emptyPayload = new EffectTargetPayload();

    public static EffectTargetPayload empty()
    {
        return emptyPayload;
    }

    public EffectTargetPayload()
    {
        this.targetPositions = new List<Vector2Int>();
        this.targetCards = new List<Card>();
    }

    public EffectTargetPayload(List<Vector2Int> targetPositions)
    {
        this.targetPositions = targetPositions;
        this.targetCards = new List<Card>();
    }

    public EffectTargetPayload(List<Card> targetCards)
    {
        this.targetPositions = new List<Vector2Int>();
        this.targetCards = targetCards;
    }

    public EffectTargetPayload(List<Vector2Int> targetPositions, List<Card> targetCards)
    {
        this.targetPositions = targetPositions;
        this.targetCards = targetCards;
    }

    public List<Vector2Int> getTargetPositions() => targetPositions;

    public List<Card> getTargetCards() => targetCards;

    public virtual bool isEmpty() => targetPositions.Count == 0 && targetCards.Count == 0;
}
