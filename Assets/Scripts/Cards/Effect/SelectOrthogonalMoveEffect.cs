using System.Collections.Generic;
using System.Threading.Tasks;
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

        if (validPieces.Count == 0)
            return EffectTargetPayload.empty();

        BoardTile seletecTile = await InputManager.waitForBoardTileSelection(validPieces.ConvertAll(p => BoardManager.getTileAtPosition(p.getPosition())));

        if (seletecTile == null)
            return null;

        Vector2Int selectedPiecePos = seletecTile.getPosition();

        List<Vector2Int> validMoves = getValidOrthogonalMoves(selectedPiecePos);
        List<BoardTile> validTiles = new List<BoardTile>();

        foreach (var posiotn in validMoves)
            validTiles.Add(BoardManager.getTileAtPosition(posiotn));

        BoardTile targetTile = await InputManager.waitForBoardTileSelection(validTiles);

        if (targetTile == null)
            return null;

        return new EffectTargetPayload(new List<Vector2Int> { selectedPiecePos, targetTile.getPosition() });
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    // ★ 實作 getUseOptions：列舉所有起點與十字終點的組合選項
    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana < card.getCost() || !canActivate(isLocal, currentTurnCount, card))
            return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        List<ChessPiece> validPieces = BoardManager.getAllChessPieces(isWhite).FindAll(p => targetTypes.Contains(p.getChessType()));

        foreach (ChessPiece piece in validPieces)
        {
            Vector2Int startPos = piece.getPosition();
            List<Vector2Int> validMoves = getValidOrthogonalMoves(startPos);

            foreach (Vector2Int targetPos in validMoves)
            {
                List<Vector2Int> path = new List<Vector2Int> { startPos, targetPos };
                options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(path)));
            }
        }

        return options;
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