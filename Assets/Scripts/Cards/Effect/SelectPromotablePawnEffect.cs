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

    // ★ 實作 getUseOptions：收集所有可升變棋子的位置並產生選項
    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana < card.getCost() || !canActivate(isLocal, currentTurnCount, card))
            return options;

        bool targetIsWhite = isAlly ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        List<BoardTile> tiles = BoardManager.getTilesOfPromotablePawn(targetIsWhite);

        foreach (BoardTile tile in tiles)
        {
            options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(new List<Vector2Int> { tile.getPosition() })));
        }

        return options;
    }
}