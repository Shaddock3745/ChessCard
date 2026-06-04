using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct ActionRecord
{
    public ChessPiece piece;
    public Vector2Int fromPos;
    public Vector2Int toPos;
    public ActionType type;

    public ActionRecord(ChessPiece piece, Vector2Int fromPos, Vector2Int toPos, ActionType type)
    {
        this.piece = piece;
        this.fromPos = fromPos;
        this.toPos = toPos;
        this.type = type;
    }
}

public class ActionValidator
{
    private static readonly int MAX_MOVE_COUNT_PER_TURN = 1;
    private static readonly List<MoveAction> rawMovesBuffer = new List<MoveAction>(64);

    public static bool canHoldCard(PlayerData playerData)
    {
        return playerData.canHoldCard();
    }

    public static bool canUseCard(PlayerData player, Card card, int currentTurnCount)
    {
        int extraCost = player.getStatusContainer().getGlobalExtraCardCost(currentTurnCount);
        int finalCost = player.isCardNonCost(currentTurnCount) ? 0 : card.getCost() + extraCost;

        return player.getMana() >= finalCost
            && player.getStatusContainer().canUseCard(card.getId(), currentTurnCount)
            && CardEffectManager.canActivateCard(player.IsLocal(), card, currentTurnCount);
    }

    public static bool canAttackGlobally(PlayerData player, int currentTurnCount)
    {
        return player.getStatusContainer().canAttack(currentTurnCount);
    }

    private static int calculateGlobalMovesUsed(List<ActionRecord> records, int currentTurnCount)
    {
        // ★ 修正：改用 getInitialPosition() 作為群組依據，避免 DFS 深拷貝後，新舊物件 Reference 不同導致計算錯誤
        var movesByPiece = records.GroupBy(r => r.piece.getInitialPosition());

        int globalMovesUsed = 0;
        foreach (var group in movesByPiece)
        {
            ChessPiece movedPiece = group.First().piece;
            globalMovesUsed += Mathf.Max(0, group.Count() - movedPiece.getExtraMoveCount(currentTurnCount));
        }
        return globalMovesUsed;
    }

    public static bool reachMoveLimit(PlayerData player, int currentTurnCount)
    {
        return calculateGlobalMovesUsed(player.getChessMoveRecords(), currentTurnCount) >= MAX_MOVE_COUNT_PER_TURN + player.getStatusContainer().getExtraMoveCount(currentTurnCount);
    }

    public static bool canPieceMoveThisTurn(PlayerData player, ChessPiece piece, int currentTurnCount)
    {
        if (player.getStatusContainer().isLimitedToMoveSpecificChess(currentTurnCount))
        {
            ChessPiece limitedPiece = player.getStatusContainer().getLimitedToMoveSpecificChess(currentTurnCount);
            // ★ 同理，改比對初始座標
            if (limitedPiece != null && limitedPiece.getInitialPosition() != piece.getInitialPosition())
                return false;
        }

        List<ActionRecord> records = player.getChessMoveRecords();

        // ★ 修正：比對 getInitialPosition() 而非直接比對 Reference
        int pieceMoves = records.Count(r => r.piece.getInitialPosition() == piece.getInitialPosition() && r.type == ActionType.Move);
        int pieceBonusMoves = piece.getExtraMoveCount(currentTurnCount);

        if (pieceMoves >= MAX_MOVE_COUNT_PER_TURN + pieceBonusMoves)
            return false;

        if (pieceMoves < pieceBonusMoves)
            return true;

        return calculateGlobalMovesUsed(records, currentTurnCount) < MAX_MOVE_COUNT_PER_TURN + player.getStatusContainer().getExtraMoveCount(currentTurnCount);
    }

    public static bool canSummon(PlayerData player, int currentTurnCount)
    {
        return player.getStatusContainer().canSummon(currentTurnCount);
    }

    // ★ 修正：加入 BoardState 參數，徹底切斷對真實 BoardManager 的依賴
    public static void getLegalMoves(BoardState state, PlayerData player, ChessPiece piece, int currentTurnCount, List<MoveAction> bufferList)
    {
        bufferList.Clear();

        if (!canPieceMoveThisTurn(player, piece, currentTurnCount) || piece.isWhite() != player.IsWhite())
            return;

        int finalMoveCost = piece.getMoveCost(currentTurnCount) + player.getStatusContainer().getGlobalExtraMoveCost(currentTurnCount);

        if (player.getMana() < finalMoveCost ||
            !player.getStatusContainer().canMoveChess(piece.getChessType(), currentTurnCount) ||
            !piece.canMove(currentTurnCount))
        {
            return;
        }

        rawMovesBuffer.Clear();
        // ★ 將 state 傳入 getAvailableMoves
        piece.getMovementType(currentTurnCount).getAvailableMoves(state, piece.getPosition(), piece.isWhite(), rawMovesBuffer);

        foreach (var move in rawMovesBuffer)
        {
            if (move.actionType == ActionType.Attack)
            {
                // ★ 改從傳入的虛擬 state 中拿取目標，而不是真實畫面
                ChessPiece target = state.getChessPieceAtPosition(move.targetPos);
                if (target != null &&
                    canAttackGlobally(player, currentTurnCount) &&
                    piece.canAttack(currentTurnCount) &&
                    target.canBeAttacked(currentTurnCount) &&
                    piece.canAttackSpecificTypeChess(target.getChessType(), currentTurnCount))
                {
                    bufferList.Add(move);
                }
            }
            else
            {
                bufferList.Add(move);
            }
        }
    }
}