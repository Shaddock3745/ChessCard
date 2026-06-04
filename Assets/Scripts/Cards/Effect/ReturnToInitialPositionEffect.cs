using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReturnToInitialPositionEffect : CardEffect
{
    public bool canActivate(bool isLocal, int currentTurnCount, Card card) => true;

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
            return;

        Vector2Int currentPosition = targets.getTargetPositions()[0];
        ChessPiece piece = BoardManager.getChessPieceAtPosition(currentPosition);

        if (piece != null)
        {
            Vector2Int initPosition = piece.getInitialPosition();

            if (currentPosition != initPosition)
            {
                await BoardManager.movePieceWithAnimation(currentPosition, initPosition);
                targets.getTargetPositions()[0] = initPosition;
            }
        }
    }

    public Task<EffectTargetPayload> gatherTargets() => Task.FromResult(EffectTargetPayload.empty());

    // ★ 更新簽章。作為 Pipeline 後端，不自行產生選項
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ DFS 虛擬執行實作：在虛擬盤面上將棋子移回初始位置
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions() == null || targets.getTargetPositions().Count == 0)
            return;

        Vector2Int currentPosition = targets.getTargetPositions()[0];
        ChessPiece virtualPiece = clonedState.getChessPieceAtPosition(currentPosition);

        if (virtualPiece != null)
        {
            Vector2Int initPosition = virtualPiece.getInitialPosition();

            if (currentPosition != initPosition)
            {
                // 呼叫虛擬盤面的移動方法
                clonedState.moveChessPiece(currentPosition, initPosition);

                // 仿照真實執行，將 payload 中的座標更新為新的位置，以供後續複合管線效果使用
                targets.getTargetPositions()[0] = initPosition;
            }
        }
    }
}