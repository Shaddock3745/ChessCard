public interface VirtualAction
{
    public AiOption GetRealOption();

    // 給 Beam Search 排列用的單步暫存分數
    public void SetQuickScore(float score);
    public float GetQuickScore();

    // 標記此動作是否包含隨機性 (例如抽牌、隨機目標法術)，若有則 DFS 不往下探勘
    public bool IsRandomEffect();

    // 標記此動作是否為「結束連擊」(例如 Hold 留牌)
    public bool IsHoldAction();

    // 真正的虛擬執行：請在內部只改動 clonedState 與 clonedAlly/Enemy，絕對不要呼叫 GameManager 或動畫！
    public void ExecuteVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy);
}
