using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class AddStatusToAllMatchingTypeChessEffect : CardEffect
{
    private bool isAlly, hardConstraint;
    private List<ChessType> targetTypes;
    private Func<int, IEnumerable<ChessStatus>> statusFactory;

    public AddStatusToAllMatchingTypeChessEffect(bool isAlly, Func<int, IEnumerable<ChessStatus>> statusFactory, bool hardConstraint, params ChessType[] targetTypes)
    {
        this.isAlly = isAlly;
        this.hardConstraint = hardConstraint;
        this.targetTypes = new List<ChessType>(targetTypes);
        this.statusFactory = statusFactory;
    }

    public AddStatusToAllMatchingTypeChessEffect(bool isAlly, int duration, Func<int, int, ChessStatus> statusFactory, bool hardConstraint, params ChessType[] targetTypes)
        : this(isAlly, (startTurn) => new ChessStatus[] { statusFactory.Invoke(startTurn, startTurn + duration) }, hardConstraint, targetTypes)
    {

    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        if (!hardConstraint)
            return true;
        else
            return BoardManager.getChessAmount(isAlly == (isLocal == GameManager.IsLocalWhite()), targetTypes) > 0;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int currentTurnCount = GameManager.getCurrentTurnCount();
        List<ChessPiece> playerChessList = BoardManager.getAllChessPieces(isAlly == (isLocal == GameManager.IsLocalWhite()), targetTypes);

        foreach (var chess in playerChessList)
            foreach (var satus in statusFactory.Invoke(currentTurnCount))
                chess.addStatus(satus);

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
