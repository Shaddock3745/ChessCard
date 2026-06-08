using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    private static LogManager instance;

    private static readonly int MAX_LOG_COUNT = 30;
    private static readonly string LINE_BREAK_SINGLE = "\n---------\n";

    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private ScrollRect scrollRect;

    private Queue<LogData> logQueue = new Queue<LogData>();
    private Dictionary<string, object> linkMap = new Dictionary<string, object>();

    public static Dictionary<string, object> getLinkMap() => instance.linkMap;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        BoardManager.OnChessMove += createChessMoveLog;
        BoardManager.OnChessAttack += createChessAttackLog;
        MatchDataManager.OnPlayerHoldCard += createCardHoldLog;
        MatchDataManager.OnPlayerUseCard += createCardUseLog;
        GameManager.OnTurnStart += createTurnStartLog;
        GameManager.OnGameOver += createGameOverLog;
        GameManager.OnResetGame += reset;
    }

    private void OnDisable()
    {
        BoardManager.OnChessMove -= createChessMoveLog;
        BoardManager.OnChessAttack -= createChessAttackLog;
        MatchDataManager.OnPlayerHoldCard -= createCardHoldLog;
        MatchDataManager.OnPlayerUseCard -= createCardUseLog;
        GameManager.OnTurnStart -= createTurnStartLog;
        GameManager.OnGameOver -= createGameOverLog;
        GameManager.OnResetGame -= reset;
    }

    public static void reset()
    {
        instance.logQueue.Clear();
        instance.updateUi();
    }

    public static string rigisterLogData(object data)
    {
        if (instance == null || data == null)
            return "";

        string id = System.Guid.NewGuid().ToString();
        instance.linkMap.Add(id, data);

        return id;
    }

    private static void createLog(LogData data)
    {
        if (instance == null)
            return;

        instance.logQueue.Enqueue(data);

        if (instance.logQueue.Count > MAX_LOG_COUNT)
        {
            LogData oldData = instance.logQueue.Dequeue();

            foreach (string id in oldData.getLinks())
                instance.linkMap.Remove(id);
        }

        instance.updateUi();
    }

    private static void createChessMoveLog(ChessPiece movingPiece, Vector2Int from, Vector2Int to)
    {
        createLog(new ChessMoveLog(movingPiece, from, to));
    }

    private static void createChessAttackLog(ChessPiece attacker, ChessPiece target, Vector2Int from, Vector2Int to)
    {
        createLog(new ChessAttackLog(attacker, target, from, to));
    }

    private static void createGameOverLog(bool isWhiteWin)
    {
        createLog(TextLog.createGameOverLog(isWhiteWin));
    }

    private static void createTurnStartLog(int currentTurnCount, bool isWhiteTurn)
    {
        createLog(TextLog.createTurnStartLog(isWhiteTurn));
    }

    private static void createCardHoldLog(bool isWhite)
    {
        createLog(new CardHoldLog(isWhite));
    }

    private static void createCardUseLog(bool isWhite, Card card)
    {
        createLog(new CardUseLog(isWhite, card));
    }

    public static string toChessCoord(Vector2Int pos)
    {
        return $"{(char)('A' + pos.x)}{pos.y + 1}";
    }

    private System.Collections.IEnumerator innerScrollToBottom()
    {
        yield return null;

        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void updateUi()
    {
        IEnumerable<string> messages = logQueue.Select(entry => entry.getMessage());
        logText.text = string.Join(LINE_BREAK_SINGLE, messages);

        if (scrollRect != null)
            StartCoroutine(innerScrollToBottom());
    }

    public static void scrollToButtom()
    {
        if (instance.scrollRect != null)
            instance.StartCoroutine(instance.innerScrollToBottom());
    }
}
