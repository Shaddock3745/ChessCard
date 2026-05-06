using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class RemoveChessEffect : CardEffect
{
    private bool isSameTeam;
    private List<ChessType> removeableChessTypes;

    public RemoveChessEffect(bool isSameTeam, params ChessType[] removeableChessTypes)
    {
        this.isSameTeam = isSameTeam;

        if (removeableChessTypes.Length == 0)
            throw new System.ArgumentException("至少要指定一種可移除的棋子類型");

        this.removeableChessTypes = new List<ChessType>(removeableChessTypes);
    }

    public bool canActivate(bool isLocal, Card card)
    {
        foreach (ChessType type in removeableChessTypes)
            if (BoardManager.getChessAmount(type, isSameTeam ? (isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite()) : (isLocal ? !GameManager.IsLocalWhite() : GameManager.IsLocalWhite())) > 0)
                return true;

        return false;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
        {
            Debug.LogError("移除棋子效果的目標位置錯誤");
            return;
        }

        if (isLocal)
            BoardManager.removePiece(targets.getTargetPositions()[0]);
        else
        {
            await BoardManager.showTargetWithAnimation(targets.getTargetPositions());
            await BoardManager.removePieceWithAnimation(targets.getTargetPositions()[0]);
            BoardManager.removeAllHint();
        }

        InputManager.tryDetectHover();
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        List<BoardTile> validTiles = new List<BoardTile>();

        foreach (ChessType type in removeableChessTypes)
            validTiles.AddRange(BoardManager.getTilesOfChesses(type, isSameTeam ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite()));

        BoardTile tile = await InputManager.waitForBoardTileSelection(validTiles);

        if (tile == null)
            return null;

        List<Vector2Int> targetPositions = new List<Vector2Int>
        {
            tile.getPos()
        };
        return new EffectTargetPayload(targetPositions);
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        List<CardUseOption> options = new List<CardUseOption>();
        List<BoardTile> validTiles = new List<BoardTile>();

        foreach (ChessType type in removeableChessTypes)
            validTiles.AddRange(BoardManager.getTilesOfChesses(type, isSameTeam ? (isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite()) : (isLocal ? !GameManager.IsLocalWhite() : GameManager.IsLocalWhite())));

        foreach (BoardTile tile in validTiles)
        {
            if (tile == null)
                continue;

            List<Vector2Int> targetPositions = new List<Vector2Int>
            {
                tile.getPos()
            };

            options.Add(new CardUseOption(card, AiEvaluator.getPieceValue(BoardManager.getChessPieceAtTile(tile).getChessType()), new EffectTargetPayload(targetPositions)));
        }

        return options;
    }
}
