using System.Collections.Generic;
using UnityEngine;

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
    public PlayerStatus(PlayerStatusType status, int startTurn, int endTurn) : base(status, startTurn, endTurn) { }

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

    public ChessPiece getLimitedToMoveSpecificChess(int currentTurnCount)
    {
        foreach (var status in GetActiveStatuses(PlayerStatusType.LIMITED_TO_MOVE_SPECIFIC_CHESS, currentTurnCount))
            if (status is PlayerLimitedToMoveSpecificChessStatus limitedToMoveSpecificChessStatus)
                return limitedToMoveSpecificChessStatus.getChessPiece();

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

    public IncreasePlayerCardCostStatus(int extraCost, int startTurn, int endTurn) : base(PlayerStatusType.CARD_COST_MODIFIER, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -10f * extraCost;

    public override BaseStatus<PlayerStatusType> Clone() => new IncreasePlayerCardCostStatus(this.extraCost, this.startTurn, this.endTurn);
}

public class IncreasePlayerMoveCostStatus : PlayerStatus
{
    private int extraCost;

    public IncreasePlayerMoveCostStatus(int extraCost, int startTurn, int endTurn) : base(PlayerStatusType.MOVE_COST_MODIFIER, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -10f * extraCost;

    public override BaseStatus<PlayerStatusType> Clone() => new IncreasePlayerMoveCostStatus(this.extraCost, this.startTurn, this.endTurn);
}

public class PlayerCantMoveStatus : PlayerStatus
{
    public PlayerCantMoveStatus(int startTurn, int endTurn) : base(PlayerStatusType.CANT_MOVE, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount)
    {
        if (MatchDataManager.hadMovedThisTurn(isLocal))
            return -4f;
        return -40f;
    }

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantMoveStatus(this.startTurn, this.endTurn);
}

public class PlayerCantSummonStatus : PlayerStatus
{
    public PlayerCantSummonStatus(int startTurn, int endTurn) : base(PlayerStatusType.CANT_SUMMON, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => -20f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantSummonStatus(this.startTurn, this.endTurn);
}

public class PlayerCantAttackStatus : PlayerStatus
{
    public PlayerCantAttackStatus(int startTurn, int endTurn) : base(PlayerStatusType.CANT_ATTACK, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => -40f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantAttackStatus(this.startTurn, this.endTurn);
}

public class PlayerCantUseAllCardStatus : PlayerStatus
{
    public PlayerCantUseAllCardStatus(int startTurn, int endTurn) : base(PlayerStatusType.CANT_USE_ALL_CARD, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => -40f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantUseAllCardStatus(this.startTurn, this.endTurn);
}

public class PlayerCantMoveSpecificChessTypeStatus : PlayerStatus
{
    private ChessType chessType;

    public PlayerCantMoveSpecificChessTypeStatus(ChessType chessType, int startTurn, int endTurn) : base(PlayerStatusType.CANT_MOVE_SPECIFIC_CHESS_TYPE, startTurn, endTurn)
    {
        this.chessType = chessType;
    }

    public ChessType getChessType() => chessType;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -15f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantMoveSpecificChessTypeStatus(this.chessType, this.startTurn, this.endTurn);
}

public class PlayerCantUseSpecificCardStatus : PlayerStatus
{
    private int cardId;

    public PlayerCantUseSpecificCardStatus(int cardId, int startTurn, int endTurn) : base(PlayerStatusType.CANT_USE_SPECIFIC_CARD, startTurn, endTurn)
    {
        this.cardId = cardId;
    }

    public int getCardId() => cardId;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -5f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCantUseSpecificCardStatus(this.cardId, this.startTurn, this.endTurn);
}

public class PlayerBonusHandCardStatus : PlayerStatus
{
    private int bonusHandCardCount;

    public PlayerBonusHandCardStatus(int bonusHandCardCount, int startTurn, int endTurn) : base(PlayerStatusType.BONUS_HAND_CARD, startTurn, endTurn)
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

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerBonusHandCardStatus(this.bonusHandCardCount, this.startTurn, this.endTurn);
}

public class PlayerBonusManaStatus : PlayerStatus
{
    private int bonusManaCount;

    public PlayerBonusManaStatus(int bonusManaCount, int startTurn, int endTurn) : base(PlayerStatusType.BONUS_MANA, startTurn, endTurn)
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

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerBonusManaStatus(this.bonusManaCount, this.startTurn, this.endTurn);
}

public class PlayerCardNonCostStatus : PlayerStatus
{
    public PlayerCardNonCostStatus(int startTurn, int endTurn) : base(PlayerStatusType.CARD_NON_COST, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => 40f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerCardNonCostStatus(this.startTurn, this.endTurn);
}

public class PlayerDiscardAllHandCardOnTurnEndStatus : PlayerStatus
{
    public PlayerDiscardAllHandCardOnTurnEndStatus(int startTurn, int endTurn) : base(PlayerStatusType.DISCARD_ALL_HAND_CARD_ON_TURN_END, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, int currentTurnCount) => -25f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerDiscardAllHandCardOnTurnEndStatus(this.startTurn, this.endTurn);
}

public class PlayerLimitedToMoveSpecificChessStatus : PlayerStatus
{
    private ChessPiece chessPiece;
    public PlayerLimitedToMoveSpecificChessStatus(ChessPiece chessPiece, int startTurn, int endTurn) : base(PlayerStatusType.LIMITED_TO_MOVE_SPECIFIC_CHESS, startTurn, endTurn)
    {
        this.chessPiece = chessPiece;
    }
    public ChessPiece getChessPiece() => chessPiece;

    public override float getAiValue(bool isLocal, int currentTurnCount) => -20f;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerLimitedToMoveSpecificChessStatus(this.chessPiece, this.startTurn, this.endTurn);
}

public class PlayerExtraMoveStatus : PlayerStatus
{
    private int extraMoveCount;

    public PlayerExtraMoveStatus(int extraMoveCount, int startTurn, int endTurn) : base(PlayerStatusType.EXTRA_MOVE, startTurn, endTurn)
    {
        this.extraMoveCount = extraMoveCount;
    }

    public int getExtraMoveCount() => extraMoveCount;

    public override float getAiValue(bool isLocal, int currentTurnCount) => 30f * extraMoveCount;

    public override BaseStatus<PlayerStatusType> Clone() => new PlayerExtraMoveStatus(this.extraMoveCount, this.startTurn, this.endTurn);
}