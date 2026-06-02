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

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}
