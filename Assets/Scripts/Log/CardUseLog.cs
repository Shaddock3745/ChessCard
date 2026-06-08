public class CardUseLog : LogData
{
    private static readonly string LINK_STRING_TEMPLATE = "<link=\"{0}\"><u><color=#FFD700>{1}</color></u></link>";

    private Card card;

    public Card getCard() => card;

    public CardUseLog(bool isWhite, Card card)
    {
        this.card = card;
        message = string.Format(LocalizationManager.get("log_card_use"), playerText(isWhite), string.Format(LINK_STRING_TEMPLATE, registerdata(card), card.getName()));
    }
}
