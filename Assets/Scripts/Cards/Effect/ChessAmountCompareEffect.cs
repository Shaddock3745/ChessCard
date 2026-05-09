using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAmountCompareEffect : CardEffect
{
    List<ChessType> chessTypes;
    bool checkAllyLess, allowEqual;
    int threshold;

    public ChessAmountCompareEffect(bool checkAllyLess, bool allowEqual, int threshold = 0, params ChessType[] chessTypes)
    {
        this.checkAllyLess = checkAllyLess;
        this.allowEqual = allowEqual;
        this.threshold = threshold;
        this.chessTypes = new List<ChessType>(chessTypes);
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        int allyAmount = BoardManager.getAllChessPieces(isLocal == GameManager.IsLocalWhite(), chessTypes).Count;
        int enemyAmount = BoardManager.getAllChessPieces(isLocal != GameManager.IsLocalWhite(), chessTypes).Count;

        if (checkAllyLess)
            if(allowEqual)
                return allyAmount + threshold <= enemyAmount;
            else
                return allyAmount + threshold < enemyAmount;
        else
            if (allowEqual)
                return allyAmount - threshold >= enemyAmount;
            else
                return allyAmount - threshold > enemyAmount;
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
