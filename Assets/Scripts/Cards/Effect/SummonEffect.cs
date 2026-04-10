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

    private static ChessType getChessTypeByCardId(int cardId) => cardId switch
    {
        1 => ChessType.Pawn,
        2 => ChessType.Bishop,
        3 => ChessType.Knight,
        4 => ChessType.Rook,
        5 => ChessType.Queen,
        _ => ChessType.Pawn
    };

    private ChessType chessType;

    public SummonEffect(ChessType chessType)
    {
        this.chessType = chessType;
    }

    public bool canActivate(bool isLocal, Card card) => card != null && BoardManager.getSummonableTiles(getChessTypeByCardId(card.getId()), isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite()).Count > 0;

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        if (currentMana < card.getCost() || !canActivate(isLocal, card))
            return new List<CardUseOption>();

        ChessType type = getChessTypeByCardId(card.getId());

        List<CardUseOption> useOptions = new List<CardUseOption>();
        List<BoardTile> summonableTiles = BoardManager.getSummonableTiles(type, isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite());
        float manaEfficiency = card.getCost() == 0 ? 2.0f : (float)currentMana / card.getCost();

        foreach (BoardTile tile in summonableTiles)
        {
            Vector2Int position = tile.getPos();
            float score = AiEvaluator.getPieceValue(type) * manaEfficiency + AiEvaluator.getPositionValue(position);
            useOptions.Add(new CardUseOption(card, score, new List<Vector2Int> { position }));
        }

        return useOptions;
    }

    public async Task executeEffect(bool isLocal, Card card, List<Vector2Int> targetPositions)
    {
        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();

        if (isLocal)
            processSummonWithoutAnimation(chessType, isWhite, targetPositions[0]);
        else
        {
            await BoardManager.showTargetWithAnimation(targetPositions);
            await processSummonWithAnimation(chessType, isWhite, targetPositions[0]);
            BoardManager.removeAllHint();
        }
    }
}
