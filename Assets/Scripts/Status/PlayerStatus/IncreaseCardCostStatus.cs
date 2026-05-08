using UnityEngine;

public class IncreaseCardCostStatus : PlayerStatus
{
    private int extraCost;

    public IncreaseCardCostStatus(int extraCost, int startTurn, int endTurn) : base(PlayerStatusType.CARD_COST_MODIFIER, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getExtraCost() => extraCost;
}
