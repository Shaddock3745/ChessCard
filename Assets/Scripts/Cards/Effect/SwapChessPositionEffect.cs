using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SwapChessPositionEffect : CardEffect, PayloadScoreEvaluator
{
    private Func<EffectTargetPayload, (Vector2Int, Vector2Int)> factory;

    public SwapChessPositionEffect(Func<EffectTargetPayload, (Vector2Int, Vector2Int)> factory)
    {
        this.factory = factory;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        var (firstPostion, secondPostion) = factory.Invoke(targets);

        await BoardManager.swapTwoPositionWithAnimation(firstPostion, secondPostion);
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ 實作評分介面：考量兩個棋子調換前後的位置分數變化
    public float evaluatePayloadScore(bool isLocal, EffectTargetPayload targets, int currentTurnCount)
    {
        var (pos1, pos2) = factory.Invoke(targets);
        ChessPiece piece1 = BoardManager.getChessPieceAtPosition(pos1);
        ChessPiece piece2 = BoardManager.getChessPieceAtPosition(pos2);

        AiProfile profile = MatchDataManager.getPlayerData(isLocal).getAiProfile();
        bool isCasterWhite = isLocal == GameManager.IsLocalWhite();
        float score = 0f;

        // 計算第一個棋子調換後的位置價值改變
        if (piece1 != null)
        {
            float oldVal = AiEvaluator.getPositionalValue(piece1, pos1, piece1.isWhite(), profile, currentTurnCount);
            float newVal = AiEvaluator.getPositionalValue(piece1, pos2, piece1.isWhite(), profile, currentTurnCount);

            if (piece1.isWhite() == isCasterWhite)
                score += (newVal - oldVal); // 友軍：換去更好的位置加分
            else
                score += (oldVal - newVal); // 敵軍：被換去更差的位置加分
        }

        // 計算第二個棋子調換後的位置價值改變
        if (piece2 != null)
        {
            float oldVal = AiEvaluator.getPositionalValue(piece2, pos2, piece2.isWhite(), profile, currentTurnCount);
            float newVal = AiEvaluator.getPositionalValue(piece2, pos1, piece2.isWhite(), profile, currentTurnCount);

            if (piece2.isWhite() == isCasterWhite)
                score += (newVal - oldVal);
            else
                score += (oldVal - newVal);
        }

        // 王車易位特殊防禦戰術價值加成
        if (piece1 != null && piece1.getChessType() == ChessType.King) score += 25f;
        if (piece2 != null && piece2.getChessType() == ChessType.King) score += 25f;

        return score;
    }
}