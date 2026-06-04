using System.Threading.Tasks;

public class CardHoldOption : AiOption
{
    private Card card;
    private float score;
    private float quickScore; // 用於 Beam Search 快篩排序

    public CardHoldOption(Card card, float score)
    {
        this.card = card;
        this.score = score;
    }

    public float getScore() => score;
    public void setScore(float score) => this.score = score;
    public float getQuickScore() => quickScore;
    public void setQuickScore(float score) => this.quickScore = score;

    public bool isRandomEffect() => false;
    public bool isHoldAction() => true; // ★ 標記為保留動作，觸發 DFS 提早結算

    public Task execute(bool isLocal)
    {
        MatchDataManager.holdCard(isLocal, card);
        return Task.CompletedTask;
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, int currentTurnCount)
    {
        // 透過 ID 在虛擬手牌中找到這張卡的「分身」，避免記憶體參考錯誤
        Card virtualCard = clonedAlly.getHandList().Find(c => c.getId() == card.getId() && c.getCost() == card.getCost());

        if (virtualCard != null)
        {
            // 在虛擬資料中保留手牌並扣除魔力 (PlayerData 內部的 holdCard 會處理扣費)
            clonedAlly.holdCard(virtualCard);
        }
    }
}