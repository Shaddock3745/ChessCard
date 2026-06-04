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

            Card pick = await InputManager.waitForCardSelection(validCards, true);

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

    // ★ 更新簽章，保留挑選目標的貪婪邏輯作為休探（Heuristic）以防組合爆炸，但將評分交給 DFS
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (!canActivate(isLocal, currentTurnCount, card))
            return options;

        List<Card> discardPile = MatchDataManager.getPlayerDiscardCardList(isLocal);
        List<Card> validCards = discardPile.FindAll(c => !invalidCardId.Contains(c.getId()));

        if (validCards.Count == 0)
            return options;

        int remainingMana = currentMana - card.getCost();
        int actualRetrieveCount = Mathf.Min(amount, validCards.Count);

        // 1. 根據「當下可用性」與「卡手代價」對有效棄牌進行評估排序 (做為選項過濾器)
        validCards.Sort((a, b) => EvaluateCardRetrieveScore(b, remainingMana).CompareTo(EvaluateCardRetrieveScore(a, remainingMana)));

        List<Card> selectedCards = new List<Card>();

        // 2. 貪婪選出當前動態價值最高的前 N 張卡片組合
        for (int i = 0; i < actualRetrieveCount; i++)
        {
            selectedCards.Add(validCards[i]);
        }

        // 3. 拔除手動算分給予 0f，由 DFS 取得卡片後在接下來的推演決定實質價值
        options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(selectedCards)));
        return options;
    }

    // 用於 DFS 選目標的輔助排序邏輯
    private float EvaluateCardRetrieveScore(Card targetCard, int remainingMana)
    {
        int targetCost = targetCard.getCost();

        if (targetCost <= remainingMana)
            return 15f + 25f + (targetCost * 2f);
        else
            return 15f - (targetCost * 12f);
    }

    // ★ DFS 虛擬執行實作：在虛擬玩家資料中將指定的卡片從棄牌堆加入手牌
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetCards() == null || targets.getTargetCards().Count == 0)
            return;

        foreach (Card realCard in targets.getTargetCards())
        {
            // 因為 PlayerData.Clone() 做了深度拷貝，虛擬棄牌堆裡的 Card 物件與 realCard 記憶體位置不同
            // 我們必須透過 ID 找到對應的虛擬卡片來執行回收
            Card virtualCard = clonedAlly.getDiscardList().Find(c => c.getId() == realCard.getId());
            if (virtualCard != null)
            {
                clonedAlly.retrieveCard(virtualCard);
            }
        }
    }
}