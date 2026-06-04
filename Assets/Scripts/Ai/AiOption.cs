using System.Threading.Tasks;

public interface AiOption
{
    // ==========================================
    // 真實遊戲執行接口
    // ==========================================
    public float getScore();
    public void setScore(float score);
    public Task execute(bool isLocal);

    // ==========================================
    // DFS 虛擬推演專用接口
    // ==========================================
    public void setQuickScore(float score);
    public float getQuickScore();

    // 判斷是否為隨機效果 (例如抽牌)。若是，DFS 會在此中斷並結算盤面。
    public bool isRandomEffect();

    // 判斷是否為保留卡片。若是，代表這回合連擊結束，DFS 提早結算。
    public bool isHoldAction();

    // 虛擬執行核心：絕對不可包含動畫、不可呼叫 Manager 的真實狀態
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, int currentTurnCount);
}