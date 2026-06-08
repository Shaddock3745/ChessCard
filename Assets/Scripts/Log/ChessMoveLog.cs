using UnityEngine;

public class ChessMoveLog : LogData
{
    private static readonly string LINK_STRING_TEMPLATE = "<link=\"{0}\"><u><color=#336699>{1}</color></u></link>";

    private ChessPiece movedChess;
    private Vector2Int from, to;

    public ChessPiece getChess() => movedChess;

    public Vector2Int getFromPosition() => from;

    public Vector2Int getToPosition() => to;

    public ChessMoveLog(ChessPiece movedChess, Vector2Int from, Vector2Int to)
    {
        this.movedChess = movedChess;
        this.from = from;
        this.to = to;

        message = string.Format(LocalizationManager.get("log_chess_move"), playerText(movedChess.isWhite()), string.Format(LINK_STRING_TEMPLATE, registerdata(movedChess), movedChess.getChessTypeName()), LogManager.toChessCoord(from), LogManager.toChessCoord(to));
    }
}
