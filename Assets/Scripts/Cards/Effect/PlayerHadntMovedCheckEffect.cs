using System.Collections.Generic;
using System.Threading.Tasks;

public class PlayerHadntMovedCheckEffect : CardEffect
{
    private bool isEnemy;

    public PlayerHadntMovedCheckEffect(bool isEnemy)
    {
        this.isEnemy = isEnemy;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        // 這裡依然透過真實的 MatchDataManager 檢查。
        // 在 DFS 產生初步出牌選項時，這能有效過濾掉不符合條件的卡片。
        return !MatchDataManager.hadMovedThisTurn(isEnemy != isLocal);
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，加入 BoardState
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        // 純條件判斷，通過則給予 0 分選項，交由 DFS 推演評估後續管線的總價值
        if (canActivate(isLocal, currentTurnCount, card))
        {
            options.Add(new CardUseOption(card, 0f));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：作為條件檢查節點，此 Effect 本身不對盤面造成任何實質改變，留空即可
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        // Do nothing.
    }
}