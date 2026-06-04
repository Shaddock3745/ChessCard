using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AddPlayerStatusEffect : CardEffect
{
    private bool isEnemy;
    private int duration, delay;
    private Func<int, int, PlayerStatus> statusFactory;

    public AddPlayerStatusEffect(bool isEnemy, int duration, Func<int, int, PlayerStatus> statusFactory)
    {
        this.isEnemy = isEnemy;
        this.duration = duration;
        this.delay = 0;
        this.statusFactory = statusFactory;
    }

    public AddPlayerStatusEffect(bool isEnemy, int duration, int delay, Func<int, int, PlayerStatus> statusFactory)
    {
        this.isEnemy = isEnemy;
        this.duration = duration;
        this.delay = delay;
        this.statusFactory = statusFactory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int currentTurnCount = GameManager.getCurrentTurnCount();
        MatchDataManager.addStatus(isEnemy ^ isLocal, statusFactory.Invoke(currentTurnCount + delay, currentTurnCount + duration + delay));
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

        if (!canActivate(isLocal, currentTurnCount, card))
            return options;

        options.Add(new CardUseOption(card, 0f, EffectTargetPayload.empty()));
        return options;
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        PlayerData targetPlayer = isEnemy ? clonedEnemy : clonedAlly;
        targetPlayer.addStatus(statusFactory.Invoke(currentTurnCount + delay, currentTurnCount + duration + delay));
    }
}