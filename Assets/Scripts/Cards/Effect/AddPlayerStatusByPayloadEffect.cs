using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AddPlayerStatusByPayloadEffect : CardEffect
{
    private bool isEnemy;
    private int duration, delay;
    private Func<int, int, EffectTargetPayload, BoardState, PlayerStatus> statusFactory;

    public AddPlayerStatusByPayloadEffect(bool isEnemy, int duration, Func<int, int, EffectTargetPayload, BoardState, PlayerStatus> statusFactory)
    { this.isEnemy = isEnemy; this.duration = duration; this.delay = 0; this.statusFactory = statusFactory; }

    public AddPlayerStatusByPayloadEffect(bool isEnemy, int duration, int delay, Func<int, int, EffectTargetPayload, BoardState, PlayerStatus> statusFactory)
    { this.isEnemy = isEnemy; this.duration = duration; this.delay = delay; this.statusFactory = statusFactory; }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int t = GameManager.getCurrentTurnCount();
        BoardState realView = BoardManager.getCloneBoardState();   // 真實路徑:真實盤面快照
        MatchDataManager.addStatus(isEnemy ^ isLocal, statusFactory.Invoke(t + delay, t + duration + delay, targets, realView));
        return Task.CompletedTask;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;
        targetPlayer.addStatus(statusFactory.Invoke(currentTurnCount + delay, currentTurnCount + duration + delay, targets, clonedState));  // 虛擬路徑:clonedState
    }
}