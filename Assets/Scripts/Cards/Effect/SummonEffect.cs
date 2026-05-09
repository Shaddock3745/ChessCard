using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class SummonEffect : CardEffect
{
    private static void processSummonWithoutAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        BoardManager.summonChessPiece(type, isWhite, position);
    }

    private static async Task processSummonWithAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        await BoardManager.summonChessPieceWithAnimation(type, isWhite, position);
    }

    private ChessType chessType;

    public SummonEffect(ChessType chessType)
    {
        this.chessType = chessType;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return card != null && MatchDataManager.canSummonThisTurn(isLocal, currentTurnCount) && !BoardManager.chessAmountReachLimit(chessType, isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite())
            && BoardManager.getSummonableTiles(chessType, isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite()).Count > 0;
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        if (currentMana < card.getCost() || !canActivate(isLocal, currentTurnCount, card))
            return new List<CardUseOption>();

        List<CardUseOption> useOptions = new List<CardUseOption>();
        List<BoardTile> summonableTiles = BoardManager.getSummonableTiles(chessType, isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite());
        float manaEfficiency = card.getCost() == 0 ? 2.0f : (float)currentMana / card.getCost();

        foreach (BoardTile tile in summonableTiles)
        {
            Vector2Int position = tile.getPos();
            float score = AiEvaluator.getPieceValue(chessType) * manaEfficiency + AiEvaluator.getPositionValue(position);
            useOptions.Add(new CardUseOption(card, score, new EffectTargetPayload(position)));
        }

        return useOptions;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();

        if (isLocal)
            processSummonWithoutAnimation(chessType, isWhite, targets.getTargetPositions()[0]);
        else
        {
            await BoardManager.showTargetWithAnimation(targets.getTargetPositions());
            await processSummonWithAnimation(chessType, isWhite, targets.getTargetPositions()[0]);
            BoardManager.removeAllHint();
        }

        InputManager.tryDetectHover();
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        BoardTile tile = await InputManager.waitForBoardTileSelection(BoardManager.getSummonableTiles(chessType, GameManager.IsLocalWhite()));

        if (tile == null)
            return null;

        return new EffectTargetPayload(tile.getPos());
    }
}
