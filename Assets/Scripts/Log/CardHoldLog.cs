public class CardHoldLog : LogData
{
    public CardHoldLog(bool isWhite)
    {
        message = string.Format(LocalizationManager.get("log_card_hold"), playerText(isWhite));
    }
}
