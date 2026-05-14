using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AddStatusToPayloadTargetChessEffect : CardEffect
{
    private Func<int, ChessStatus> factory;

    public AddStatusToPayloadTargetChessEffect(Func<int, ChessStatus> factory)
    {
        this.factory = factory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int startTurn = GameManager.getCurrentTurnCount();
        ChessPiece targetPiece = BoardManager.getChessPieceAtPosition(targets.getTargetPositions()[0]);

        if (targetPiece != null)
            targetPiece.addStatus(factory.Invoke(startTurn));

        return Task.CompletedTask;
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
