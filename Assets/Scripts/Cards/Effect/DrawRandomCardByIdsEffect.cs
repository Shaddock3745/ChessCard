using System.Collections.Generic;
using System.Threading.Tasks;

public class DrawRandomCardByIdsEffect : CardEffect
{
    private bool isEnemy;
    private int[] validCardIds;

    public DrawRandomCardByIdsEffect(bool isEnemy, params int[] validCardIds)
    {
        this.isEnemy = isEnemy;
        this.validCardIds = validCardIds;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        // 這裡維持呼叫 MatchDataManager 是沒問題的，
        // 因為這是在真正發動前，對「真實牌庫」進行有沒有牌的防呆檢查
        return MatchDataManager.hasValidCardInDeckOrWaiting(isLocal, validCardIds);
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        MatchDataManager.drawRandomCardFromDeck(isEnemy != isLocal, validCardIds);
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 新增這段：標記為隨機效果，告訴 DFS 引擎在此中斷深搜，直接結算盤面分數
    public bool isRandomEffect()
    {
        return true;
    }

    // ★ 更新簽章，完全拔除對卡片價值的猜測算分，直接給 0f
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (card != null)
        {
            options.Add(new CardUseOption(card));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：在虛擬玩家資料中執行指定 ID 的抽牌
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;

        // 直接呼叫虛擬 PlayerData 內部的抽牌方法
        // 雖然抽出來的牌 DFS 不會繼續探勘（因為 isRandomEffect 為 true），
        // 但抽牌動作會增加虛擬玩家的「手牌數量」，這會讓結算時的「資源分數(resourceWeight)」大幅提升！
        targetPlayer.drawRandomCardFromDeck(validCardIds);
    }
}