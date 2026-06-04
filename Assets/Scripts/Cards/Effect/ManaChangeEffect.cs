using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ManaChangeEffect : CardEffect
{
    private bool isEnemy;
    private int delta;

    public ManaChangeEffect(bool isEnemy, int delta)
    {
        this.isEnemy = isEnemy;
        this.delta = delta;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        MatchDataManager.changePlayerMana(isEnemy != isLocal, delta);
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，拔除對真實 MatchDataManager 的依賴與複雜算分
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (card != null)
        {
            // 給予 0f，交由 DFS 推演後面的動作來決定這張卡帶來的實質效益
            options.Add(new CardUseOption(card, 0f, EffectTargetPayload.empty()));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：直接修改虛擬玩家資料的魔力值
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;
        targetPlayer.addMana(delta);
    }
}