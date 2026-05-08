using UnityEditor.Rendering;

public enum ChessStatusType
{
    CANT_MOVE,
    CANT_ATTACK,
    CANT_BE_ATTACKED,
    MOVE_COST_MODIFIER,
    MOVEMENT_OVERRIDE
}

public class ChessStatus : BaseStatus<ChessStatusType>
{
    public ChessStatus(ChessStatusType status, int startTurn, int endTurn) : base(status, startTurn, endTurn)
    {

    }
}

public class ChessStatusContainer : StatusContainer<ChessStatus, ChessStatusType>
{

}