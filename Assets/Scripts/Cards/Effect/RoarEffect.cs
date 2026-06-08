using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoarEffect : CardEffect
{
    private Func<EffectTargetPayload, Vector2Int> piecePositionFactory;
    private Vector2Int[] directions;

    public RoarEffect(Func<EffectTargetPayload, Vector2Int> piecePositionFactory, params Vector2Int[] directions)
    {
        this.piecePositionFactory = piecePositionFactory;
        this.directions = directions;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        List<Task> tasks = new List<Task>();
        Vector2Int roaringPiecePosition = piecePositionFactory.Invoke(targets);
        bool isRoarerWhite = BoardManager.isWhiteAtPosition(roaringPiecePosition);

        foreach (Vector2Int direction in directions)
            tasks.Add(BoardManager.pieceGotRoared(roaringPiecePosition + direction, direction, isRoarerWhite));

        await Task.WhenAll(tasks);
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章。作為 Pipeline 後端，不自行產生選項
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ DFS 虛擬執行實作：在虛擬盤面上模擬推開效果
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        Vector2Int roaringPiecePosition = piecePositionFactory.Invoke(targets);

        // 邊緣棋子或無效 payload 可能給出越界座標,查棋盤前先擋掉(否則 AI 推演會 IndexOutOfRange)
        if (!clonedState.isPositionOnBoard(roaringPiecePosition)) return;

        ChessPiece roaringPiece = clonedState.getChessPieceAtPosition(roaringPiecePosition);

        if (roaringPiece == null) return;

        bool isRoarerWhite = roaringPiece.isWhite();

        foreach (Vector2Int direction in directions)
        {
            Vector2Int targetPos = roaringPiecePosition + direction;

            // targetPos 可能落在棋盤外,先檢查再查詢
            if (!clonedState.isPositionOnBoard(targetPos)) continue;

            // 檢查範圍內是否有敵方棋子
            ChessPiece affectedPiece = clonedState.getChessPieceAtPosition(targetPos);
            if (affectedPiece != null && affectedPiece.isWhite() != isRoarerWhite)
            {
                // 推往該方向的下一格
                Vector2Int pushedPos = targetPos + direction;

                // 若終點在棋盤內且為空，則發生推開位移
                if (clonedState.isPositionOnBoard(pushedPos) && clonedState.getChessPieceAtPosition(pushedPos) == null)
                {
                    clonedState.moveChessPiece(targetPos, pushedPos);
                }
            }
        }
    }
}