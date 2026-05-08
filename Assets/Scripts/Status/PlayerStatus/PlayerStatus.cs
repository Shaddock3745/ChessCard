public enum PlayerStatusType
{
    CANT_MOVE,
    CANT_SUMMON,
    CANT_ATTACK,
    CANT_USE_SPECIFIC_CARD,
    CANT_USE_ALL_CARD,
    MOVE_COST_MODIFIER,
    CARD_COST_MODIFIER,
}

public class PlayerStatus : BaseStatus<PlayerStatusType>
{
    public PlayerStatus(PlayerStatusType status, int startTurn, int endTurn) : base(status, startTurn, endTurn)
    {

    }
}

public class PlayerStatusContainer : StatusContainer<PlayerStatus, PlayerStatusType>
{

}
