using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChessAmountCompareEffect : CardEffect
{
    List<ChessType> chessTypes;
    bool checkAllyLess, allowEqual;
    int threshold;

    public ChessAmountCompareEffect(bool checkAllyLess, bool allowEqual, int threshold = 0, params ChessType[] chessTypes)
    {
        this.checkAllyLess = checkAllyLess;
        this.allowEqual = allowEqual;
        this.threshold = threshold;
        this.chessTypes = new List<ChessType>(chessTypes);
    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        bool isWhite = isLocal == GameManager.IsLocalWhite();
        int allyAmount = BoardManager.getAllChessPieces(isWhite, chessTypes).Count;
        int enemyAmount = BoardManager.getAllChessPieces(!isWhite, chessTypes).Count;

        if (checkAllyLess)
            if (allowEqual)
                return allyAmount + threshold <= enemyAmount;
            else
                return allyAmount + threshold < enemyAmount;
        else
            if (allowEqual)
                return allyAmount - threshold >= enemyAmount;
            else
                return allyAmount - threshold > enemyAmount;
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
        HashSet<ChessType> typeSet = new HashSet<ChessType>(chessTypes);
        int allyAmount = 0;
        int enemyAmount = 0;

        // 在虛擬盤面上數棋子
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && typeSet.Contains(piece.getChessType()))
                {
                    if (piece.isWhite() == isWhite)
                        allyAmount++;
                    else
                        enemyAmount++;
                }
            }
        }

        // 條件判定
        bool conditionMet = false;
        if (checkAllyLess)
        {
            if (allowEqual)
                conditionMet = allyAmount + threshold <= enemyAmount;
            else
                conditionMet = allyAmount + threshold < enemyAmount;
        }
        else
        {
            if (allowEqual)
                conditionMet = allyAmount - threshold >= enemyAmount;
            else
                conditionMet = allyAmount - threshold > enemyAmount;
        }

        if (conditionMet)
        {
            // 作為條件節點回傳 0f，由複合卡片與 DFS 推演決定最終價值
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