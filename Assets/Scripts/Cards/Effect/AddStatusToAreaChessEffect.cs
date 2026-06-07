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
    private Func<BoardState, bool, EffectTargetPayload, List<Vector2Int>> centerPositionsFactory;
    private Func<Vector2Int, Vector2Int, bool> positionFilter;
    private Vector2Int[] directions;

    public AddStatusToAreaChessEffect(
         bool isAlly,
         int duration,
         Func<int, int, ChessStatus> statusFactory,
         ChessType[] validTargetTypes,
         Func<BoardState, bool, EffectTargetPayload, List<Vector2Int>> centerPositionsFactory,
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
        Func<BoardState, bool, EffectTargetPayload, List<Vector2Int>> centerPositionsFactory,
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

        List<Vector2Int> centerPositions = centerPositionsFactory.Invoke(BoardManager.getCloneBoardState(), isLocal, targets);

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

                    BoardManager.addStatus(piece, statusFactory.Invoke(startTurn, endTurn));
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章。由於是管線後端元件，直接回傳空清單，交由複合卡片組合產生選項
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ DFS 虛擬執行實作：直接在虛擬盤面上進行範圍判定與掛載狀態
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        int startTurn = currentTurnCount;
        int endTurn = startTurn + duration;
        bool casterIsWhite = clonedAlly.IsWhite();

        List<Vector2Int> centerPositions = centerPositionsFactory.Invoke(clonedState, clonedAlly.IsLocal(), targets);

        foreach (Vector2Int center in centerPositions)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int targetPos = center + dir;

                // 基礎邊界過濾 (替代 BoardManager.isPositionOnBoard)
                if (targetPos.x < 0 || targetPos.x >= 8 || targetPos.y < 0 || targetPos.y >= 8)
                    continue;

                ChessPiece virtualPiece = clonedState.getChessPieceAtPosition(targetPos);

                if (virtualPiece != null)
                {
                    if (positionFilter != null && !positionFilter.Invoke(center, targetPos))
                        continue;

                    if (virtualPiece.isWhite() == casterIsWhite != isAlly)
                        continue;

                    if (!validTargetTypes.Contains(virtualPiece.getChessType()))
                        continue;

                    virtualPiece.addStatus(statusFactory.Invoke(startTurn, endTurn));
                }
            }
        }
    }
}