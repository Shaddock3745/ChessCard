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

    // ★ 更新簽章，改由 BoardState 遍歷虛擬棋盤列舉十字移動選項
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana < card.getCost() || !canActivate(isLocal, currentTurnCount, card))
            return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        HashSet<ChessType> typeSet = new HashSet<ChessType>(targetTypes);

        // 遍歷虛擬棋盤尋找符合條件的起點棋子
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && piece.isWhite() == isWhite && typeSet.Contains(piece.getChessType()))
                {
                    Vector2Int startPos = new Vector2Int(x, y);
                    List<Vector2Int> validMoves = getValidOrthogonalMoves(state, startPos);

                    // 為每個合法的十字空位終點產生 Option (基礎分數 0f，交由後續管線或 DFS 結算)
                    foreach (Vector2Int targetPos in validMoves)
                    {
                        List<Vector2Int> path = new List<Vector2Int> { startPos, targetPos };
                        options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(path)));
                    }
                }
            }
        }

        return options;
    }

    // 虛擬環境專用的十字空位檢查
    private List<Vector2Int> getValidOrthogonalMoves(BoardState state, Vector2Int startPos)
    {
        List<Vector2Int> validMoves = new List<Vector2Int>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int target = startPos + dir;

            // 進行虛擬棋盤邊界檢查與空位檢查
            if (target.x >= 0 && target.x < 8 && target.y >= 0 && target.y < 8)
            {
                if (state.getChessPieceAtPosition(target) == null)
                {
                    validMoves.Add(target);
                }
            }
        }

        return validMoves;
    }

    // 真實環境使用的十字空位檢查 (維持原樣)
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

    // ★ DFS 虛擬執行：目標選擇節點本身不改變盤面狀態，留空即可
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        // Do nothing.
    }
}