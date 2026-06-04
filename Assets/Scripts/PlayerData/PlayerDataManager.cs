using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private static PlayerDataManager instance;

    private static readonly List<int> DEFAULT_BALANCED_DECK = new List<int>
    {
        // ★ 棋子卡 (10張)：標準西洋棋配置，兼顧兵力鋪場、中場機動與後方防守
        1, 1, 1, 1,       // 士兵 x4
        2, 2,             // 主教 x2
        3, 3,             // 騎士 x2
        4,                // 城堡 x1
        5,                // 皇后 x1

        // ★ 功能卡 (17張)：涵蓋過牌、位移、防守與升變，能靈活應對各種常規局勢
        7,                // 王車易位 x1 (國王基本防禦與保險)
        9,                // 咆嘯 x1 (戰線推擠與陣型破壞)
        10,               // 護盾 x1 (關鍵棋子被動保護)
        11,               // 亢奮 x1 (強化士兵突擊機動)
        15,               // 遣返 x1 (軟解敵方前線高威脅單位)
        18, 18,           // 施術準備 x2 (穩定補充手牌資源)
        21,               // 再填充 x1 (手牌不順時的重洗機制)
        24,               // 求援 x1 (從牌組定向隨機檢索棋子)
        28,               // 捨身取義 x1 (將殘血或卡位棋子轉化為 AP 與資源)
        32,               // 正面突破 x1 (修正士兵攻擊盲區)
        35,               // 回馬槍 x1 (騎士突襲吃子後安全回撤)
        37,               // 宗教改革 x1 (主教十字方向微調位移)
        44, 44,           // 雙管齊下 x2 (提供全場棋子高彈性的額外步數)
        47,               // 升變 - 城堡 x1 (底線士兵轉化為穩健防線)
        49                // 升變 - 皇后 x1 (底線士兵轉化為終結比賽的進攻點)
    };

    private AiPersonality enemyAiPersonality = AiPersonality.Aggressive;
    private AiDifficulty enemyAiDifficulty = AiDifficulty.Hard;

    private List<int> playerDeck = DEFAULT_BALANCED_DECK;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static List<int> getPlayerDeck()
    {
        return instance.playerDeck;
    }

    public static PlayerData initLocalPlayerData(bool isAI, bool isWhite) => new PlayerData(true, isWhite, getPlayerDeck(), isAI);

    public static AiPersonality getEnemyAiPersonality() => instance.enemyAiPersonality;

    public static AiDifficulty getEnemyAiDifficulty() => instance.enemyAiDifficulty;
}
