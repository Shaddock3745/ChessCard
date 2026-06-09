using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayerReplenishAndShuffleEffect : CardEffect
{
    private bool isEnemy;

    public PlayerReplenishAndShuffleEffect(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        bool replenishLocal = isLocal != isEnemy;
        MatchDataManager.replenish(replenishLocal);
        MatchDataManager.shuffleDeck(replenishLocal);
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 新增這段：標記為隨機效果，告訴 DFS 引擎洗牌後未來的抽牌不可測，在此中斷深搜
    public bool isRandomEffect()
    {
        return true;
    }

    // ★ 更新簽章並實作原本未完成的方法，給予 0f 讓 DFS 結算
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (card != null)
        {
            options.Add(new CardUseOption(card));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：在虛擬玩家資料中執行資源回收與洗牌
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;

        // 呼叫你在 PlayerData 中寫好的 replenish 方法 (內部已包含 waitingList 回收與 shuffleDeck)
        targetPlayer.replenish();
    }
}