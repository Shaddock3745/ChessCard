using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;

[System.Serializable]
public class CardTextData
{
    public int id;
    public int cost;
    public string name_tw;
    public string desc_tw;
    public string name_en;
    public string desc_en;
    public string name_ja;
    public string desc_ja;
}

[System.Serializable]
public class CardTextList
{
    public List<CardTextData> cards;
}

[System.Serializable]
public class PlayerStaticData
{
    public List<int> playDeck = new List<int>();
    public Language language = Language.TRADITIONAL_CHINESE;
    public AiPersonality aiPersonality = AiPersonality.Balanced;
    public AiDifficulty aiDifficulty = AiDifficulty.Normal;
}

public class JsonReader : MonoBehaviour
{
    private static JsonReader instance;
    private Dictionary<int, CardTextData> cardTable = new Dictionary<int, CardTextData>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            loadCardData();
            ensurePlayerDataExists();
        }
        else
            Destroy(gameObject);
    }

    private void loadCardData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("CardList");

        try
        {
            List<CardTextData> list = JsonConvert.DeserializeObject<List<CardTextData>>(jsonFile.text);
            cardTable = list.ToDictionary(card => card.id);
        }
        catch (JsonException e)
        {
            Debug.LogError($"Failed to parse card data: {e.Message}");
            cardTable = new Dictionary<int, CardTextData>();
        }
    }

    public static List<int> getCardIdList()
    {
        return instance.cardTable.Keys.ToList();
    }

    public static int getCardCost(int id)
    {
        if (instance.cardTable.TryGetValue(id, out CardTextData cardData))
            return cardData.cost;
        else
        {
            Debug.LogWarning($"Card ID {id} not found in database.");
            return -1; // Return -1 to indicate an error
        }
    }

    public static string getCardName(int id, Language language)
    {
        switch (language)
        {
            case Language.TRADITIONAL_CHINESE: // Traditional Chinese
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataTw))
                    return cardDataTw.name_tw;
                break;
            case Language.ENGLISH: // English
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataEn))
                    return cardDataEn.name_en;
                break;
            case Language.JAPANESE: // Japanese
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataJa))
                    return cardDataJa.name_ja;
                break;
            default:
                Debug.LogWarning($"Unsupported language: {language}.");
                break;
        }

        return null; // Return null if card not found or unsupported language
    }

    public static string getCardDescription(int id, Language language)
    {
        switch (language)
        {
            case Language.TRADITIONAL_CHINESE: // Traditional Chinese
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataTw))
                    return cardDataTw.desc_tw;
                break;
            case Language.ENGLISH: // English
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataEn))
                    return cardDataEn.desc_en;
                break;
            case Language.JAPANESE: // Japanese
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataJa))
                    return cardDataJa.desc_ja;
                break;
            default:
                Debug.LogWarning($"Unsupported language: {language}.");
                break;
        }
        return null; // Return null if card not found or unsupported language
    }

    private static string getPlayerDataPath()
    {
        return Path.Combine(Application.persistentDataPath, "PlayerData.json");
    }

    private static void ensurePlayerDataExists()
    {
        if (File.Exists(getPlayerDataPath()))
            return;

        PlayerStaticData data = new PlayerStaticData();
        data.playDeck = AiProfile.getDefaultDeckListOfPersonality(AiPersonality.Balanced);
        writePlayerData(data);
    }

    private static PlayerStaticData loadPlayerData()
    {
        string path = getPlayerDataPath();

        if (!File.Exists(path))
            return new PlayerStaticData();

        try
        {
            string json = File.ReadAllText(path);
            PlayerStaticData data = JsonConvert.DeserializeObject<PlayerStaticData>(json);
            return data ?? new PlayerStaticData();
        }
        catch (JsonException e)
        {
            Debug.LogError($"Failed to parse player data: {e.Message}");
            return new PlayerStaticData();
        }
    }

    // 把整份 PlayerData 寫回檔案
    private static void writePlayerData(PlayerStaticData data)
    {
        string path = getPlayerDataPath();

        try
        {
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write player data: {e.Message}");
        }
    }

    // 存牌組（先讀回現有資料，只改 playDeck，避免蓋掉 language）
    public static void savePlayDeck(List<int> cardList)
    {
        PlayerStaticData data = loadPlayerData();
        data.playDeck = cardList ?? new List<int>();
        writePlayerData(data);
    }

    // 讀牌組
    public static List<int> loadPlayDeck()
    {
        return loadPlayerData().playDeck;
    }

    // 存使用者選擇的語言（只改 language，不動 playDeck）
    public static void saveLanguage(Language language)
    {
        PlayerStaticData data = loadPlayerData();
        data.language = language;
        writePlayerData(data);
    }

    // 讀使用者選擇的語言
    public static Language loadLanguage()
    {
        return loadPlayerData().language;
    }

    public static void saveAiPersonality(AiPersonality personality)
    {
        PlayerStaticData data = loadPlayerData();
        data.aiPersonality = personality;
        writePlayerData(data);
    }

    public static AiPersonality loadAiPersonality()
    {
        return loadPlayerData().aiPersonality;
    }

    // 存敵人 AI 難度
    public static void saveAiDifficulty(AiDifficulty difficulty)
    {
        PlayerStaticData data = loadPlayerData();
        data.aiDifficulty = difficulty;
        writePlayerData(data);
    }

    public static AiDifficulty loadAiDifficulty()
    {
        return loadPlayerData().aiDifficulty;
    }
}
