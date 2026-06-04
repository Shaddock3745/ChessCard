using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAmountCheckEffect : CardEffect
{
    List<ChessType> chessTypes;
    int amount;
    bool checkAlly, checkLess, allowEqual;

    public ChessAmountCheckEffect(bool checkAlly, int amount, bool checkLess, bool allowEqual, params ChessType[] type)
    {
        chessTypes = new List<ChessType>(type);
        this.checkAlly = checkAlly;
        this.amount = amount;
        this.checkLess = checkLess;
        this.allowEqual = allowEqual;
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        if (card != null)
        {
            int count = BoardManager.getChessAmount(checkAlly ? isLocal == GameManager.IsLocalWhite() : isLocal != GameManager.IsLocalWhite(), chessTypes);

            if (allowEqual)
                if (checkLess)
                    return count <= amount;
                else
                    return count >= amount;
            else
                if (checkLess)
                    return count < amount;
                else
                    return count > amount;
        }

        return false;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，並完全改用傳入的 state 來計算虛擬盤面上的數量
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        if (card == null) return options;

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        bool targetIsWhite = checkAlly ? isWhite : !isWhite;
        HashSet<ChessType> typeSet = new HashSet<ChessType>(chessTypes);
        int count = 0;

        // 在虛擬盤面上數棋子
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && piece.isWhite() == targetIsWhite && typeSet.Contains(piece.getChessType()))
                {
                    count++;
                }
            }
        }

        // 條件判定
        bool conditionMet = false;
        if (allowEqual)
            conditionMet = checkLess ? count <= amount : count >= amount;
        else
            conditionMet = checkLess ? count < amount : count > amount;

        if (conditionMet)
        {
            // 本身只做條件判定，無須預先計分，回傳 0f 讓 DFS 評價後續串聯的效果
            options.Add(new CardUseOption(card, 0f));
        }

        return options;
    }

    // ★ DFS 虛擬執行實作：作為條件檢查節點，此 Effect 本身不對盤面造成任何實質改變，留空即可
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        // Do nothing.
    }
}