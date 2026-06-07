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

public abstract class ChessStatus : BaseStatus<ChessStatusType>
{
    public ChessStatus(ChessStatusType status, bool isPermanetStatus, int startTurn, int endTurn) : base(status, isPermanetStatus, startTurn, endTurn) { }

    public virtual float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 0f;

    // 真實遊戲執行
    public virtual void onPostMove(ChessPiece owner, Vector2Int fromPos, Vector2Int toPos) { }
    public virtual void onPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos) { }

    // ★ 虛擬沙盒執行：專供 DFS 推演使用，只修改虛擬盤面
    public virtual void virtualOnPostMove(BoardState state, ChessPiece owner, Vector2Int fromPos, Vector2Int toPos) { }
    public virtual void virtualOnPostAttack(BoardState state, ChessPiece owner, ChessPiece victim, Vector2Int fromPos) { }

    public abstract override BaseStatus<ChessStatusType> Clone();
}

public class ChessStatusContainer : StatusContainer<ChessStatus, ChessStatusType>
{
    public ChessStatusContainer() : base() { }

    public ChessStatusContainer Clone()
    {
        ChessStatusContainer clone = new ChessStatusContainer();
        clone.DeepCopyStatuses(this);
        return clone;
    }

    public float getStatusContainerAiValue(bool isLocal, ChessPiece owner, int currentTurnCount)
    {
        float result = 0f;

        foreach (var status in activeStatuses)
            if (status.isActive(currentTurnCount))
                result += status.getAiValue(isLocal, owner, currentTurnCount);

        return result;
    }

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

    // ★ 提供給 ChessMoveOption 虛擬執行的觸發介面
    public void virtualTriggerPostMove(BoardState state, ChessPiece owner, Vector2Int fromPos, Vector2Int toPos, int currentTotalTurn)
    {
        foreach (var status in GetActiveStatuses(ChessStatusType.DO_ACTION_AFTER_MOVE, currentTotalTurn))
            status.virtualOnPostMove(state, owner, fromPos, toPos);
    }

    public void virtualTriggerPostAttack(BoardState state, ChessPiece owner, ChessPiece victim, Vector2Int fromPos, int currentTotalTurn)
    {
        foreach (var status in GetActiveStatuses(ChessStatusType.DO_ACTION_AFTER_ATTACK, currentTotalTurn))
            status.virtualOnPostAttack(state, owner, victim, fromPos);
    }
}

public class ChessCantMoveStatus : ChessStatus
{
    public ChessCantMoveStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.CANT_MOVE, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => -20f;

    public override BaseStatus<ChessStatusType> Clone() => new ChessCantMoveStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class ChessCantAttackStatus : ChessStatus
{
    public ChessCantAttackStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.CANT_ATTACK, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => -20f;

    public override BaseStatus<ChessStatusType> Clone() => new ChessCantAttackStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class ChessCantBeAttackedStatus : ChessStatus
{
    public ChessCantBeAttackedStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.CANT_BE_ATTACKED, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 40f;

    public override BaseStatus<ChessStatusType> Clone() => new ChessCantBeAttackedStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class IncreaseChessMoveCostStatus : ChessStatus
{
    private int extraCost;

    public IncreaseChessMoveCostStatus(int extraCost, bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.MOVE_COST_MODIFIER, isPermanetStatus, startTurn, endTurn)
    {
        this.extraCost = extraCost;
    }

    public int getValue() => extraCost;

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => -5f * extraCost;

    public override BaseStatus<ChessStatusType> Clone() => new IncreaseChessMoveCostStatus(this.extraCost, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class ChessNonCostOnMoveStatus : ChessStatus
{
    public ChessNonCostOnMoveStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.NON_COST_ON_MOVE, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 0f;

    public override BaseStatus<ChessStatusType> Clone() => new ChessNonCostOnMoveStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class ChessMovementOverrideStatus : ChessStatus
{
    private MovementType movementType;

    public ChessMovementOverrideStatus(MovementType movementType, bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.MOVEMENT_OVERRIDE, isPermanetStatus, startTurn, endTurn)
    {
        this.movementType = movementType;
    }

    public MovementType getMovementType() => movementType;

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 0f;

    public override BaseStatus<ChessStatusType> Clone() => new ChessMovementOverrideStatus(this.movementType, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class ChessCantAttackSpecificTypeStatus : ChessStatus
{
    private ChessType type;

    public ChessCantAttackSpecificTypeStatus(ChessType type, bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.CANT_ATTACK_SPECIFIC_TYPE_CHESS, isPermanetStatus, startTurn, endTurn)
    {
        this.type = type;
    }

    public ChessType getCantAttackType() => type;

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => -5f;

    public override BaseStatus<ChessStatusType> Clone() => new ChessCantAttackSpecificTypeStatus(this.type, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class ChessExtraMoveStatus : ChessStatus
{
    private int extraMoveCount;

    public ChessExtraMoveStatus(int extraMoveCount, bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.EXTRA_MOVE, isPermanetStatus, startTurn, endTurn)
    {
        this.extraMoveCount = extraMoveCount;
    }

    public int getExtraMoveCount() => extraMoveCount;

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 0f;

    public override BaseStatus<ChessStatusType> Clone() => new ChessExtraMoveStatus(this.extraMoveCount, this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class PawnRammingStatus : ChessStatus
{
    public PawnRammingStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_MOVE, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 15f;

    public override void onPostMove(ChessPiece owner, Vector2Int fromPos, Vector2Int toPos)
    {
        Vector2Int forwardDir = owner.isWhite() ? Vector2Int.up : Vector2Int.down;
        Vector2Int frontPos = toPos + forwardDir;

        if (!BoardManager.isEmpty(frontPos))
            _ = BoardManager.pieceGotRoared(frontPos, forwardDir, owner.isWhite());
    }

    public override void virtualOnPostMove(BoardState state, ChessPiece owner, Vector2Int fromPos, Vector2Int toPos)
    {
        Vector2Int forwardDir = owner.isWhite() ? Vector2Int.up : Vector2Int.down;
        Vector2Int frontPos = toPos + forwardDir;

        if (frontPos.x >= 0 && frontPos.x < 8 && frontPos.y >= 0 && frontPos.y < 8)
        {
            ChessPiece affectedPiece = state.getChessPieceAtPosition(frontPos.x, frontPos.y);
            if (affectedPiece != null && affectedPiece.isWhite() != owner.isWhite())
            {
                Vector2Int pushedPos = frontPos + forwardDir;
                if (pushedPos.x >= 0 && pushedPos.x < 8 && pushedPos.y >= 0 && pushedPos.y < 8)
                {
                    if (state.getChessPieceAtPosition(pushedPos.x, pushedPos.y) == null)
                        state.moveChessPiece(frontPos, pushedPos);
                }
            }
        }
    }

    public override BaseStatus<ChessStatusType> Clone() => new PawnRammingStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class ChessPartingShotStatus : ChessStatus
{
    public ChessPartingShotStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_ATTACK, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 20f;

    public override void onPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        _ = BoardManager.movePieceWithAnimationByEffect(owner.getPosition(), fromPos);
    }

    public override void virtualOnPostAttack(BoardState state, ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        state.moveChessPiece(owner.getPosition(), fromPos);
    }

    public override BaseStatus<ChessStatusType> Clone() => new ChessPartingShotStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class ChessConversionStatus : ChessStatus
{
    public ChessConversionStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_ATTACK, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 30f;

    public override void onPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        ChessType victimType = victim.getChessType();
        int cardId = CardEffectManager.getCardIdByChessType(victimType);

        if (cardId != -1)
            MatchDataManager.materializeCard(GameManager.IsLocalWhite() == owner.isWhite(), cardId, true);
    }

    public override void virtualOnPostAttack(BoardState state, ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        // 虛擬環境中不直接改變手牌資料 (PlayerData)，單純完成盤面戰略分數評估即可
    }

    public override BaseStatus<ChessStatusType> Clone() => new ChessConversionStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class KnightTrampleStatus : ChessStatus
{
    public KnightTrampleStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_MOVE, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 25f;

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

    public override void virtualOnPostMove(BoardState state, ChessPiece owner, Vector2Int fromPos, Vector2Int toPos)
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

                ChessPiece target = state.getChessPieceAtPosition(checkPos.x, checkPos.y);
                if (target != null && target.isWhite() != owner.isWhite())
                {
                    // 虛擬移除：只修改虛擬盤面，絕對不能呼叫 BoardManager
                    state.setChessPieceAtPosition(checkPos.x, checkPos.y, null);
                }
            }
    }

    public override BaseStatus<ChessStatusType> Clone() => new KnightTrampleStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}

public class BattleHardenedPostAttackStatus : ChessStatus
{
    public BattleHardenedPostAttackStatus(bool isPermanetStatus, int startTurn, int endTurn) : base(ChessStatusType.DO_ACTION_AFTER_ATTACK, isPermanetStatus, startTurn, endTurn) { }

    public override float getAiValue(bool isLocal, ChessPiece owner, int currentTurnCount) => 25f;

    public override void onPostAttack(ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        int currentTurn = GameManager.getCurrentTurnCount();
        BoardManager.addStatus(owner, new ChessExtraMoveStatus(1, false, currentTurn, currentTurn + 1));
    }

    public override void virtualOnPostAttack(BoardState state, ChessPiece owner, ChessPiece victim, Vector2Int fromPos)
    {
        int currentTurn = GameManager.getCurrentTurnCount();
        owner.addStatus(new ChessExtraMoveStatus(1, false, currentTurn, currentTurn + 1));
    }

    public override BaseStatus<ChessStatusType> Clone() => new BattleHardenedPostAttackStatus(this.isPermanet(), this.getStartTurn(), this.getEndTurn());
}