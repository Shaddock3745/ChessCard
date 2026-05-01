using UnityEngine;

public class ChessAttackLog : LogData
{
    private static readonly string MESSAGE_TEMPLATE = "{0} 移動 {1} , {2} -> {3}, 擊殺 {4}", ATTACKER_LINK_STRING_TEMPLATE = "<link=\"{0}\"><u><color=#336699>{1}</color></u></link>", VICTIM_LINK_STRING_TEMPLATE = "<link=\"{0}\"><u><color=#336699>{1}</color></u></link>";

    private ChessPiece attacker, victim;
    private Vector2Int from, to;

    public ChessPiece getChess() => attacker;

    public Vector2Int getFromPosition() => from;

    public Vector2Int getToPosition() => to;

    public ChessAttackLog(ChessPiece attacker, ChessPiece victim, Vector2Int from, Vector2Int to)
    {
        this.attacker = attacker;
        this.victim = victim;
        this.from = from;
        this.to = to;

        message = string.Format(MESSAGE_TEMPLATE, attacker.isWhite() ? LogData.WHITE_TEXT : LogData.BLACK_TEXT, string.Format(ATTACKER_LINK_STRING_TEMPLATE, registerdata(attacker), attacker.getChessType()),
            LogManager.toChessCoord(from), LogManager.toChessCoord(to), string.Format(VICTIM_LINK_STRING_TEMPLATE, registerdata(victim), victim.getChessType()));
    }
}
