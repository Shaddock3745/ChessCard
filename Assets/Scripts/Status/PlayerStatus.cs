public enum PlayerStatusType
{
    CANT_MOVE,
    CANT_MOVE_SPECIFIC_CHESS_TYPE,
    CANT_SUMMON,
    CANT_ATTACK,
    CANT_USE_SPECIFIC_CARD,
    CANT_USE_ALL_CARD,
    MOVE_COST_MODIFIER,
    CARD_COST_MODIFIER,
}

public class PlayerStatus : BaseStatus<PlayerStatusType>
{
    public PlayerStatus(PlayerStatusType status, int startTurn, int endTurn) : base(status, startTurn, endTurn) { }
}

public class PlayerStatusContainer : StatusContainer<PlayerStatus, PlayerStatusType>
{
    public bool canSummon(int currentTotalTurn)
    {
        return !hasActiveStatus(PlayerStatusType.CANT_SUMMON, currentTotalTurn);
    }

    public int getGlobalExtraMoveCost(int currentTotalTurn)
    {
        int extraCost = 0;

        foreach (var status in GetActiveStatuses(PlayerStatusType.MOVE_COST_MODIFIER, currentTotalTurn))
            if (status is IncreasePlayerMoveCostStatus increaseGlobalMoveCostStatus)
                extraCost += increaseGlobalMoveCostStatus.getValue();

        return extraCost;
    }

    public int getGlobalExtraCardCost(int currentTotalTurn)
    {
        int extraCost = 0;

        foreach (var status in GetActiveStatuses(PlayerStatusType.CARD_COST_MODIFIER, currentTotalTurn))
            if (status is IncreasePlayerCardCostStatus increaseGlobalCardCostStatus)
                extraCost += increaseGlobalCardCostStatus.getValue();

        return extraCost;
    }

    public bool canUseCard(int cardId, int currentTotalTurn)
    {
        if (hasActiveStatus(PlayerStatusType.CANT_USE_ALL_CARD, currentTotalTurn))
            return false;

        foreach (var status in GetActiveStatuses(PlayerStatusType.CANT_USE_SPECIFIC_CARD, currentTotalTurn))
            if (status is PlayerCantUseSpecificCardStatus cantUseSpecificCardStatus && cantUseSpecificCardStatus.cardId == cardId)
                return false;

        return true;
    }

    public bool canMoveChess(ChessType chessType, int currentTotalTurn)
    {
        if (hasActiveStatus(PlayerStatusType.CANT_MOVE, currentTotalTurn))
            return false;

        foreach (var status in GetActiveStatuses(PlayerStatusType.CANT_MOVE_SPECIFIC_CHESS_TYPE, currentTotalTurn))
            if (status is PlayerCantMoveSpecificChessTypeStatus cantMoveSpecificChessTypeStatus && cantMoveSpecificChessTypeStatus.getChessType() == chessType)
                return false;

        return true;
    }

    public bool canAttack(int currentTotalTurn)
    {
        return !hasActiveStatus(PlayerStatusType.CANT_ATTACK, currentTotalTurn);
    }
}

public class IncreasePlayerCardCostStatus : PlayerStatus
{
    private int extraCost;

    public IncreasePlayerCardCostStatus(int extraCost, int startTurn, int endTurn) : base(PlayerStatusType.CARD_COST_MODIFIER, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;
}

public class IncreasePlayerMoveCostStatus : PlayerStatus
{
    private int extraCost;

    public IncreasePlayerMoveCostStatus(int extraCost, int startTurn, int endTurn) : base(PlayerStatusType.MOVE_COST_MODIFIER, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;
}

public class PlayerCantMoveStatus : PlayerStatus
{
    public PlayerCantMoveStatus(int startTurn, int endTurn) : base(PlayerStatusType.CANT_MOVE, startTurn, endTurn)
    {
    }
}

public class PlayerCantSummonStatus : PlayerStatus
{
    public PlayerCantSummonStatus(int startTurn, int endTurn) : base(PlayerStatusType.CANT_SUMMON, startTurn, endTurn)
    {
    }
}

public class PlayerCantAttackStatus : PlayerStatus
{
    public PlayerCantAttackStatus(int startTurn, int endTurn) : base(PlayerStatusType.CANT_ATTACK, startTurn, endTurn)
    {
    }
}

public class PlayerCantUseAllCardStatus : PlayerStatus
{
    public PlayerCantUseAllCardStatus(int startTurn, int endTurn) : base(PlayerStatusType.CANT_USE_ALL_CARD, startTurn, endTurn)
    {
    }
}

public class PlayerCantMoveSpecificChessTypeStatus : PlayerStatus
{
    ChessType chessType;

    public PlayerCantMoveSpecificChessTypeStatus(ChessType chessType, int startTurn, int endTurn) : base(PlayerStatusType.CANT_MOVE_SPECIFIC_CHESS_TYPE, startTurn, endTurn)
    {
        this.chessType = chessType;
    }

    public ChessType getChessType() => chessType;
}

public class PlayerCantUseSpecificCardStatus : PlayerStatus
{
    public int cardId;
    public PlayerCantUseSpecificCardStatus(int cardId, int startTurn, int endTurn) : base(PlayerStatusType.CANT_USE_SPECIFIC_CARD, startTurn, endTurn)
    {
        this.cardId = cardId;
    }

    public int getCardId() => cardId;
}
