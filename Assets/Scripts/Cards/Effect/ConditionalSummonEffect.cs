using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ConditionalSummonEffect : CardEffect
{
    private ChessType typeToSummon;
    private ChessType typeToCheck;
    private int thresholdCount;

    public ConditionalSummonEffect(ChessType typeToSummon, ChessType typeToCheck, int thresholdCount)
    {
        this.typeToSummon = typeToSummon;
        this.typeToCheck = typeToCheck;
        this.thresholdCount = thresholdCount;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        return true;
    }

    public async Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (targets == null || targets.getTargetPositions().Count == 0)
            return;

        bool isWhite = isLocal == GameManager.IsLocalWhite();

        if (BoardManager.getAllChessPieces(isWhite).FindAll(p => p.getChessType() == typeToCheck).Count < thresholdCount)
            await BoardManager.summonChessPieceWithAnimation(typeToSummon, isWhite, targets.getTargetPositions()[0]);
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，並改用傳入的 state 來檢查條件與尋找空位
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (!canActivate(isLocal, currentTurnCount, card))
            return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        int currentCheckCount = 0;

        // 1. 在虛擬盤面上計算條件棋子的數量
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && piece.isWhite() == isWhite && piece.getChessType() == typeToCheck)
                {
                    currentCheckCount++;
                }
            }
        }

        // 若數量已達標，無法召喚，直接放棄不產生選項
        if (currentCheckCount >= thresholdCount)
            return options;

        // 2. 掃描虛擬全棋盤尋找合法空位
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (state.getChessPieceAtPosition(x, y) == null)
                {
                    // ★ 拔除原本的手動算分，給予 0f 讓 DFS 在落子後評估全盤分數
                    options.Add(new CardUseOption(card, 0f, new EffectTargetPayload(new Vector2Int(x, y))));
                }
            }
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：在虛擬盤面上判定條件，若符合則直接將虛擬棋子加入盤面
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        if (targets == null || targets.getTargetPositions() == null || targets.getTargetPositions().Count == 0)
            return;

        Vector2Int targetPos = targets.getTargetPositions()[0];
        bool isWhite = clonedAlly.IsWhite();
        int currentCheckCount = 0;

        // 在虛擬盤面上確認目前的條件棋子數量
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = clonedState.getChessPieceAtPosition(x, y);
                if (piece != null && piece.isWhite() == isWhite && piece.getChessType() == typeToCheck)
                {
                    currentCheckCount++;
                }
            }
        }

        // 若數量未達標且目標位置為空，則在虛擬盤面上直接生成棋子
        if (currentCheckCount < thresholdCount && clonedState.getChessPieceAtPosition(targetPos) == null)
        {
            clonedState.setChessPieceAtPosition(targetPos, new ChessPiece(typeToSummon, isWhite, targetPos, true));
        }
    }
}