using System.Collections.Generic;
using UnityEngine;

enum PlayerType
{
    Human,
    AI
}

public class PlayerData
{
    private static readonly int DEFAULT_MANA = 5, DEFAULT_HAND_CARD_COUNT = 5;

    private readonly PlayerType playerType;
    private readonly bool isWhite;
    private readonly bool isLocal;
    private PlayerStatusContainer statusContainer = new PlayerStatusContainer();
    private List<Card> deck, handList, waitingList, discardList;
    private int mana = DEFAULT_MANA;
    private int moveCount = 0;

    public static int getDefaultMana()
    {
        return DEFAULT_MANA;
    }

    public static int getDefaultHandCardCount()
    {
        return DEFAULT_HAND_CARD_COUNT;
    }

    public PlayerData(bool isAI, bool isLocal, bool isWhite, List<int> cardIds)
    {
        playerType = isAI ? PlayerType.AI : PlayerType.Human;
        this.isLocal = isLocal;
        this.isWhite = isWhite;
        loadDeck(cardIds);
    }

    public bool isAI()
    {
        return playerType == PlayerType.AI;
    }

    public bool IsWhite() => isWhite;

    public bool IsLocal() => isLocal;

    public int getMana() => mana;

    public void addMana(int amount)
    {
        mana += amount;
    }

    public int getHandCardCount() => handList.Count;

    public int getDeckCardCount() => deck.Count;

    public int getBonusHandCardCount(int currentTurnCount) => statusContainer.getBonusHandCardCount(currentTurnCount);

    public int getBonusManaCount(int currentTurnCount) => statusContainer.getBonusManaCount(currentTurnCount);

    public int getWaitingCardCount() => waitingList.Count;

    public int getDiscardCardCount() => discardList.Count;

    public List<Card> getDeck() => deck;

    public List<Card> getHandList() => handList;

    public List<Card> getWaitingList() => waitingList;

    public List<Card> getDiscardList() => discardList;

    private void consumeMana(int cost)
    {
        mana -= cost;
    }

    public int getMoveCount() => moveCount;

    public PlayerStatusContainer getStatusContainer() => statusContainer;

    public void doChessMoved(ChessPiece chess, int currentTurnCount)
    {
        moveCount++;
        mana -= chess.getMoveCost(currentTurnCount) + statusContainer.getGlobalExtraMoveCost(currentTurnCount);
    }

    public void resetForNewTurn(int currentTurnCount)
    {
        mana = DEFAULT_MANA + statusContainer.getBonusManaCount(currentTurnCount);
        moveCount = 0;
    }

    public void resetForEndTurn(int currentTurnCount)
    {

    }

    private void loadDeck(List<int> cardIds)
    {
        deck = new List<Card>();
        handList = new List<Card>();
        waitingList = new List<Card>();
        discardList = new List<Card>();

        foreach (int id in cardIds)
            deck.Add(new Card(id));

        shuffleDeck();
    }

    public void shuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            Card temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void replenish()
    {
        deck.AddRange(waitingList);
        waitingList.Clear();
        shuffleDeck();
    }

    public Card drawCard()
    {
        Card result = null;

        if (deck.Count > 0)
            result = deck[deck.Count - 1];
        else if (waitingList.Count > 0)
        {
            replenish();
            result = deck[deck.Count - 1];
        }

        if (result != null)
        {
            deck.RemoveAt(deck.Count - 1);
            handList.Add(result);
        }

        return result;
    }

    public Card materializeCard(int cardId, bool noConsumption)
    {
        Card result = new Card(cardId, true, noConsumption);
        handList.Add(result);
        return result;
    }

    public void useCard(Card card)
    {
        consumeMana(card.getCost());
        handList.Remove(card);

        if (!card.isExileOnUseCard())
            discardList.Add(card);
    }

    public void flushHandCards()
    {
        foreach (Card card in handList)
            waitingList.Add(card);

        handList.Clear();
    }

    public void addStatus(PlayerStatus status)
    {
        statusContainer.addStatus(status);
    }

    public void cleanUpExpiredStatuses(int currentTotalTurn)
    {
        statusContainer.cleanUpExpiredStatuses(currentTotalTurn);
    }

    public void onGameOver()
    {
        statusContainer.Dispose();
    }
}
