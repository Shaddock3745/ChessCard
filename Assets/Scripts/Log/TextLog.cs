public class TextLog : LogData
{
    // 外框與顏色裝飾保留在程式碼，{0} 由對照表的語句填入（避免把富文字標籤塞進翻譯檔）
    private static readonly string DECORATION_TEMPLATE = "=========\n<color=#FACC15>【 {0} 】</color>\n========";

    public TextLog(string message)
    {
        this.message = message;
    }

    public static TextLog createTurnStartLog(bool isWhite)
    {
        string inner = string.Format(LocalizationManager.get("log_turn_start"), playerText(isWhite));
        return new TextLog(string.Format(DECORATION_TEMPLATE, inner));
    }

    public static TextLog createGameOverLog(bool isWhiteWin)
    {
        string inner = string.Format(LocalizationManager.get("log_game_over"), playerText(isWhiteWin));
        return new TextLog(string.Format(DECORATION_TEMPLATE, inner));
    }
}
