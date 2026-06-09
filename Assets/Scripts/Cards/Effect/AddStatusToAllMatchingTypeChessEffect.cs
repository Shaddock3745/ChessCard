using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class AddStatusToAllMatchingTypeChessEffect : CardEffect
{
    private bool isAlly, hardConstraint;
    private List<ChessType> targetTypes;
    private Func<int, IEnumerable<ChessStatus>> statusFactory;

    public AddStatusToAllMatchingTypeChessEffect(bool isAlly, Func<int, IEnumerable<ChessStatus>> statusFactory, bool hardConstraint, params ChessType[] targetTypes)
    {
        this.isAlly = isAlly;
        this.hardConstraint = hardConstraint;
        this.targetTypes = new List<ChessType>(targetTypes);
        this.statusFactory = statusFactory;
    }

    public AddStatusToAllMatchingTypeChessEffect(bool isAlly, int duration, Func<int, int, ChessStatus> statusFactory, bool hardConstraint, params ChessType[] targetTypes)
        : this(isAlly, (startTurn) => new ChessStatus[] { statusFactory.Invoke(startTurn, startTurn + duration) }, hardConstraint, targetTypes)
    {

    }

    public bool canActivate(bool isLocal, int currentTurnCount, Card card)
    {
        if (!hardConstraint)
            return true;
        else
            return BoardManager.getChessAmount(isAlly == (isLocal == GameManager.IsLocalWhite()), targetTypes) > 0;
    }

    public Task executeEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        int currentTurnCount = GameManager.getCurrentTurnCount();
        List<ChessPiece> playerChessList = BoardManager.getAllChessPieces(isAlly == (isLocal == GameManager.IsLocalWhite()), targetTypes);

        foreach (var chess in playerChessList)
            foreach (var status in statusFactory.Invoke(currentTurnCount))
                BoardManager.addStatus(chess, status);

        return Task.CompletedTask;
    }

    public Task<EffectTargetPayload> gatherTargets()
    {
        return Task.FromResult(EffectTargetPayload.empty());
    }

    // ★ 更新簽章，並完全改用傳入的 state 來搜查虛擬盤面
    public List<CardUseOption> getUseOptions(BoardState state, bool isLocal, Card card, int currentMana, int currentTurnCount)
    {
        List<CardUseOption> options = new List<CardUseOption>();

        bool isWhite = isLocal == GameManager.IsLocalWhite();
        bool targetIsWhite = isAlly == isWhite;
        HashSet<ChessType> typeSet = new HashSet<ChessType>(targetTypes);
        bool hasTarget = false;

        // 在虛擬盤面上確認是否有目標
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece piece = state.getChessPieceAtPosition(x, y);
                if (piece != null && piece.isWhite() == targetIsWhite && typeSet.Contains(piece.getChessType()))
                {
                    hasTarget = true;
                    break;
                }
            }
            if (hasTarget) break;
        }

        // 如果是硬性限制且沒有目標，則不產生出牌選項
        if (hardConstraint && !hasTarget)
            return options;

        // ★ 拔除原有的計分邏輯，給予初始分數 0f，讓 DFS 透過虛擬推演後的盤面來決定真正的價值
        options.Add(new CardUseOption(card,EffectTargetPayload.empty()));
        return options;
    }

    // ★ DFS 虛擬執行實作：遍歷虛擬盤面，對所有符合條件的虛擬棋子掛載狀態
    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, EffectTargetPayload targets, int currentTurnCount)
    {
        bool isWhite = clonedAlly.IsWhite();
        bool targetIsWhite = isAlly == isWhite;
        HashSet<ChessType> typeSet = new HashSet<ChessType>(targetTypes);

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                ChessPiece virtualPiece = clonedState.getChessPieceAtPosition(x, y);

                if (virtualPiece != null && virtualPiece.isWhite() == targetIsWhite && typeSet.Contains(virtualPiece.getChessType()))
                {
                    IEnumerable<ChessStatus> targetStatuses = statusFactory.Invoke(currentTurnCount);
                    foreach (ChessStatus status in targetStatuses)
                    {
                        virtualPiece.addStatus(status);
                    }
                }
            }
        }
    }
}