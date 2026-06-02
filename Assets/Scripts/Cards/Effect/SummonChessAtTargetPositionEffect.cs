using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SummonChessAtTargetPositionEffect : CardEffect
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

    public SummonChessAtTargetPositionEffect(ChessType chessType)
    {
        this.chessType = chessType;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        return !BoardManager.chessAmountReachLimit(chessType, isWhite);
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
            return;

        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        Vector2Int pos = targets.getTargetPositions()[0];

        if (isLocal)
            await processSummonWithAnimation(chessType, isWhite, pos);
        else
        {
            await BoardManager.showTargetWithAnimation(targets.getTargetPositions());
            await processSummonWithAnimation(chessType, isWhite, pos);
            BoardManager.removeAllHint();
        }

        InputManager.tryDetectHover();
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}
