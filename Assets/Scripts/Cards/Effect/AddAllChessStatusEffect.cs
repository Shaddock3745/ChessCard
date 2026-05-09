using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class AddAllChessStatusEffect : CardEffect
{
    private bool isSameTeam, hardConstraint;
    private int duration;
    private List<ChessType> targetTypes;
    private Func<int, int, ChessStatus> statusFactory;

    public AddAllChessStatusEffect(bool isSameTeam, int duration, Func<int, int, ChessStatus> statusFactory, bool hardConstraint, params ChessType[] targetTypes)
    {
        this.isSameTeam = isSameTeam;
        this.hardConstraint = hardConstraint;
        this.duration = duration;
        this.targetTypes = new List<ChessType>(targetTypes);
        this.statusFactory = statusFactory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        if (!hardConstraint)
            return true;
        else
            return BoardManager.getChessAmount(isSameTeam == (isLocal == GameManager.IsLocalWhite()), targetTypes) > 0;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int currentTurnCount = GameManager.getCurrentTurnCount();
        List<ChessPiece> playerChessList = BoardManager.getAllChessPieces(isSameTeam == (isLocal == GameManager.IsLocalWhite()), targetTypes);

        foreach (var chess in playerChessList)
            chess.addStatus(statusFactory.Invoke(currentTurnCount, currentTurnCount + duration));

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
