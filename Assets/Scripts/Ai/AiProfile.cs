using UnityEngine;

public enum AiPersonality { Aggressive, Conservative, Tricky }
public enum AiDifficulty { Easy, Normal, Hard }

public class AiProfile
{
    public AiPersonality personality;
    public AiDifficulty difficulty;

    // 權重參數
    public float attackWeight;
    public float protectWeight;
    public float mobilityWeight;
    public float resourceWeight; // AP與手牌保留的權重
    public float cardPreference; // 對於使用卡片的偏好(奇招個性會較高)

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
}