using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum AiPersonality { Aggressive, Conservative, Tricky }
public enum AiDifficulty { Easy, Normal, Hard }

public class AiProfile
{
    private static readonly List<int> DEFAULT_AGGRESSIVE_DECK = new List<int>
    { 
        // ★ 棋子卡 (10張)：偏重高機動與高擊殺威脅的棋子
        1, 1, 1, 1,       // 士兵 x4 (前線砲灰與強襲基礎)
        3, 3, 3,          // 騎士 x3 (越子奇襲、強力踩子核心)
        5, 5, 5,          // 皇后 x3 (絕對的盤面壓制力)

        // ★ 功能卡 (17張)
        11, 11,           // 亢奮 x2 (強化士兵機動突擊)
        14, 14,           // 愈戰愈勇 x2 (士兵連環吃子大解場)
        18, 18,           // 施術準備 x2 (過牌維持進攻資源)
        25, 25,           // 千里馬 x2 (騎士無消耗二連跳)
        30,               // 魔力爆發 x1 (極限回合全手牌 0 消耗傾瀉)
        32, 32,           // 正面突破 x2 (解鎖士兵正前方擊殺能力)
        34, 34,           // 踐踏 x2 (騎士衝鋒大範圍清場)
        43, 43,           // 大軍壓境 x2 (全體士兵零消耗群體推進)
        49, 49            // 升變 - 皇后 x2 (突進底線後迅速轉化為終結比賽的皇軍)
    };

    private static readonly List<int> DEFAULT_CONSERVATIVE_DECK = new List<int>
    { 
        // ★ 棋子卡 (10張)：以陣地戰、區域防守為核心的重型配置
        1, 1, 1,          // 士兵 x3 (基礎防線防護)
        2, 2,             // 主教 x2 (遠程斜線牽制)
        4, 4, 4, 4,       // 城堡 x4 (構築鋼鐵防線、陣地戰核心)
        5,                // 皇后 x1 (後方保底重火力)

        // ★ 功能卡 (17張)
        7, 7,             // 王車易位 x2 (國王緊急避險與城堡重定位)
        10, 10,           // 護盾 x2 (劣勢時保護核心防禦棋子)
        12, 12,           // 神聖加護 x2 (主教神聖領域，大範圍群體免疫選取)
        18, 18,           // 施術準備 x2 (穩定過牌尋找防守解牌)
        23, 23,           // 停戰協議 x2 (強行拖延對手進攻節奏，極致龜縮)
        26,               // 大飢荒 x1 (封鎖雙方召喚，打斷對手鋪場攻勢)
        31, 31,           // 防禦陣型 x2 (士兵手牽手群體無法被攻擊)
        39, 39,           // 堅固壁壘 x2 (以城堡為中心的 8 格絕對庇護所)
        40, 40            // 割讓土地 x2 (城堡退回原點換取高額 AP 補給)
    };

    private static readonly List<int> DEFAULT_TRICKY_DECK = new List<int>
    { 
        // ★ 棋子卡 (11張)：高彈性、注重功能性配對與資源回收的棋子
        1, 1, 1, 1, 1,    // 士兵 x5 (用於資源獻祭與多路線升變)
        2, 2, 2,          // 主教 x3 (黑白格資源戰與戰術干擾)
        3, 3,             // 騎士 x2 (點對點功能性切入)
        5,                // 皇后 x1 (逼迫對手走位的戰術威懾)

        // ★ 功能卡 (16張)
        6, 6,             // 徵兵令 x2 (補足砲灰，空手套白狼)
        9,                // 咆嘯 x1 (打亂敵方陣型，強制位移)
        15, 15,           // 遣返 x2 (將敵方突進的核心高費棋子直接踢回老家)
        16, 16,           // 鐐銬 x2 (鎖定並癱瘓敵方高價值個體)
        17,               // 一換一 x1 (用我方爛位置棋子強制兌掉敵方等價核心)
        19, 19,           // 回收 x2 (無限續航，無限循環打出的惡意資源)
        21,               // 再填充 x1 (手牌太爛時全洗回牌組尋找新策略)
        36,               // 招降 x1 (主教擊殺順便白嫖對手棋子到手牌)
        45, 45,           // 打亂 x2 (摧毀對手手牌上限與保留區，極度噁心對手)
        46,               // 升變 - 騎士 x1 (依據局勢靈活升變為控制點)
        48                // 升變 - 主教 x1 (依據局勢靈活升變為遠程干擾點)
    };

    private AiPersonality personality;
    private AiDifficulty difficulty;

    // 權重參數
    private float attackWeight;
    private float protectWeight;
    private float mobilityWeight;
    private float resourceWeight; // AP與手牌保留的權重
    private float cardPreference; // 對於使用卡片的偏好(奇招個性會較高)

    public AiProfile(AiPersonality personality, AiDifficulty difficulty)
    {
        this.personality = personality;
        this.difficulty = difficulty;

        // 依據個性給予不同權重
        switch (personality)
        {
            case AiPersonality.Aggressive: // 激進：愛吃子、不愛防守、不在乎資源
                attackWeight = 1.5f; protectWeight = 0.5f; mobilityWeight = 1.0f; resourceWeight = 0.5f; cardPreference = 1.0f;
                break;
            case AiPersonality.Conservative: // 保守：重防守、愛存AP與手牌、不隨便攻擊
                attackWeight = 0.8f; protectWeight = 1.5f; mobilityWeight = 0.8f; resourceWeight = 1.5f; cardPreference = 0.8f;
                break;
            case AiPersonality.Tricky: // 奇招：極度依賴卡片效果與高機動性
                attackWeight = 1.0f; protectWeight = 0.8f; mobilityWeight = 1.5f; resourceWeight = 0.8f; cardPreference = 2.0f;
                break;
        }
    }

    public float getNoiseRange()
    {
        return difficulty switch
        {
            AiDifficulty.Easy => 20f,
            AiDifficulty.Normal => 10f,
            AiDifficulty.Hard => 1f,
            _ => 10f
        };
    }

    public AiPersonality getPersonality() => personality;

    public AiDifficulty getDifficulty() => difficulty;

    public float getAttackWeight() => attackWeight;

    public float getProtectWeight() => protectWeight;

    public float getResourceWeight() => resourceWeight;

    public float getMobilityWeight() => mobilityWeight;

    public float getCardPreference() => cardPreference;

    public List<int> getDefaultDeckList()
    {
        switch (personality)
        {
            case AiPersonality.Conservative:
                return DEFAULT_CONSERVATIVE_DECK;
            case AiPersonality.Aggressive:
                return DEFAULT_AGGRESSIVE_DECK;
            case AiPersonality.Tricky:
                return DEFAULT_TRICKY_DECK;
            default:
                return null;
        }
    }
}