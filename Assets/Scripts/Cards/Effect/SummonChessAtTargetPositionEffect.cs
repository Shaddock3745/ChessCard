using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SummonChessAtTargetPositionEffect : CardEffect
{
    private static async Task processSummonWithAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        await BoardManager.summonChessPieceWithAnimation(type, isWhite, position);
    }

    private ChessType chessType;

    public SummonChessAtTargetPositionEffect(ChessType chessType)
    {
        this.chessType = chessType;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        return !BoardManager.chessAmountReachLimit(chessType, isWhite);
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
            return;

        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        Vector2Int pos = targets.getTargetPositions()[0];

        if (isLocal)
            await processSummonWithAnimation(chessType, isWhite, pos);
        else
        {
            await BoardManager.showTargetWithAnimation(targets.getTargetPositions());
            await processSummonWithAnimation(chessType, isWhite, pos);
            BoardManager.removeAllHint();
        }

        InputManager.tryDetectHover();
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章。作為 Pipeline 後端效果，不自行產生選項
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ DFS 虛擬執行實作：在虛擬盤面的目標位置上直接放入新棋子
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions() == null || targets.getTargetPositions().Count == 0)
            return;

        Vector2Int pos = targets.getTargetPositions()[0];
        bool isWhite = clonedAlly.IsWhite();

        // 直接在虛擬盤面上放置新棋子，後續交由 DFS 結算該棋子進場後的盤面戰略價值
        clonedState.setChessPieceAtPosition(pos, new ChessPiece(chessType, isWhite, pos, true));
    }
}