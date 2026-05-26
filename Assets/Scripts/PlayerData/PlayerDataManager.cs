using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    private static PlayerDataManager instance;

    private List<int> playerDeck = new List<int>(new int[] { 15, 15, 30, 5, 5, 5 });

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

    public static PlayerData initLocalPlayerData(bool isAI, bool isWhite) => new PlayerData(isAI, true, isWhite, getPlayerDeck());
}
