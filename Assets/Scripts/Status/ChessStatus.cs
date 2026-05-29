using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public enum ChessStatusType
{
    CANT_MOVE,
    CANT_ATTACK,
    CANT_BE_ATTACKED,
    MOVE_COST_MODIFIER,
    MOVEMENT_OVERRIDE,
    CANT_ATTACK_SPECIFIC_TYPE_CHESS,
    EXTRA_MOVE,
    NON_COST_ON_MOVE,
    DO_ACTION_AFTER_MOVE,
    DO_ACTION_AFTER_ATTACK
}

public class ChessStatus : BaseStatus<ChessStatusType>
{
    public ChessStatus(ChessStatusType status, int startTurn, int endTurn) : base(status, startTurn, endTurn) { }

    public virtual void onPostMove(ChessPiece owner, Vector2Int fromPos, Vector2Int toPos) { }

    public virtual void onPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos) { }
}

public class ChessStatusContainer : StatusContainer<ChessStatus, ChessStatusType>
{
    public ChessStatusContainer() : base() { }

    public bool isNonCostOnMove(int currentTotalTurn)
    {
        return hasActiveStatus(ChessStatusType.NON_COST_ON_MOVE, currentTotalTurn);
    }

    public int getExtraMoveCost(int currentTotalTurn)
    {
        if (isNonCostOnMove(currentTotalTurn))
            return 0;

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

    public bool canAttackSpecificTypeChess(ChessType chessType, int currentTotalTurn)
    {
        foreach (var status in GetActiveStatuses(ChessStatusType.CANT_ATTACK_SPECIFIC_TYPE_CHESS, currentTotalTurn))
            if (status is ChessCantAttackSpecificTypeStatus chessCantAttackSpecificType && chessCantAttackSpecificType.getCantAttackType() == chessType)
                return false;

        return true;
    }

    public bool canBeAttacked(int currentTotalTurn)
    {
        return !hasActiveStatus(ChessStatusType.CANT_BE_ATTACKED, currentTotalTurn);
    }

    public MovementType getMovementOverride(int currentTotalTurn)
    {
        foreach (var status in GetActiveStatuses(ChessStatusType.MOVEMENT_OVERRIDE, currentTotalTurn))
            if (status is ChessMovementOverrideStatus movementOverrideStatus)
                return movementOverrideStatus.getMovementType();

        return null;
    }

    public int getExtraMoveCount(int currentTotalTurn)
    {
        int extraMoveCount = 0;

        foreach (var status in GetActiveStatuses(ChessStatusType.EXTRA_MOVE, currentTotalTurn))
            if (status is ChessExtraMoveStatus chessExtraMoveStatus)
                extraMoveCount += chessExtraMoveStatus.getExtraMoveCount();

        return extraMoveCount;
    }

    public void triggerPostMove(ChessPiece owner, Vector2Int fromPos, Vector2Int toPos, int currentTotalTurn)
    {
        foreach (var status in GetActiveStatuses(ChessStatusType.DO_ACTION_AFTER_MOVE, currentTotalTurn))
            status.onPostMove(owner, fromPos, toPos);
    }

    public void triggerPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos, int currentTotalTurn)
    {
        foreach (var status in GetActiveStatuses(ChessStatusType.DO_ACTION_AFTER_ATTACK, currentTotalTurn))
            status.onPostAttack(owner, victim, fromPos);
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
    private int extraCost;

    public IncreaseChessMoveCostStatus(int extraCost, int startTurn, int endTurn) : base(ChessStatusType.MOVE_COST_MODIFIER, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;
}

public class ChessNonCostOnMoveStatus : ChessStatus
{
    public ChessNonCostOnMoveStatus(int startTurn, int endTurn) : base(ChessStatusType.NON_COST_ON_MOVE, startTurn, endTurn) { }
}

public class ChessMovementOverrideStatus : ChessStatus
{
    private MovementType movementType;

    public ChessMovementOverrideStatus(MovementType movementType, int startTurn, int endTurn) : base(ChessStatusType.MOVEMENT_OVERRIDE, startTurn, endTurn)
    {
        this.movementType = movementType;
    }

    public MovementType getMovementType() => movementType;
}

public class ChessCantAttackSpecificTypeStatus : ChessStatus
{
    private ChessType type;

    public ChessCantAttackSpecificTypeStatus(ChessType type, int startTurn, int endTurn) : base(ChessStatusType.CANT_ATTACK_SPECIFIC_TYPE_CHESS, startTurn, endTurn)
    {
        this.type = type;
    }

    public ChessType getCantAttackType() => type;
}

public class ChessExtraMoveStatus : ChessStatus
{
    private int extraMoveCount;

    public ChessExtraMoveStatus(int extraMoveCount, int startTurn, int endTurn) : base(ChessStatusType.EXTRA_MOVE, startTurn, endTurn)
    {
        this.extraMoveCount = extraMoveCount;
    }

    public int getExtraMoveCount() => extraMoveCount;
}

public class PawnRammingStatus : ChessStatus
{
    public PawnRammingStatus(int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_MOVE, startTurn, endTurn) { }

    public override void onPostMove(ChessPiece owner, Vector2Int fromPos, Vector2Int toPos)
    {
        Vector2Int forwardDir = owner.isWhite() ? Vector2Int.up : Vector2Int.down;
        Vector2Int frontPos = toPos + forwardDir;

        if (!BoardManager.isEmpty(frontPos))
            _ = BoardManager.pieceGotRoared(frontPos, forwardDir, owner.isWhite());
    }
}

public class ChessPartingShotStatus : ChessStatus
{
    public ChessPartingShotStatus(int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_ATTACK, startTurn, endTurn) { }

    public override void onPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        _ = BoardManager.movePieceWithAnimation(owner.getPosition(), fromPos);
    }
}

public class ChessConversionStatus : ChessStatus
{
    public ChessConversionStatus(int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_ATTACK, startTurn, endTurn) { }

    public override void onPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        ChessType victimType = victim.getChessType();
        int cardId = CardEffectManager.getCardIdByChessType(victimType);

        if (cardId != -1)
            MatchDataManager.materializeCard(GameManager.IsLocalWhite() == owner.isWhite(), cardId, true);
    }
}

public class KnightTrampleStatus : ChessStatus
{
    public KnightTrampleStatus(int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_MOVE, startTurn, endTurn) { }

    public override void onPostMove(ChessPiece owner, Vector2Int fromPos, Vector2Int toPos)
    {
        int minX = Mathf.Min(fromPos.x, toPos.x);
        int maxX = Mathf.Max(fromPos.x, toPos.x);
        int minY = Mathf.Min(fromPos.y, toPos.y);
        int maxY = Mathf.Max(fromPos.y, toPos.y);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int checkPos = new Vector2Int(x, y);

                if (checkPos == fromPos || checkPos == toPos)
                    continue;

                if (!BoardManager.isEmpty(checkPos) && BoardManager.isWhiteAtPosition(checkPos) != owner.isWhite())
                    _ = BoardManager.removePieceWithAnimation(checkPos);
            }
    }
}

public class BattleHardenedPostAttackStatus : ChessStatus
{
    public BattleHardenedPostAttackStatus(int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_ATTACK, startTurn, endTurn) { }

    public override void onPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        int currentTurn = GameManager.getCurrentTurnCount();
        owner.addStatus(new ChessExtraMoveStatus(1, currentTurn, currentTurn + 1));
    }
}