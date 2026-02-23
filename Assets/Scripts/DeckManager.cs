using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    private static DeckManager instance;

    [SerializeField] private TMPro.TextMeshProUGUI deckCount, waitingCount, discardCount;

    private List<Card> deck, handList, waitingList, discardList;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            setDeck(PlayerDataManager.getPlayerDeck());
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

    public static void shuffleDeck()
    {
        for (int i = 0; i < instance.deck.Count; i++)
        {
            int randomIndex = Random.Range(i, instance.deck.Count);
            Card temp = instance.deck[i];
            instance.deck[i] = instance.deck[randomIndex];
            instance.deck[randomIndex] = temp;
        }
    }

    private static void replenish()
    {
        instance.deck.AddRange(instance.waitingList);
        instance.waitingList.Clear();
        shuffleDeck();
    }

    public static Card drawCard()
    {
        Card result = null;

        if (instance.deck.Count > 0)
            result = instance.deck[instance.deck.Count - 1];
        else if (instance.waitingList.Count > 0)
        {
            replenish();
            result = instance.deck[instance.deck.Count - 1];
        }

        if (result != null)
        {
            instance.deck.RemoveAt(instance.deck.Count - 1);
            instance.handList.Add(result);
            HandViewManager.drawCard(result);
            instance.updateUI();
        }

        return result;
    }

    public static void flush()
    {
        foreach (Card card in instance.handList)
            instance.waitingList.Add(card);

        instance.handList.Clear();
        HandViewManager.flush();

        instance.updateUI();
    }

    private void updateUI()
    {
        deckCount.text = instance.deck.Count.ToString();
        waitingCount.text = instance.waitingList.Count.ToString();
        discardCount.text = instance.discardList.Count.ToString();
    }

    public static void setDeck(List<int> cardIds)
    {
        instance.deck = new List<Card>();
        instance.handList = new List<Card>();
        instance.waitingList = new List<Card>();
        instance.discardList = new List<Card>();

        foreach (int id in cardIds)
            instance.deck.Add(new Card(id));

        shuffleDeck();
        instance.updateUI();
    }

    public static List<Card> getDeck()
    {
        return instance.deck;
    }

    public static List<Card> getHandList()
    {
        return instance.handList;
    }

    public static List<Card> getWaitingList()
    {
        return instance.waitingList;
    }

    public static List<Card> getDiscardList()
    {
        return instance.discardList;
    }
}
