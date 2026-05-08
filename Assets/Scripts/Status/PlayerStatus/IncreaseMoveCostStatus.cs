using UnityEngine;

public class IncreaseMoveCostStatus : PlayerStatus
{
    private int extraCost;

    public IncreaseMoveCostStatus(int extraCost, int startTurn, int endTurn) : base(PlayerStatusType.MOVE_COST_MODIFIER, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getExtraCost() => extraCost;
}