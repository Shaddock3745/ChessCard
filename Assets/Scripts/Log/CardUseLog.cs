using UnityEngine;

public class CardUseLog : LogData
{
    private static readonly string MESSAGE_TEMPLATE = "{0} 使用了 {1}", LINK_STRING_TEMPLATE = "<link=\"{0}\"><u><color=#FFD700>{1}</color></u></link>";

    private Card card;

    public Card getCard() => card;

    public CardUseLog(bool isWhite, Card card)
    {
        this.card = card;
        message = string.Format(MESSAGE_TEMPLATE, isWhite ? LogData.WHITE_TEXT : LogData.BLACK_TEXT, string.Format(LINK_STRING_TEMPLATE, registerdata(card), card.getName()));
    }
}
