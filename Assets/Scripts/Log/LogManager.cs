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

    public static string rigisterLogData(object data)
    {
        if (instance == null || data == null)
            return "";

        string id = System.Guid.NewGuid().ToString();
        instance.linkMap.Add(id, data);

        return id;
    }

    public static void createLog(LogData data)
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

    public static string toChessCoord(Vector2Int pos)
    {
        return $"{(char)('A' + pos.x)}{pos.y + 1}";
    }

    private System.Collections.IEnumerator scrollToBottom()
    {
        yield return null;

        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void updateUi()
    {
        IEnumerable<string> messages = logQueue.Select(entry => entry.getMessage());
        logText.text = string.Join(LINE_BREAK_SINGLE, messages);

        if (scrollRect != null)
            StartCoroutine(scrollToBottom());
    }
}
