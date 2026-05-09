public enum ChessStatusType
{
    CANT_MOVE,
    CANT_ATTACK,
    CANT_BE_ATTACKED,
    MOVE_COST_MODIFIER,
    MOVEMENT_OVERRIDE
}

public class ChessStatus : BaseStatus<ChessStatusType>
{
    public ChessStatus(ChessStatusType status, int startTurn, int endTurn) : base(status, startTurn, endTurn)
    {

    }
}

public class ChessStatusContainer : StatusContainer<ChessStatus, ChessStatusType>
{
    public ChessStatusContainer() : base()
    {

    }

    public int getExtraMoveCost(int currentTotalTurn)
    {
        int extraCost = 0;

        foreach (var status in GetActiveStatuses(ChessStatusType.MOVE_COST_MODIFIER, currentTotalTurn))
            if (status is IncreaseChessMoveCostStatus increaseChessMoveCostStatus)
                extraCost += increaseChessMoveCostStatus.getValue();

        return extraCost;
    }

    public bool canMove(int currentTotalTurn)
    {
        return !hasActiveStatus(ChessStatusType.CANT_MOVE, currentTotalTurn);
    }

    public bool canAttack(int currentTotalTurn)
    {
        return !hasActiveStatus(ChessStatusType.CANT_ATTACK, currentTotalTurn);
    }

    public bool canBeAttacked(int currentTotalTurn)
    {
        return !hasActiveStatus(ChessStatusType.CANT_BE_ATTACKED, currentTotalTurn);
    }

    public MovementType getMovementOverride(int currentTotalTurn)
    {
        foreach (var status in GetActiveStatuses(ChessStatusType.MOVEMENT_OVERRIDE, currentTotalTurn))
            if (status is ChessMovementOverrideStatus movementOverrideStatus)
                return movementOverrideStatus.movementType;

        return null;
    }
}

public class ChessCantMoveStatus : ChessStatus
{
    public ChessCantMoveStatus(int startTurn, int endTurn) : base(ChessStatusType.CANT_MOVE, startTurn, endTurn) { }
}

public class ChessCantAttackStatus : ChessStatus
{
    public ChessCantAttackStatus(int startTurn, int endTurn) : base(ChessStatusType.CANT_ATTACK, startTurn, endTurn) { }
}

public class ChessCantBeAttackedStatus : ChessStatus
{
    public ChessCantBeAttackedStatus(int startTurn, int endTurn) : base(ChessStatusType.CANT_BE_ATTACKED, startTurn, endTurn) { }
}


public class IncreaseChessMoveCostStatus : ChessStatus
{
    int extraCost;

    public IncreaseChessMoveCostStatus(int extraCost, int startTurn, int endTurn) : base(ChessStatusType.MOVE_COST_MODIFIER, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;
}

public class ChessMovementOverrideStatus : ChessStatus
{
    public MovementType movementType;
    public ChessMovementOverrideStatus(MovementType movementType, int startTurn, int endTurn) : base(ChessStatusType.MOVEMENT_OVERRIDE, startTurn, endTurn)
    {
        this.movementType = movementType;
    }
}