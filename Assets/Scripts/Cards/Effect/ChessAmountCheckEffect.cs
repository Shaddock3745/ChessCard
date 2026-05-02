using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAmountCheckEffect : CardEffect
{
    ChessType chessType;
    int amount;
    bool checkLess, allowEqual;

    public ChessAmountCheckEffect(ChessType type, int amount, bool checkLess, bool allowEqual)
    {
        chessType = type;
        this.amount = amount;
        this.checkLess = checkLess;
        this.allowEqual = allowEqual;
    }

    public bool canActivate(bool isLocal, Card card)
    {
        if (card != null)
        {
            int count = BoardManager.getChessAmount(chessType, isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite());

            if (allowEqual)
                if (checkLess)
                    return count <= amount;
                else
                    return count >= amount;
            else
                if (checkLess)
                    return count < amount;
                else
                    return count > amount;
        }

        return false;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana >= card.getCost())
            options.Add(new CardUseOption(card, 1));

        return options;
    }
}
