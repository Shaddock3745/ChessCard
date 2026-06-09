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

                if (BoardManager.isEmpty(initPos) && piece.getPosition() != initPos)
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
            (BoardManager.isEmpty(p.getInitialPosition()) || p.getPosition() != p.getInitialPosition())
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

    // ★ 更新簽章，改由 BoardState 遍歷虛擬棋盤列舉可遣返的棋子
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();
        if (currentMana < card.getCost() || !canActivate(isLocal, currentTurnCount, card)) return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        bool targetIsWhite = isAlly ? isWhite : !isWhite;
        HashSet<ChessType> typeSet = new HashSet<ChessType>(targetTypes);

        // 遍歷虛擬棋盤
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && piece.isWhite() == targetIsWhite)
                {
                    // 檢查類型是否符合
                    if (targetTypes.Length == 0 || typeSet.Contains(piece.getChessType()))
                    {
                        Vector2Int initPos = piece.getInitialPosition();

                        // 在虛擬盤面上確認：初始位置為空，且該棋子目前不在初始位置
                        if (state.getChessPieceAtPosition(initPos) == null && piece.getPosition() != initPos)
                        {
                            // 產生 Option (基礎分數 0f，交由後續管線或 DFS 結算)
                            options.Add(new CardUseOption(card,new EffectTargetPayload(new List<Vector2Int> { piece.getPosition() })));
                        }
                    }
                }
            }
        }

        return options;
    }

    // ★ DFS 虛擬執行：目標選擇節點本身不改變盤面狀態，留空即可
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        // Do nothing.
    }
}