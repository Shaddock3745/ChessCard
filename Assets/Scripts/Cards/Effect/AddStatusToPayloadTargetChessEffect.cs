using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AddStatusToPayloadTargetChessEffect : CardEffect
{
    private Func<int, ChessStatus> factory;

    public AddStatusToPayloadTargetChessEffect(Func<int, ChessStatus> factory)
    {
        this.factory = factory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int startTurn = GameManager.getCurrentTurnCount();
        ChessPiece targetPiece = BoardManager.getChessPieceAtPosition(targets.getTargetPositions()[0]);

        if (targetPiece != null)
            BoardManager.addStatus(targetPiece, factory.Invoke(startTurn));

        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章。作為管線後端元件，不由它自己產生選項，交由複合卡片組合
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ DFS 虛擬執行實作：直接將狀態掛載到虛擬盤面中 payload 指定的棋子身上
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions() == null || targets.getTargetPositions().Count == 0)
            return;

        Vector2Int targetPos = targets.getTargetPositions()[0];

        // 透過座標從虛擬盤面上抓出對應的虛擬棋子
        ChessPiece virtualPiece = clonedState.getChessPieceAtPosition(targetPos);

        if (virtualPiece != null)
        {
            virtualPiece.addStatus(factory.Invoke(currentTurnCount));
        }
    }
}