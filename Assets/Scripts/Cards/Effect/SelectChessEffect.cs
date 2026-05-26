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
        return BoardManager.getChessAmount(isAlly ? isLocal == GameManager.IsLocalWhite() : isLocal != GameManager.IsLocalWhite(), validTypes) > 0;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        List<BoardTile> tiles = new List<BoardTile>();

        foreach (ChessType type in validTypes)
            tiles.AddRange(BoardManager.getTilesOfChesses(type, GameManager.IsLocalWhite()));

        BoardTile targetTile = await InputManager.waitForBoardTileSelection(tiles);

        return new EffectTargetPayload(targetTile.getPosition());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}
