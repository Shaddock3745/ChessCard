using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ConditionalSummonEffect : CardEffect
{
    private ChessType typeToSummon;
    private ChessType typeToCheck;
    private int thresholdCount;

    public ConditionalSummonEffect(ChessType typeToSummon, ChessType typeToCheck, int thresholdCount)
    {
        this.typeToSummon = typeToSummon;
        this.typeToCheck = typeToCheck;
        this.thresholdCount = thresholdCount;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0) 
            return;

        bool isWhite = isLocal == GameManager.IsLocalWhite();

        if (BoardManager.getAllChessPieces(isWhite).FindAll(p => p.getChessType() == typeToCheck).Count < thresholdCount)
            await BoardManager.summonChessPieceWithAnimation(typeToSummon, isWhite, targets.getTargetPositions()[0]);
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