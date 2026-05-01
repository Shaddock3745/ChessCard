using UnityEngine;

public class TextLog : LogData
{
    private static readonly string TURN_CHANGE_MESSAGE_TEMPLATE = "=========\n<color=#FACC15>【 {0}回合開始 】</color>\n========";

    public TextLog(string message)
    {
        this.message = message;
    }

    public static TextLog createTurnStartLog(bool isWhite)
    {
        return new TextLog(string.Format(TURN_CHANGE_MESSAGE_TEMPLATE, isWhite ? LogData.WHITE_TEXT : LogData.BLACK_TEXT));
    }
}
