using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RedrawEffect : CardEffect
{
    private bool isEnemy;

    public RedrawEffect(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return MatchDataManager.getPlayerHandCardCount(isEnemy != isLocal) > 0;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        bool isPlayerLocal = isEnemy != isLocal;
        List<Card> currentHand = new List<Card>(MatchDataManager.getPlayerData(isPlayerLocal).getHandList());
        int count = currentHand.Count;

        if (count == 0)
            return;

        foreach (Card c in currentHand)
            MatchDataManager.rechargeCard(isPlayerLocal, c);

        MatchDataManager.replenish(isPlayerLocal);
        await Task.Delay(500);

        for (int i = 0; i < count; i++)
            MatchDataManager.drawCard(isPlayerLocal);
    }

    // ★ 標記為隨機效果，告訴 DFS 引擎洗牌並重新抽牌後結果不可測，在此中斷深搜
    public bool isRandomEffect()
    {
        return true;
    }

    // ★ 更新簽章，完全拔除原本複雜的「卡手牌估算」邏輯，直接給予 0f
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (canActivate(isLocal, currentTurnCount, card))
        {
            options.Add(new CardUseOption(card, 0f, EffectTargetPayload.empty()));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：在虛擬玩家資料中充能手牌、洗牌並重新抽出相同數量
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;
        List<Card> currentHand = new List<Card>(targetPlayer.getHandList());
        int count = currentHand.Count;

        if (count == 0)
            return;

        // 將虛擬手牌塞回虛擬牌庫
        foreach (Card c in currentHand)
        {
            targetPlayer.rechargeCard(c);
        }

        // 補充與洗牌
        targetPlayer.replenish();

        // 重新抽出相同數量的牌
        for (int i = 0; i < count; i++)
        {
            targetPlayer.drawCard();
        }
    }
}