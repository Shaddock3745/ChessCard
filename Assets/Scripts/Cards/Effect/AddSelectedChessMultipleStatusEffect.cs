using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AddSelectedChessMultipleStatusEffect : CardEffect
{
    private bool isSameTeam;
    private List<ChessType> targetTypes;
    private Func<int, IEnumerable<ChessStatus>> statusFactory;

    public AddSelectedChessMultipleStatusEffect(bool isSameTeam, Func<int, IEnumerable<ChessStatus>> statusFactory, params ChessType[] targetTypes)
    {
        this.isSameTeam = isSameTeam;
        this.statusFactory = statusFactory;
        this.targetTypes = new List<ChessType>(targetTypes);
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return BoardManager.getChessAmount(isSameTeam ? isLocal == GameManager.IsLocalWhite() : isLocal != GameManager.IsLocalWhite(), targetTypes) > 0;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        ChessPiece targetPiece = BoardManager.getChessPieceAtPosition(targets.getTargetPositions()[0]);

        if (targetPiece != null)
        {
            IEnumerable<ChessStatus> targetStatus = statusFactory.Invoke(GameManager.getCurrentTurnCount());

            foreach (ChessStatus status in targetStatus)
                targetPiece.addStatus(status);
        }

        return Task.CompletedTask;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        List<BoardTile> vaildTiles = new List<BoardTile>();

        foreach (ChessType chessType in targetTypes)
            foreach (BoardTile tile in BoardManager.getTilesOfChesses(chessType, GameManager.IsLocalWhite() && isSameTeam))
                vaildTiles.Add(tile);

        if (vaildTiles.Count == 0)
            return null;

        BoardTile selectTile = await InputManager.waitForBoardTileSelection(vaildTiles);

        if (selectTile == null)
            return null;

        return new EffectTargetPayload(selectTile.getPos());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}
