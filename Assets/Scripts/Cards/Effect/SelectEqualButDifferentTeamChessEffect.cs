using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SelectEqualButDifferentTeamChessEffect : CardEffect
{
    private List<ChessType> validTypes;

    public SelectEqualButDifferentTeamChessEffect(params ChessType[] chessTypes)
    {
        this.validTypes = new List<ChessType>(chessTypes);
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        foreach (ChessType chessType in validTypes)
            if (BoardManager.getChessAmount(true, chessType) > 0 && BoardManager.getChessAmount(false, chessType) > 0)
                return true;

        return false;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        List<BoardTile> tiles = new List<BoardTile>();

        foreach (ChessType chessType in validTypes)
            if (BoardManager.getChessAmount(true, chessType) > 0 && BoardManager.getChessAmount(false, chessType) > 0)
                tiles.AddRange(BoardManager.getTilesOfChesses(chessType, GameManager.IsLocalWhite()));

        BoardTile firstTile = await InputManager.waitForBoardTileSelection(tiles);

        if (firstTile == null)
            return null;

        Vector2Int firstPosition = firstTile.getPosition();
        ChessType selectableType = BoardManager.getChessPieceAtPosition(firstPosition).getChessType();

        tiles.Clear();
        tiles.AddRange(BoardManager.getTilesOfChesses(selectableType, !GameManager.IsLocalWhite()));

        BoardTile secondTile = await InputManager.waitForBoardTileSelection(tiles);

        if (secondTile == null)
            return null;

        return new EffectTargetPayload(firstPosition, secondTile.getPosition());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana < card.getCost() || !canActivate(isLocal, currentTurnCount, card))
            return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();

        foreach (ChessType chessType in validTypes)
        {
            List<BoardTile> allyTiles = BoardManager.getTilesOfChesses(chessType, isWhite);
            List<BoardTile> enemyTiles = BoardManager.getTilesOfChesses(chessType, !isWhite);

            // 確保雙方都存在該棋子才進行配對
            if (allyTiles.Count > 0 && enemyTiles.Count > 0)
            {
                foreach (BoardTile allyTile in allyTiles)
                {
                    foreach (BoardTile enemyTile in enemyTiles)
                    {
                        List<Vector2Int> positions = new List<Vector2Int> { allyTile.getPosition(), enemyTile.getPosition() };
                        options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(positions)));
                    }
                }
            }
        }

        return options;
    }
}
