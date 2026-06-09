using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SummonEffect : CardEffect
{
    private static void processSummonWithoutAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        BoardManager.summonChessPiece(type, isWhite, position);
    }

    private static async Task processSummonWithAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        await BoardManager.summonChessPieceWithAnimation(type, isWhite, position);
    }

    private ChessType chessType;

    public SummonEffect(ChessType chessType)
    {
        this.chessType = chessType;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return card != null && MatchDataManager.canSummonThisTurn(isLocal, currentTurnCount) && !BoardManager.chessAmountReachLimit(chessType, isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite())
            && BoardManager.getSummonableTiles(chessType, isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite()).Count > 0;
    }

    // ★ 更新簽章，改用 BoardState 篩選虛擬空位，並將分數改為 0f
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        if (!canActivate(isLocal, currentTurnCount, card))
            return new List<CardUseOption>();

        List<CardUseOption> useOptions = new List<CardUseOption>();
        bool isWhite = isLocal == GameManager.IsLocalWhite();

        // 取得基礎合法召喚區域
        List<BoardTile> summonableTiles = BoardManager.getSummonableTiles(chessType, isWhite);

        foreach (BoardTile tile in summonableTiles)
        {
            Vector2Int position = tile.getPosition();

            // 確保該位置在當前的虛擬盤面上是空的（防止先前的虛擬效果已佔用此格子）
            if (state.getChessPieceAtPosition(position) == null)
            {
                // 分數給予 0f，交由 DFS 模擬放子後進行全局評估
                useOptions.Add(new CardUseOption(card,new EffectTargetPayload(position)));
            }
        }

        return useOptions;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();

        if (isLocal)
            await processSummonWithAnimation(chessType, isWhite, targets.getTargetPositions()[0]);
        else
        {
            await BoardManager.showTargetWithAnimation(targets.getTargetPositions());
            await processSummonWithAnimation(chessType, isWhite, targets.getTargetPositions()[0]);
            BoardManager.removeAllHint();
        }

        InputManager.tryDetectHover();
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        BoardTile tile = await InputManager.waitForBoardTileSelection(BoardManager.getSummonableTiles(chessType, GameManager.IsLocalWhite()));

        if (tile == null)
            return null;

        return new EffectTargetPayload(tile.getPosition());
    }

    // ★ DFS 虛擬執行實作：在虛擬盤面的目標位置上直接生成新棋子
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions() == null || targets.getTargetPositions().Count == 0)
            return;

        Vector2Int pos = targets.getTargetPositions()[0];
        bool isWhite = clonedAlly.IsWhite();

        // 在虛擬盤面上直接放置新實體化的棋子
        clonedState.setChessPieceAtPosition(pos, new ChessPiece(chessType, isWhite, pos, true));
    }
}