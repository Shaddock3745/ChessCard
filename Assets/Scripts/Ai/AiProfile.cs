using System.Collections.Generic;

public enum AiPersonality { Balanced = 0, Aggressive = 1, Conservative = 2, Tricky = 3 } // ★ Balanced 新增,當玩家預設
public enum AiDifficulty { Easy = 0, Normal = 1, Hard = 2 }

// 個性 → 餵給評估函數的權重
public struct AiWeights
{
    public float attack;             // 攻擊傾向:威脅敵子、逼宮、吃子收益
    public float defend;             // 防守傾向:保護己方子、護王
    public float mobilityW;          // 機動力每步價值
    public float handCardW;          // 每張手牌的資源價值
    public float ownerReactDiscount; // 「輪到該方」時其棋子受威脅打折(越低越敢拚)
    public float statusWeight;       // buff/debuff 靜態權重
    public float cardPreference;     // 偏好使用卡片(每用一張卡的計畫加成,AiManager 使用)
}

public class AiProfile
{
    // === 各個性牌組(約 25 張,棋子卡 8~10 張)。皆為起手建議,實戰再微調 ===
    private static readonly List<int> BALANCED_DECK = new List<int> {
        1,1,1, 2,2, 3,3, 4, 5,                       // 棋子 9
        6,24, 14,43, 10,31, 7,11, 32,35, 25,28, 18,21, 46,49   // 攻守資源混合 16
    };
    private static readonly List<int> AGGRESSIVE_DECK = new List<int> {
        1,1,1, 2,2, 3,3, 4, 5,                       // 棋子 9
        6,24, 14,43, 34,35, 33,32, 9,25, 36,28, 41,11, 46,49   // 進攻/節奏 16
    };
    private static readonly List<int> CONSERVATIVE_DECK = new List<int> {
        1,1,1,1, 2,2, 3, 4,4, 5,                     // 棋子 10
        6,18,21, 10,31,39,12, 23,26, 13,40,7, 22,38,15         // 防守/資源 15
    };
    private static readonly List<int> TRICKY_DECK = new List<int> {
        1,1, 2,2, 3,3, 4, 5,                         // 棋子 8
        6,9,15,16, 17,36,45,37, 42,19,21,18, 24,27,38,8, 29    // 卡牌連段/操控 17
    };

    private AiPersonality personality;
    private AiDifficulty difficulty;
    private AiWeights weights;

    public AiProfile(AiPersonality personality, AiDifficulty difficulty)
    {
        this.personality = personality;
        this.difficulty = difficulty;
        this.weights = buildWeights(personality);
    }

    private static AiWeights buildWeights(AiPersonality p)
    {
        switch (p)
        {
            case AiPersonality.Aggressive:    // 愛吃子、不太顧自己、敢拚
                return new AiWeights { attack = 1.6f, defend = 0.6f, mobilityW = 3f, handCardW = 12f, ownerReactDiscount = 0.25f, statusWeight = 5f, cardPreference = 0f };
            case AiPersonality.Conservative:  // 重防守、囤資源、不亂換子
                return new AiWeights { attack = 0.7f, defend = 1.6f, mobilityW = 2f, handCardW = 28f, ownerReactDiscount = 0.6f, statusWeight = 5f, cardPreference = 0f };
            case AiPersonality.Tricky:        // 高機動、愛用卡片連段
                return new AiWeights { attack = 1.1f, defend = 0.9f, mobilityW = 5f, handCardW = 24f, ownerReactDiscount = 0.4f, statusWeight = 5f, cardPreference = 10f };
            case AiPersonality.Balanced:
            default:                          // 中庸:各項 1.0,等同你目前調好的基準
                return new AiWeights { attack = 1.0f, defend = 1.0f, mobilityW = 3f, handCardW = 20f, ownerReactDiscount = 0.4f, statusWeight = 5f, cardPreference = 0f };
        }
    }

    public AiWeights getWeights() => weights;

    // 難度 → 分數雜訊範圍(越低難度雜訊越大,AI 越常選非最佳解)
    public float getNoiseRange()
    {
        return difficulty switch
        {
            AiDifficulty.Easy => 1200f,
            AiDifficulty.Normal => 500f,
            AiDifficulty.Hard => 100f,
            _ => 500f
        };
    }

    public AiPersonality getPersonality() => personality;
    public AiDifficulty getDifficulty() => difficulty;

    public List<int> getDefaultDeckList()
    {
        switch (personality)
        {
            case AiPersonality.Aggressive: return new List<int>(AGGRESSIVE_DECK);
            case AiPersonality.Conservative: return new List<int>(CONSERVATIVE_DECK);
            case AiPersonality.Tricky: return new List<int>(TRICKY_DECK);
            case AiPersonality.Balanced:
            default: return new List<int>(BALANCED_DECK);
        }
    }

    public static List<int> getDefaultDeckListOfPersonality(AiPersonality personality)
    {
        switch (personality)
        {
            case AiPersonality.Aggressive: return new List<int>(AGGRESSIVE_DECK);
            case AiPersonality.Conservative: return new List<int>(CONSERVATIVE_DECK);
            case AiPersonality.Tricky: return new List<int>(TRICKY_DECK);
            case AiPersonality.Balanced:
            default: return new List<int>(BALANCED_DECK);
        }
    }
}