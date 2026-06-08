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
    DISCARD_ALL_HAND_CARD_ON_TURN_END,
    LIMITED_TO_MOVE_SPECIFIC_CHESS,
    EXTRA_MOVE
}

public abstract class PlayerStatus : BaseStatus<PlayerStatusType>
{
    public PlayerStatus(PlayerStatusType status, bool isPermanetStatus, int startTurn, int endTurn) : base(status, isPermanetStatus, startTurn, endTurn) { }

    public virtual float getAiValue(bool isLocal, int currentTurnCount) => 0f;

    public abstract override BaseStatus<PlayerStatusType> Clone();
}

public class PlayerStatusContainer : StatusContainer<PlayerStatus, PlayerStatusType>
{
    public PlayerStatusContainer() : base()
    {
    }

    public PlayerStatusContainer Clone()
    {
        PlayerStatusContainer clone = new PlayerStatusContainer();
        clone.DeepCopyStatuses(this);
        return clone;
    }

    public float getStatusContainerAiValue(bool isLocal, int currentTurnCount)
    {
        float result = 0f;

        foreach (var status in activeStatuses)
            if (status.isActive(currentTurnCount))
                result += status.getAiValue(isLocal, currentTurnCount);

        return result;
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

    public bool isLimitedToMoveSpecificChess(int currentTurnCount)
    {
        return hasActiveStatus(PlayerStatusType.LIMITED_TO_MOVE_SPECIFIC_CHESS, currentTurnCount);
    }

    public int? getLimitedToMoveSpecificChessUniqueId(int currentTurnCount)
    {
        foreach (var status in GetActiveStatuses(PlayerStatusType.LIMITED_TO_MOVE_SPECIFIC_CHESS, currentTurnCount))
            if (status is PlayerLimitedToMoveSpecificChessStatus s)
                return s.getChessUniqueId();

        return null;
    }

    public int getExtraMoveCount(int currentTurnCount)
    {
        int extraMoveCount = 0;
        foreach (var status in GetActiveStatuses(PlayerStatusType.EXTRA_MOVE, currentTurnCount))
            if (status is PlayerExtraMoveStatus playerExtraMoveStatus)
                extraMoveCount += playerExtraMoveStatus.getExtraMoveCount();
        return extraMoveCount;
    }
}

public class IncreasePlayerCardCostStatus : PlayerStatus
{
    private int extraCost;

    public IncreasePlayerCardCostStatus(int extraCost, bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.CARD_COST_MODIFIER, isPermanetStatus, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -10f * extraCost;

    public override BaseStatus<PlayerStatusType> Clone() => new IncreasePlayerCardCostStatus(this.extraCost, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class IncreasePlayerMoveCostStatus : PlayerStatus
{
    private int extraCost;

    public IncreasePlayerMoveCostStatus(int extraCost, bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.MOVE_COST_MODIFIER, isPermanetStatus, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -10f * extraCost;

    public override BaseStatus<PlayerStatusType> Clone() => new IncreasePlayerMoveCostStatus(this.extraCost, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerCantMoveStatus : PlayerStatus
{
    public PlayerCantMoveStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.CANT_MOVE, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount)
    {
        if (MatchDataManager.hadMovedThisTurn(isLocal))
            return -4f;
        return -40f;
    }

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantMoveStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerCantSummonStatus : PlayerStatus
{
    public PlayerCantSummonStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.CANT_SUMMON, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => -20f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantSummonStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerCantAttackStatus : PlayerStatus
{
    public PlayerCantAttackStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.CANT_ATTACK, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => -40f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantAttackStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerCantUseAllCardStatus : PlayerStatus
{
    public PlayerCantUseAllCardStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.CANT_USE_ALL_CARD, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => -40f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantUseAllCardStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerCantMoveSpecificChessTypeStatus : PlayerStatus
{
    private ChessType chessType;

    public PlayerCantMoveSpecificChessTypeStatus(ChessType chessType, bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.CANT_MOVE_SPECIFIC_CHESS_TYPE, isPermanetStatus, startTurn, endTurn)
    {
        this.chessType = chessType;
    }

    public ChessType getChessType() => chessType;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -15f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantMoveSpecificChessTypeStatus(this.chessType, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerCantUseSpecificCardStatus : PlayerStatus
{
    private int cardId;

    public PlayerCantUseSpecificCardStatus(int cardId, bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.CANT_USE_SPECIFIC_CARD, isPermanetStatus, startTurn, endTurn)
    {
        this.cardId = cardId;
    }

    public int getCardId() => cardId;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -5f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantUseSpecificCardStatus(this.cardId, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerBonusHandCardStatus : PlayerStatus
{
    private int bonusHandCardCount;

    public PlayerBonusHandCardStatus(int bonusHandCardCount, bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.BONUS_HAND_CARD, isPermanetStatus, startTurn, endTurn)
    {
        this.bonusHandCardCount = bonusHandCardCount;
    }

    public int getBonusHandCardCount() => bonusHandCardCount;

    public override float getAiValue(bool isLocal, int currentTurnCount)
    {
        float baseValue = 15f * bonusHandCardCount;
        if (getStartTurn() == currentTurnCount && MatchDataManager.hadMovedThisTurn(isLocal))
            return baseValue * 0.4f;

        return baseValue;
    }

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerBonusHandCardStatus(this.bonusHandCardCount, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerBonusManaStatus : PlayerStatus
{
    private int bonusManaCount;

    public PlayerBonusManaStatus(int bonusManaCount, bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.BONUS_MANA, isPermanetStatus, startTurn, endTurn)
    {
        this.bonusManaCount = bonusManaCount;
    }

    public int getBonusManaCount() => bonusManaCount;

    public override float getAiValue(bool isLocal, int currentTurnCount)
    {
        float baseValue = 10f * bonusManaCount;
        if (getStartTurn() == currentTurnCount && MatchDataManager.hadMovedThisTurn(isLocal))
            return baseValue * 0.5f;

        return baseValue;
    }

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerBonusManaStatus(this.bonusManaCount, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerCardNonCostStatus : PlayerStatus
{
    public PlayerCardNonCostStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.CARD_NON_COST, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => 0f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCardNonCostStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerDiscardAllHandCardOnTurnEndStatus : PlayerStatus
{
    public PlayerDiscardAllHandCardOnTurnEndStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.DISCARD_ALL_HAND_CARD_ON_TURN_END, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => -25f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerDiscardAllHandCardOnTurnEndStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerLimitedToMoveSpecificChessStatus : PlayerStatus
{
    private int chessUniqueId;

    public PlayerLimitedToMoveSpecificChessStatus(int chessUniqueId, bool isPermanetStatus, int startTurn, int endTurn)
        : base(PlayerStatusType.LIMITED_TO_MOVE_SPECIFIC_CHESS, isPermanetStatus, startTurn, endTurn)
    {
        this.chessUniqueId = chessUniqueId;
    }

    public int getChessUniqueId() => chessUniqueId;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -20f;

    public override BaseStatus<PlayerStatusType> Clone()
        => new PlayerLimitedToMoveSpecificChessStatus(this.chessUniqueId, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PlayerExtraMoveStatus : PlayerStatus
{
    private int extraMoveCount;

    public PlayerExtraMoveStatus(int extraMoveCount, bool isPermanetStatus, int startTurn, int endTurn) : base(PlayerStatusType.EXTRA_MOVE, isPermanetStatus, startTurn, endTurn)
    {
        this.extraMoveCount = extraMoveCount;
    }

    public int getExtraMoveCount() => extraMoveCount;

    public override float getAiValue(bool isLocal, int currentTurnCount) => 0f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerExtraMoveStatus(this.extraMoveCount, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}