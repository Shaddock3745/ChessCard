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

public class CardDatabaseManager : MonoBehaviour
{
    private static CardDatabaseManager instance;
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
}
