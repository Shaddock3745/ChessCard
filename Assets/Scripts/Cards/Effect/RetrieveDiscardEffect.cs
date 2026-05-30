using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RetrieveDiscardEffect : CardEffect
{
    private int amount;
    private List<int> invalidCardId;

    public RetrieveDiscardEffect(int amount, params int[] invalidCardId)
    {
        this.amount = amount;
        this.invalidCardId = new List<int>(invalidCardId);
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        List<Card> discard = MatchDataManager.getPlayerDiscardCardList(isLocal);
        return discard.Exists(c => !invalidCardId.Contains(c.getId()));
    }

    public async Task<EffectTargetPayload> gatherTargets()
    {
        List<Card> validCards = MatchDataManager.getPlayerDiscardCardList(true).FindAll(c => !invalidCardId.Contains(c.getId()));
        List<Card> selectedCards = new List<Card>(); 
        
        for (int i = 0; i < amount; i++)
        {
            if (validCards.Count == 0)
                break;

            Card pick = await InputManager.waitForCardSelection(validCards);

            if (pick != null)
            {
                selectedCards.Add(pick);
                validCards.Remove(pick);

                await Task.Delay(150);
            }
        }

        return new EffectTargetPayload(selectedCards);
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets != null && targets.getTargetCards().Count > 0)
        {
            foreach (Card c in targets.getTargetCards())
                MatchDataManager.retrieveCard(isLocal, c);
        }
        return Task.CompletedTask;
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new System.NotImplementedException();
    }
}
