using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SelectReturnableChessEffect : CardEffect
{
    private bool isAlly;
    private ChessType[] targetTypes;

    public SelectReturnableChessEffect(bool isAlly, params ChessType[] targetTypes)
    {
        this.isAlly = isAlly;
        this.targetTypes = targetTypes;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        List<ChessPiece> validPieces = BoardManager.getAllChessPieces(isAlly ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite());

        foreach (ChessPiece piece in validPieces)
        {
            if (System.Array.IndexOf(targetTypes, piece.getChessType()) != -1)
            {
                Vector2Int initPos = piece.getInitialPosition();

                if (BoardManager.isEmpty(initPos) || piece.getPosition() == initPos)
                    return true;
            }
        }
        return false;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        bool targetIsWhite = isAlly ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        List<ChessPiece> validPieces = BoardManager.getAllChessPieces(targetIsWhite).FindAll(p =>
            (targetTypes.Length == 0 || System.Array.IndexOf(targetTypes, p.getChessType()) != -1) &&
            (BoardManager.isEmpty(p.getInitialPosition()) || p.getPosition() == p.getInitialPosition())
        );

        if (validPieces.Count == 0)
            return EffectTargetPayload.empty();

        List<Vector2Int> positions = validPieces.ConvertAll(p => p.getPosition());
        List<BoardTile> validTiles = new List<BoardTile>();

        foreach (Vector2Int position in positions)
            validTiles.Add(BoardManager.getTileAtPosition(position));

        BoardTile selectedTile = await InputManager.waitForBoardTileSelection(validTiles);

        if (selectedTile == null)
            return null;

        return new EffectTargetPayload(new List<Vector2Int> { selectedTile.getPosition() });
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();
        if (currentMana < card.getCost()) return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        bool targetIsWhite = isAlly ? isWhite : !isWhite;

        List<ChessPiece> validPieces = BoardManager.getAllChessPieces(targetIsWhite).FindAll(p =>
            (targetTypes.Length == 0 || System.Array.IndexOf(targetTypes, p.getChessType()) != -1) &&
            (BoardManager.isEmpty(p.getInitialPosition()) || p.getPosition() == p.getInitialPosition())
        );

        foreach (ChessPiece piece in validPieces)
        {
            options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(new List<Vector2Int> { piece.getPosition() })));
        }

        return options;
    }
}