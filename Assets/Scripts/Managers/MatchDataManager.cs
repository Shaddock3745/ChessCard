using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MatchDataManager : MonoBehaviour
{
    private static MatchDataManager instance;

    [SerializeField] private TMPro.TextMeshProUGUI deckCount, waitingCount, discardCount, manaCount;

    private List<Card> deck, handList, waitingList, discardList;
    private int mana = DEFAULT_MANA;
    private bool chessMoved = false;
    private bool couldntMoveChessThisTurn = false;
    private int bonusMana = 0, bonusHandCardCount = 0;

    private static readonly int DEFAULT_MANA = 5, DEFAULT_HAND_CARD_COUNT = 5;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            loadDeck(PlayerDataManager.getPlayerDeck());
            updateManaUi();
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
            instance.updateCardCountUi();
        }

        return result;
    }

    public static bool useCard(Card card)
    {
        if (isManaEnough(card.getCost()))
        {
            consumeMana(card.getCost());
            instance.handList.Remove(card);
            CardEffectManager.processCardEffect(card);

            if (card.isExileOnUseCard())
                instance.discardList.Add(card);

            instance.updateCardCountUi();
            return true;
        }

        return false;
    }

    public static void flushHandCards()
    {
        foreach (Card card in instance.handList)
            instance.waitingList.Add(card);

        instance.handList.Clear();
        HandViewManager.flush();

        instance.updateCardCountUi();
    }

    private void updateCardCountUi()
    {
        deckCount.text = instance.deck.Count.ToString();
        waitingCount.text = instance.waitingList.Count.ToString();
        discardCount.text = instance.discardList.Count.ToString();
    }

    private void updateManaUi()
    {
        manaCount.text = instance.mana.ToString();
    }

    public static void loadDeck(List<int> cardIds)
    {
        instance.deck = new List<Card>();
        instance.handList = new List<Card>();
        instance.waitingList = new List<Card>();
        instance.discardList = new List<Card>();

        foreach (int id in cardIds)
            instance.deck.Add(new Card(id));

        shuffleDeck();
        instance.updateCardCountUi();
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

    public static int getMana()
    {
        return instance.mana;
    }

    public static bool isManaEnough(int cost)
    {
        return instance.mana >= cost;
    }

    public static void consumeMana(int cost)
    {
        instance.mana -= cost;

        instance.updateManaUi();
    }

    public static bool canMoveChess(ChessPiece chess)
    {
        return !instance.couldntMoveChessThisTurn && !instance.chessMoved && instance.mana >= (1 + chess.getExtraMoveCost());
    }

    public static bool isCouldntMoveChessThisTurn()
    {
        return instance.couldntMoveChessThisTurn;
    }

    public static void resetBonusMana()
    {
        instance.bonusMana = 0;
    }

    public static void resetBonusHandCardCount()
    {
        instance.bonusHandCardCount = 0;
    }

    public static void addBonusMana(int amount)
    {
        instance.bonusMana += amount;
    }

    public static void addBonusHandCardCount(int amount)
    {
        instance.bonusHandCardCount += amount;
    }

    public static void setCouldntMoveChessThisTurn(bool value)
    {
        instance.couldntMoveChessThisTurn = value;
    }

    public static void turnStart()
    {
        instance.mana = DEFAULT_MANA + instance.bonusMana;
        instance.chessMoved = false;

        int handCardCount = DEFAULT_HAND_CARD_COUNT + instance.bonusHandCardCount;
        
        for (int i = 0; i < handCardCount; i++)
            drawCard();

        instance.updateCardCountUi();
        instance.updateManaUi();
    }

    public static void turnEnd()
    {
        resetBonusHandCardCount();
        resetBonusMana();
        flushHandCards();
        instance.updateCardCountUi();
        instance.updateManaUi();
    }

    public static void setChessMoved(ChessPiece chess)
    {
        instance.chessMoved = true;
        instance.mana -= 1 + chess.getExtraMoveCost();
        instance.updateManaUi();
    }
}