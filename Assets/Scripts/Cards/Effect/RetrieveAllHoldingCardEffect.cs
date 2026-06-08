using System.Collections.Generic;
using System.Threading.Tasks;

public class RetrieveAllHoldingCardEffect : CardEffect
{
    private bool isEnemy;

    public RetrieveAllHoldingCardEffect(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        // ★ 幫你修正了原本忽略 isEnemy 的小 Bug
        MatchDataManager.retrieveAllHoldingCard(isLocal != isEnemy);
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，並給予 0f 讓 DFS 與複合卡片共同結算
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (card != null)
        {
            options.Add(new CardUseOption(card, 0f));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：直接在虛擬玩家資料中收回保留卡片
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;

        // 呼叫 PlayerData 中的 retrieveAllHoldingCard()
        // 將 holdingList 的虛擬卡片轉移到 deck 中
        targetPlayer.retrieveAllHoldingCard();
    }
}