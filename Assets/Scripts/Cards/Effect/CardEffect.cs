using System.Collections.Generic;
using System.Threading.Tasks;

public interface CardEffect
{
    // ==========================================
    // 真實遊戲執行接口
    // ==========================================
    public bool canActivate(bool isLocal, int currentTurnCount, Card card);

    public Task<EffectTargetPayload> gatherTargets();

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets);

    Task<EffectTargetPayload> gatherTargets(List<EffectTargetPayload> previousPayloads)
    {
        return gatherTargets();
    }

    // ==========================================
    // AI DFS 虛擬推演專用接口
    // ==========================================

    // ★ 擴充 BoardState 參數，讓卡片能在虛擬盤面上尋找打法
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount);

    // AI 在腦海中模擬執行這張卡片的效果
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount);

    bool isRandomEffect()
    {
        return false;
    }
}