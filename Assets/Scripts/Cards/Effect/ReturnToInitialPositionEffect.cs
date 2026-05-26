using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ReturnToInitialPositionEffect : CardEffect
{
    public bool canActivate(bool isLocal, int currentTurnCount, Card card) => true;

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
            return;

        Vector2Int currentPosition = targets.getTargetPositions()[0];
        ChessPiece piece = BoardManager.getChessPieceAtPosition(currentPosition);

        if (piece != null)
        {
            Vector2Int initPosition = piece.getInitialPosition();

            if (currentPosition != initPosition)
            {
                await BoardManager.movePieceWithAnimation(currentPosition, initPosition);
                targets.getTargetPositions()[0] = initPosition;
            }
        }
    }

    public Task<EffectTargetPayload> gatherTargets() => Task.FromResult(EffectTargetPayload.empty());

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}