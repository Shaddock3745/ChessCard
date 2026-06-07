using System.Threading.Tasks;
using UnityEngine;

public class ChessMoveOption : AiOption
{
    private ChessPiece piece;
    private Vector2Int currentPos, targetPos;
    private float score;
    private float quickScore;

    public ChessMoveOption(ChessPiece piece, Vector2Int currentPos, Vector2Int targetPos, float score)
    {
        this.piece = piece;
        this.currentPos = currentPos;
        this.targetPos = targetPos;
        this.score = score;
    }

    public Vector2Int getCurrentPosition() => currentPos;

    public Vector2Int getTargetPosition() => targetPos;

    public float getScore() => score;
    public void setScore(float score) => this.score = score;
    public float getQuickScore() => quickScore;
    public void setQuickScore(float score) => this.quickScore = score;

    public bool isRandomEffect() => false;
    public bool isHoldAction() => false;

    public async Task execute(bool isLocal)
    {
        if (BoardManager.isEmpty(targetPos))
        {
            MatchDataManager.doChessMoved(isLocal, piece, currentPos, targetPos, ActionType.Move, GameManager.getCurrentTurnCount());
            await BoardManager.movePieceWithAnimation(currentPos, targetPos);
        }
        else
        {
            MatchDataManager.doChessMoved(isLocal, piece, currentPos, targetPos, ActionType.Attack, GameManager.getCurrentTurnCount());
            await BoardManager.attackPieceWithAnimation(currentPos, targetPos);
        }
    }

    public void executeVirtually(BoardState clonedState, PlayerData clonedAlly, PlayerData clonedEnemy, int currentTurnCount)
    {
        // 1. 利用「座標」取得虛擬盤面上的那顆虛擬棋子
        ChessPiece virtualPiece = clonedState.getChessPieceAtPosition(currentPos);
        if (virtualPiece == null) return;

        // 2. 判斷是單純移動還是攻擊
        ChessPiece virtualVictim = clonedState.getChessPieceAtPosition(targetPos);
        bool isAttack = virtualVictim != null;
        ActionType type = isAttack ? ActionType.Attack : ActionType.Move;

        // 3. 在虛擬玩家資料中扣除移動花費 (AP/Mana) 並添加移動紀錄
        clonedAlly.doChessMoved(virtualPiece, currentPos, targetPos, type, currentTurnCount);

        // 4. 在虛擬盤面上執行移動 (這會自動觸發目標格上棋子的 onDeath() 並清空舊座標)
        clonedState.moveChessPiece(currentPos, targetPos);

        // 5. ★ 核心修正：不呼叫會觸發真實動畫的 onMoveEnd / onAttackEnd
        // 直接改為呼叫 StatusContainer 的「虛擬」觸發介面，將虛擬盤面 (clonedState) 傳入狀態中
        if (isAttack)
        {
            virtualPiece.getStatusContainer().virtualTriggerPostAttack(clonedState, virtualPiece, virtualVictim, currentPos, currentTurnCount);
        }
        else
        {
            virtualPiece.getStatusContainer().virtualTriggerPostMove(clonedState, virtualPiece, currentPos, targetPos, currentTurnCount);
        }
    }

    public string getChessName() => piece.getName();
}