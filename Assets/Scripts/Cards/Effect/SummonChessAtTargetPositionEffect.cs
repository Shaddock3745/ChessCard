using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class SummonChessAtTargetPositionEffect : CardEffect, PayloadScoreEvaluator
{
    private static void processSummonWithoutAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        BoardManager.summonChessPiece(type, isWhite, position);
    }

    private static async Task processSummonWithAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        await BoardManager.summonChessPieceWithAnimation(type, isWhite, position);
    }

    private ChessType chessType;

    public SummonChessAtTargetPositionEffect(ChessType chessType)
    {
        this.chessType = chessType;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        return !BoardManager.chessAmountReachLimit(chessType, isWhite);
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
            return;

        bool isWhite = isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite();
        Vector2Int pos = targets.getTargetPositions()[0];

        if (isLocal)
            await processSummonWithAnimation(chessType, isWhite, pos);
        else
        {
            await BoardManager.showTargetWithAnimation(targets.getTargetPositions());
            await processSummonWithAnimation(chessType, isWhite, pos);
            BoardManager.removeAllHint();
        }

        InputManager.tryDetectHover();
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    public List<CardUseOption> getUseOptions(bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        return new List<CardUseOption>();
    }

    // ★ 修正實作評分介面：不考慮原本格子上的棋子狀態，純粹評估新棋子放置於此的價值
    public float evaluatePayloadScore(bool isLocal, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
            return 0f;

        Vector2Int pos = targets.getTargetPositions()[0];
        bool isCasterWhite = isLocal == GameManager.IsLocalWhite();
        AiProfile profile = MatchDataManager.getPlayerData(isLocal).getAiProfile();

        // 計算新棋子進場的總價值 (本體分 + 位置分)
        float newPieceValue = AiEvaluator.getPieceValue(chessType);
        ChessPiece tempChess = new ChessPiece(chessType, isCasterWhite, pos, true); ;
        float newPiecePosValue = AiEvaluator.getPositionalValue(tempChess, pos, isCasterWhite, profile, currentTurnCount);

        return newPieceValue + newPiecePosValue;
    }
}