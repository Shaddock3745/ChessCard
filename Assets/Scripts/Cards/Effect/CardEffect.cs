using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface CardEffect
{
    List<CardUseOption> getUseOptions(Card card, int currentMana);

    Task executeEffect(bool isPlayer, Card card, List<Vector2Int> targetPositions);
}
