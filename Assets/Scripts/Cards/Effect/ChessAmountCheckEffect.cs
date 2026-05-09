using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAmountCheckEffect : CardEffect
{
    List<ChessType> chessTypes;
    int amount;
    bool checkAlly, checkLess, allowEqual;

    public ChessAmountCheckEffect(bool checkAlly, int amount, bool checkLess, bool allowEqual, params ChessType[] type)
    {
        chessTypes = new List<ChessType>(type);
        this.checkAlly = checkAlly;
        this.amount = amount;
        this.checkLess = checkLess;
        this.allowEqual = allowEqual;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        if (card != null)
        {
            int count = BoardManager.getChessAmount(checkAlly ? isLocal == GameManager.IsLocalWhite() : isLocal != GameManager.IsLocalWhite(), chessTypes);

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

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (currentMana >= card.getCost())
            options.Add(new CardUseOption(card, 1));

        return options;
    }
}
