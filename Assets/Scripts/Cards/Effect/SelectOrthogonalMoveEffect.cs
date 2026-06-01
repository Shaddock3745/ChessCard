using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class SelectOrthogonalMoveEffect : CardEffect
{
    private List<ChessType> targetTypes;

    public SelectOrthogonalMoveEffect(params ChessType[] targetType)
    {
        targetTypes = new List<ChessType>(targetType);
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        bool isWhite = isLocal == GameManager.IsLocalWhite();
        List<ChessPiece> validPieces = BoardManager.getAllChessPieces(isWhite).FindAll(p => targetTypes.Contains(p.getChessType()));

        foreach (ChessPiece piece in validPieces)
            if (getValidOrthogonalMoves(piece.getPosition()).Count > 0)
                return true;

        return false;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        bool isWhite = GameManager.IsLocalWhite();

        List<ChessPiece> validPieces = BoardManager.getAllChessPieces(isWhite).FindAll(
            p => targetTypes.Contains(p.getChessType()) && getValidOrthogonalMoves(p.getPosition()).Count > 0
        );

        if (validPieces.Count == 0) return EffectTargetPayload.empty();

        Vector2Int selectedPiecePos = (await InputManager.waitForBoardTileSelection(validPieces.ConvertAll(p => BoardManager.getTileAtPosition(p.getPosition())))).getPosition();

        List<Vector2Int> validMoves = getValidOrthogonalMoves(selectedPiecePos);
        List<BoardTile> validTiles = new List<BoardTile>();

        foreach (var posiotn in validMoves)
            validTiles.Add(BoardManager.getTileAtPosition(posiotn));

        Vector2Int targetPos = (await InputManager.waitForBoardTileSelection(validTiles)).getPosition();

        return new EffectTargetPayload(new List<Vector2Int> { selectedPiecePos, targetPos });
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }


    private List<Vector2Int> getValidOrthogonalMoves(Vector2Int startPos)
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int target = startPos + dir;
            if (BoardManager.isPositionOnBoard(target) && BoardManager.isEmpty(target))
                validMoves.Add(target);
        }

        return validMoves;
    }
}