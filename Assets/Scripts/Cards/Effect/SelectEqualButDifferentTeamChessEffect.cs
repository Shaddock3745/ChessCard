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
        // 真實遊戲發動前的防呆檢查，維持使用 BoardManager 沒問題
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

        return new EffectTargetPayload(new List<Vector2Int> { firstPosition, secondTile.getPosition() });
    }

    // ★ 更新簽章，改用 BoardState 來遍歷虛擬棋盤找尋可配對的棋子
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        HashSet<ChessType> typeSet = new HashSet<ChessType>(validTypes);

        Dictionary<ChessType, List<Vector2Int>> allyPieces = new Dictionary<ChessType, List<Vector2Int>>();
        Dictionary<ChessType, List<Vector2Int>> enemyPieces = new Dictionary<ChessType, List<Vector2Int>>();

        foreach (ChessType type in validTypes)
        {
            allyPieces[type] = new List<Vector2Int>();
            enemyPieces[type] = new List<Vector2Int>();
        }

        // 1. 掃描虛擬棋盤，將棋子依據類型與陣營分組
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && typeSet.Contains(piece.getChessType()))
                {
                    if (piece.isWhite() == isWhite)
                        allyPieces[piece.getChessType()].Add(new Vector2Int(x, y));
                    else
                        enemyPieces[piece.getChessType()].Add(new Vector2Int(x, y));
                }
            }
        }

        // 2. 針對每種符合的類型，將我方與敵方的位置進行交叉配對產生所有合法選項
        foreach (ChessType type in validTypes)
        {
            List<Vector2Int> allies = allyPieces[type];
            List<Vector2Int> enemies = enemyPieces[type];

            if (allies.Count > 0 && enemies.Count > 0)
            {
                foreach (Vector2Int allyPos in allies)
                {
                    foreach (Vector2Int enemyPos in enemies)
                    {
                        List<Vector2Int> positions = new List<Vector2Int> { allyPos, enemyPos };
                        options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(positions)));
                    }
                }
            }
        }

        return options;
    }

    // ★ DFS 虛擬執行：目標選擇節點本身不改變棋盤，留空即可
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        // Do nothing.
    }
}