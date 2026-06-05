using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DrawCardEffect : CardEffect
{
    private bool isEnemy;
    private int amount;

    public DrawCardEffect(bool isEnemy, int amount)
    {
        this.isEnemy = isEnemy;
        this.amount = amount;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        // 真實遊戲執行
        bool targetIsLocal = isEnemy ? !isLocal : isLocal;
        int count = MatchDataManager.getPlayerDeckCardCount(targetIsLocal) + MatchDataManager.getPlayerWaitingCardCount(targetIsLocal);

        for (int i = 0; i < amount && count-- > 0; i++)
            MatchDataManager.drawCard(targetIsLocal);

        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，拔除對真實 MatchDataManager 的依賴與算分，給予 0f 讓 DFS 在落子後評估資源價值
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (card != null)
        {
            options.Add(new CardUseOption(card, 0f));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：在虛擬玩家資料中執行抽牌
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;

        // 由於 AI 的 DFS 推演無法預測會抽到什麼牌，
        // 這裡假設你的 PlayerData 內部有對應的純資料抽牌方法（例如抽一張未知的虛擬牌或單純增加手牌數量）。
        // 這樣 DFS 在此處中斷探勘並結算時，就能把「手牌數量增加」算入運營級的資源分數 (resourceValue) 中。

        for (int i = 0; i < amount; i++)
        {
            targetPlayer.drawCard(); // 請確保 PlayerData 中有這個給虛擬資料用的方法，沒有的話可自行調整名稱
        }
    }

    public bool isRandomEffect() => true;

}