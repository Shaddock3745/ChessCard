using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AddStatusToAreaChessEffect : CardEffect
{
    private bool isAlly;
    private int duration;
    private Func<int, int, ChessStatus> statusFactory;
    private HashSet<ChessType> validTargetTypes;
    private Func<bool, EffectTargetPayload, List<Vector2Int>> centerPositionsFactory;
    private Func<Vector2Int, Vector2Int, bool> positionFilter;
    private Vector2Int[] directions;

    public AddStatusToAreaChessEffect(
         bool isAlly,
         int duration,
         Func<int, int, ChessStatus> statusFactory,
         ChessType[] validTargetTypes,
         Func<bool, EffectTargetPayload, List<Vector2Int>> centerPositionsFactory,
         Func<Vector2Int, Vector2Int, bool> positionFilter,
         params Vector2Int[] directions)
    {
        this.isAlly = isAlly;
        this.duration = duration;
        this.statusFactory = statusFactory;
        this.validTargetTypes = validTargetTypes != null ? new HashSet<ChessType>(validTargetTypes) : new HashSet<ChessType>();
        this.centerPositionsFactory = centerPositionsFactory;
        this.positionFilter = positionFilter;
        this.directions = directions;
    }

    public AddStatusToAreaChessEffect(
         bool isAlly,
         int duration,
         Func<int, int, ChessStatus> statusFactory,
         ChessType[] validTargetTypes,
         Func<bool, EffectTargetPayload, List<Vector2Int>> centerPositionsFactory,
         params Vector2Int[] directions)
         : this(isAlly, duration, statusFactory, validTargetTypes, centerPositionsFactory, null, directions)
    {
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int startTurn = GameManager.getCurrentTurnCount();
        int endTurn = startTurn + duration;

        bool casterIsWhite = isLocal == GameManager.IsLocalWhite();

        List<Vector2Int> centerPositions = centerPositionsFactory.Invoke(isLocal, targets);

        foreach (Vector2Int center in centerPositions)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int targetPos = center + dir;

                if (BoardManager.isPositionOnBoard(targetPos) && !BoardManager.isEmpty(targetPos))
                {
                    if (positionFilter != null && !positionFilter.Invoke(center, targetPos))
                        continue;

                    ChessPiece piece = BoardManager.getChessPieceAtPosition(targetPos);

                    if (piece.isWhite() == casterIsWhite != isAlly)
                        continue;

                    if (!validTargetTypes.Contains(piece.getChessType()))
                        continue;

                    piece.addStatus(statusFactory.Invoke(startTurn, endTurn));
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        throw new NotImplementedException();
    }
}