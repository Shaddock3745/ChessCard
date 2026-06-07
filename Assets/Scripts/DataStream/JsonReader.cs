using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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
}

[System.Serializable]
public class CardTextList
{
    public List<CardTextData> cards;
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
        }
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

    public static string getCardName(int id)
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE: // Traditional Chinese
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataTw))
                    return cardDataTw.name_tw;
                break;
            case Language.ENGLISH: // English
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataEn))
                    return cardDataEn.name_en;
                break;
            default:
                Debug.LogWarning($"Unsupported language: {GameManager.getCurrentLanguage()}.");
                break;
        }

        return null; // Return null if card not found or unsupported language
    }

    public static string getCardDescription(int id)
    {
        switch (GameManager.getCurrentLanguage())
        {
            case Language.TRADITIONAL_CHINESE: // Traditional Chinese
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataTw))
                    return cardDataTw.desc_tw;
                break;
            case Language.ENGLISH: // English
                if (instance.cardTable.TryGetValue(id, out CardTextData cardDataEn))
                    return cardDataEn.desc_en;
                break;
            default:
                Debug.LogWarning($"Unsupported language: {GameManager.getCurrentLanguage()}.");
                break;
        }
        return null; // Return null if card not found or unsupported language
    }
}
