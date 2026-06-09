using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SelectChessEffect : CardEffect
{
    private bool isAlly;
    private List<ChessType> validTypes;

    public SelectChessEffect(bool isAlly, params ChessType[] validTypes)
    {
        this.isAlly = isAlly;
        this.validTypes = new List<ChessType>(validTypes);
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        bool isWhite = isLocal == GameManager.IsLocalWhite();
        return BoardManager.getChessAmount(isAlly ? isWhite : !isWhite, validTypes) > 0;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        bool isWhite = GameManager.IsLocalWhite();
        List<BoardTile> tiles = new List<BoardTile>();

        foreach (ChessType type in validTypes)
            tiles.AddRange(BoardManager.getTilesOfChesses(type, isAlly ? isWhite : !isWhite));

        BoardTile targetTile = await InputManager.waitForBoardTileSelection(tiles);

        if (targetTile == null)
            return null;

        return new EffectTargetPayload(targetTile.getPosition());
    }

    // ★ 更新簽章，並完全改用 BoardState 遍歷棋子
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        bool targetIsWhite = isAlly ? isWhite : !isWhite;
        HashSet<ChessType> typeSet = new HashSet<ChessType>(validTypes);

        // 在虛擬盤面上遍歷找棋子
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && piece.isWhite() == targetIsWhite && typeSet.Contains(piece.getChessType()))
                {
                    // 這裡給 0f，由後續的複合效果 (Pipeline) 或卡片本身決定該位置的戰略價值
                    options.Add(new CardUseOption(card,new EffectTargetPayload(new Vector2Int(x, y))));
                }
            }
        }

        return options;
    }

    // ★ DFS 虛擬執行：選棋效果本質上是對後續操作的「指定」，本身不改變棋盤，留空即可
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        // Do nothing.
    }
}