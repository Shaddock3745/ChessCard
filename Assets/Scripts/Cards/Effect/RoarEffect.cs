using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoarEffect : CardEffect
{
    private Func<EffectTargetPayload, Vector2Int> piecePositionFactory;
    private Vector2Int[] directions;

    public RoarEffect(Func<EffectTargetPayload, Vector2Int> piecePositionFactory, params Vector2Int[] directions)
    {
        this.piecePositionFactory = piecePositionFactory;
        this.directions = directions;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        List<Task> tasks = new List<Task>();
        Vector2Int roaringPiecePosition = piecePositionFactory.Invoke(targets);

        foreach (Vector2Int direction in directions)
            tasks.Add(BoardManager.pieceGotRoared(roaringPiecePosition + direction, direction, BoardManager.isWhiteAtPosition(roaringPiecePosition)));

        await Task.WhenAll(tasks);
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
