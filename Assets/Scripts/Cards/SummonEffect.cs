using System.Collections.Generic;
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

    public List<CardUseOption> getUseOptions(Card card, int currentMana)
    {
        if (currentMana < card.getCost())
            return new List<CardUseOption>();

        ChessType type = card.getId() switch
        {
            1 => ChessType.Pawn,
            2 => ChessType.Bishop,
            3 => ChessType.Knight,
            4 => ChessType.Rook,
            5 => ChessType.Queen,
            _ => ChessType.Pawn
        };

        List<CardUseOption> useOptions = new List<CardUseOption>();
        List<BoardTile> summonableTiles = BoardManager.getSummonableTiles(type, AiManager.isWhite());
        float manaEfficiency = card.getCost() == 0 ? 2.0f : (float)currentMana / card.getCost();

        foreach (BoardTile tile in summonableTiles)
        {
            Vector2Int position = tile.getPos();
            float score = AiEvaluator.getPieceValue(type) * manaEfficiency + AiEvaluator.getPositionValue(position);
            useOptions.Add(new CardUseOption(card, score, new List<Vector2Int> { position }));
        }

        return useOptions;
    }

    public async Task executeEffect(bool isPlayer, Card card, List<Vector2Int> targetPositions)
    {
        bool isWhite = isPlayer ? GameManager.IsPlayerWhite() : !GameManager.IsPlayerWhite();

        if (isPlayer)
            processSummonWithoutAnimation(chessType, isWhite, targetPositions[0]);
        else
        {
            await BoardManager.showTargetWithAnimation(targetPositions);
            await processSummonWithAnimation(chessType, isWhite, targetPositions[0]);
            BoardManager.removeAllHint();
        }
    }
}
