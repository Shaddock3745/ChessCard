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

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (!canActivate(isLocal, currentTurnCount, card))
            return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        bool targetIsWhite = isAlly ? isWhite : !isWhite;

        // 將所有符合條件的棋子轉化為 Option (基礎分數 0)
        foreach (ChessType type in validTypes)
        {
            List<BoardTile> tiles = BoardManager.getTilesOfChesses(type, targetIsWhite);
            foreach (BoardTile tile in tiles)
            {
                options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(tile.getPosition())));
            }
        }

        return options;
    }
}