using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private static PlayerDataManager instance;

    private static readonly List<int> DEFAULT_BALANCED_DECK = AiProfile.getDefaultDeckListOfPersonality(AiPersonality.Balanced);
    private static readonly List<int> TESTING_DECK = new List<int> { 6 };

    private AiPersonality enemyAiPersonality = AiPersonality.Conservative;
    private AiDifficulty enemyAiDifficulty = AiDifficulty.Easy;

    private List<int> playerDeck = DEFAULT_BALANCED_DECK;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            loadData();
        }
        else
            Destroy(gameObject);
    }

    private void loadData()
    {
        playerDeck = JsonReader.loadPlayDeck();
        enemyAiDifficulty = JsonReader.loadAiDifficulty();
        enemyAiPersonality = JsonReader.loadAiPersonality();
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
