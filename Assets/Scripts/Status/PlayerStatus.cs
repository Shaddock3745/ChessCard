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
    BONUS_HAND_CARD,
    BONUS_MANA,
    CARD_NON_COST,
    DISCARD_ALL_HAND_CARD_ON_TURN_END
}

public class PlayerStatus : BaseStatus<PlayerStatusType>
{
    public PlayerStatus(PlayerStatusType status, int startTurn, int endTurn) : base(status, startTurn, endTurn) { }
}

public class PlayerStatusContainer : StatusContainer<PlayerStatus, PlayerStatusType>
{
    public PlayerStatusContainer() : base()
    {

    }

    public bool canSummon(int currentTurnCount)
    {
        return !hasActiveStatus(PlayerStatusType.CANT_SUMMON, currentTurnCount);
    }

    public int getGlobalExtraMoveCost(int currentTurnCount)
    {
        int extraCost = 0;

        foreach (var status in GetActiveStatuses(PlayerStatusType.MOVE_COST_MODIFIER, currentTurnCount))
            if (status is IncreasePlayerMoveCostStatus increaseGlobalMoveCostStatus)
                extraCost += increaseGlobalMoveCostStatus.getValue();

        return extraCost;
    }

    public int getGlobalExtraCardCost(int currentTurnCount)
    {
        int extraCost = 0;

        foreach (var status in GetActiveStatuses(PlayerStatusType.CARD_COST_MODIFIER, currentTurnCount))
            if (status is IncreasePlayerCardCostStatus increaseGlobalCardCostStatus)
                extraCost += increaseGlobalCardCostStatus.getValue();

        return extraCost;
    }

    public int getBonusHandCardCount(int currentTurnCount)
    {
        int bonusHandCardCount = 0;

        foreach (var status in GetActiveStatuses(PlayerStatusType.BONUS_HAND_CARD, currentTurnCount))
            if (status is PlayerBonusHandCardStatus bonusHandCardStatus)
                bonusHandCardCount += bonusHandCardStatus.getBonusHandCardCount();

        return bonusHandCardCount;
    }

    public int getBonusManaCount(int currentTurnCount)
    {
        int bonusManaCount = 0;

        foreach (var status in GetActiveStatuses(PlayerStatusType.BONUS_MANA, currentTurnCount))
            if (status is PlayerBonusManaStatus bonusManaStatus)
                bonusManaCount += bonusManaStatus.getBonusManaCount();

        return bonusManaCount;
    }

    public bool canUseCard(int cardId, int currentTurnCount)
    {
        if (hasActiveStatus(PlayerStatusType.CANT_USE_ALL_CARD, currentTurnCount))
            return false;

        foreach (var status in GetActiveStatuses(PlayerStatusType.CANT_USE_SPECIFIC_CARD, currentTurnCount))
            if (status is PlayerCantUseSpecificCardStatus cantUseSpecificCardStatus && cantUseSpecificCardStatus.getCardId() == cardId)
                return false;

        return true;
    }

    public bool canMoveChess(ChessType chessType, int currentTurnCount)
    {
        if (hasActiveStatus(PlayerStatusType.CANT_MOVE, currentTurnCount))
            return false;

        foreach (var status in GetActiveStatuses(PlayerStatusType.CANT_MOVE_SPECIFIC_CHESS_TYPE, currentTurnCount))
            if (status is PlayerCantMoveSpecificChessTypeStatus cantMoveSpecificChessTypeStatus && cantMoveSpecificChessTypeStatus.getChessType() == chessType)
                return false;

        return true;
    }

    public bool canAttack(int currentTurnCount)
    {
        return !hasActiveStatus(PlayerStatusType.CANT_ATTACK, currentTurnCount);
    }

    public bool isCardNonCost(int currentTurnCount)
    {
        return hasActiveStatus(PlayerStatusType.CARD_NON_COST, currentTurnCount);
    }

    public bool isDiscardAllHandCardOnTurnEnd(int currentTurnCount)
    {
        return hasActiveStatus(PlayerStatusType.DISCARD_ALL_HAND_CARD_ON_TURN_END, currentTurnCount);
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
    private int cardId;

    public PlayerCantUseSpecificCardStatus(int cardId, int startTurn, int endTurn) : base(PlayerStatusType.CANT_USE_SPECIFIC_CARD, startTurn, endTurn)
    {
        this.cardId = cardId;
    }

    public int getCardId() => cardId;
}

public class PlayerBonusHandCardStatus : PlayerStatus
{
    private int bonusHandCardCount;

    public PlayerBonusHandCardStatus(int bonusHandCardCount, int startTurn, int endTurn) : base(PlayerStatusType.BONUS_HAND_CARD, startTurn, endTurn)
    {
        this.bonusHandCardCount = bonusHandCardCount;
    }

    public int getBonusHandCardCount() => bonusHandCardCount;
}

public class PlayerBonusManaStatus : PlayerStatus
{
    private int bonusManaCount;

    public PlayerBonusManaStatus(int bonusManaCount, int startTurn, int endTurn) : base(PlayerStatusType.BONUS_MANA, startTurn, endTurn)
    {
        this.bonusManaCount = bonusManaCount;
    }

    public int getBonusManaCount() => bonusManaCount;
}

public class PlayerCardNonCostStatus : PlayerStatus
{
    public PlayerCardNonCostStatus(int startTurn, int endTurn) : base(PlayerStatusType.CARD_NON_COST, startTurn, endTurn)
    {
    }
}

public class PlayerDiscardAllHandCardOnTurnEndStatus : PlayerStatus
{
    public PlayerDiscardAllHandCardOnTurnEndStatus(int startTurn, int endTurn) : base(PlayerStatusType.DISCARD_ALL_HAND_CARD_ON_TURN_END, startTurn, endTurn)
    {
    }
}
