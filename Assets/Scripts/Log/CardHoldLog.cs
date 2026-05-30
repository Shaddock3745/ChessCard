using UnityEngine;

public class CardHoldLog : LogData
{
    private static readonly string MESSAGE_TEMPLATE = "{0} 保留了一張卡";

    public CardHoldLog(bool isWhite)
    {
        message = string.Format(MESSAGE_TEMPLATE, isWhite ? LogData.WHITE_TEXT : LogData.BLACK_TEXT);
    }
}
