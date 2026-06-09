using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SelectPromotablePawnEffect : CardEffect
{
    private bool isAlly;

    public SelectPromotablePawnEffect(bool isAlly)
    {
        this.isAlly = isAlly;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        bool isWhite = isAlly ? isLocal == GameManager.IsLocalWhite() : isLocal != GameManager.IsLocalWhite();
        return BoardManager.getTilesOfPromotablePawn(isWhite).Count > 0;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        bool targetIsWhite = isAlly ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        List<BoardTile> tiles = BoardManager.getTilesOfPromotablePawn(targetIsWhite);

        BoardTile targetTile = await InputManager.waitForBoardTileSelection(tiles);

        if (targetTile == null)
            return null;

        return new EffectTargetPayload(targetTile.getPosition());
    }

    // ★ 更新簽章，改由 BoardState 遍歷虛擬棋盤列舉可升變的士兵位置
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana < card.getCost() || !canActivate(isLocal, currentTurnCount, card))
            return options;

        bool targetIsWhite = isAlly ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        int promotionRow = targetIsWhite ? 7 : 0; // 白兵在第 7 列升變，黑兵在第 0 列升變

        // 遍歷虛擬棋盤尋找符合陣營且位於底線的兵
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && piece.getChessType() == ChessType.Pawn && piece.isWhite() == targetIsWhite)
                {
                    if (y == promotionRow)
                    {
                        // 產生 Option (基礎分數 0f，交由後續管線效果如移除與生成來決定權重)
                        options.Add(new CardUseOption(card,new EffectTargetPayload(new List<Vector2Int> { new Vector2Int(x, y) })));
                    }
                }
            }
        }

        return options;
    }

    // ★ DFS 虛擬執行：目標選擇節點本身不改變盤面狀態，留空即可
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        // Do nothing.
    }
}