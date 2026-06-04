using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoarEffect : CardEffect, PayloadScoreEvaluator
{
    private Func<EffectTargetPayload, Vector2Int> piecePositionFactory;
    private Vector2Int[] directions;

    public RoarEffect(Func<EffectTargetPayload, Vector2Int> piecePositionFactory, params Vector2Int[] directions)
    {
        this.piecePositionFactory = piecePositionFactory;
        this.directions = directions;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        List<Task> tasks = new List<Task>();
        Vector2Int roaringPiecePosition = piecePositionFactory.Invoke(targets);
        bool isRoarerWhite = BoardManager.isWhiteAtPosition(roaringPiecePosition);

        foreach (Vector2Int direction in directions)
            tasks.Add(BoardManager.pieceGotRoared(roaringPiecePosition + direction, direction, isRoarerWhite));

        await Task.WhenAll(tasks);
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ 實作評分介面：推開棋子時，加算目標被推移前後的位置分數差
    public float evaluatePayloadScore(bool isLocal, EffectTargetPayload targets, int currentTurnCount)
    {
        Vector2Int roaringPiecePosition = piecePositionFactory.Invoke(targets);
        ChessPiece roaringPiece = BoardManager.getChessPieceAtPosition(roaringPiecePosition);

        if (roaringPiece == null) return 0f;

        AiProfile profile = MatchDataManager.getPlayerData(isLocal).getAiProfile();
        bool isWhite = roaringPiece.isWhite();
        float totalScore = 0f;

        foreach (Vector2Int direction in directions)
        {
            Vector2Int targetPos = roaringPiecePosition + direction;

            if (BoardManager.isPositionOnBoard(targetPos) && !BoardManager.isEmpty(targetPos))
            {
                ChessPiece affectedPiece = BoardManager.getChessPieceAtPosition(targetPos);

                if (affectedPiece != null && affectedPiece.isWhite() != isWhite)
                {
                    float pieceValue = AiEvaluator.getPieceValue(affectedPiece.getChessType());

                    // 計算目標棋子當前的位置分數
                    float oldPosValue = AiEvaluator.getPositionalValue(affectedPiece, targetPos, affectedPiece.isWhite(), profile, currentTurnCount);

                    // 估算被推走後的終點位置分數
                    Vector2Int pushedPos = targetPos + direction;
                    float newPosValue = 0f;

                    if (BoardManager.isPositionOnBoard(pushedPos) && BoardManager.isEmpty(pushedPos))
                    {
                        newPosValue = AiEvaluator.getPositionalValue(affectedPiece, pushedPos, affectedPiece.isWhite(), profile, currentTurnCount);
                    }

                    // 敵軍：原本位置分數越高、新位置分數越低（降得越多越好），AI 獲得的分數就越高
                    float positionalDemotion = oldPosValue - newPosValue;
                    totalScore += 15f + (pieceValue * 0.3f) + positionalDemotion;
                }
            }
        }

        return totalScore;
    }
}