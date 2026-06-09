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
        bool isWhite = isLocal == GameManager.IsLocalWhite();
        bool targetIsWhite = isSameTeam ? isWhite : !isWhite;
        return BoardManager.getChessAmount(targetIsWhite, targetTypes) > 0;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        ChessPiece targetPiece = BoardManager.getChessPieceAtPosition(targets.getTargetPositions()[0]);

        if (targetPiece != null)
        {
            IEnumerable<ChessStatus> targetStatus = statusFactory.Invoke(GameManager.getCurrentTurnCount());

            foreach (ChessStatus status in targetStatus)
                BoardManager.addStatus(targetPiece, status);
        }

        return Task.CompletedTask;
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        List<BoardTile> vaildTiles = new List<BoardTile>();
        bool targetIsWhite = isSameTeam ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();

        foreach (ChessType chessType in targetTypes)
            foreach (BoardTile tile in BoardManager.getTilesOfChesses(chessType, targetIsWhite))
                vaildTiles.Add(tile);

        if (vaildTiles.Count == 0)
            return null;

        BoardTile selectTile = await InputManager.waitForBoardTileSelection(vaildTiles);

        if (selectTile == null)
            return null;

        return new EffectTargetPayload(selectTile.getPosition());
    }

    // ★ 更新簽章，並完全改用傳入的 state 來搜查虛擬盤面
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (!canActivate(isLocal, currentTurnCount, card))
            return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        bool targetIsWhite = isSameTeam ? isWhite : !isWhite;
        HashSet<ChessType> typeSet = new HashSet<ChessType>(targetTypes);

        // 遍歷虛擬盤面，找出所有符合條件的目標棋子
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);

                if (piece != null && piece.isWhite() == targetIsWhite && typeSet.Contains(piece.getChessType()))
                {
                    options.Add(new CardUseOption(card,new EffectTargetPayload(new Vector2Int(x, y))));
                }
            }
        }

        return options;
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions() == null || targets.getTargetPositions().Count == 0)
            return;

        Vector2Int targetPos = targets.getTargetPositions()[0];

        // 透過座標從虛擬盤面上抓出對應的虛擬棋子
        ChessPiece virtualPiece = clonedState.getChessPieceAtPosition(targetPos);

        if (virtualPiece != null)
        {
            IEnumerable<ChessStatus> targetStatuses = statusFactory.Invoke(currentTurnCount);

            foreach (ChessStatus status in targetStatuses)
            {
                virtualPiece.addStatus(status);
            }
        }
    }
}