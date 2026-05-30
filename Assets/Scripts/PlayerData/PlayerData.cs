using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum PlayerType
{
    Human,
    AI
}

public class PlayerData
{
    private static readonly int DEFAULT_MANA = 5, DEFAULT_HAND_CARD_COUNT = 5, DEFAULT_HOLD_COST = 1;

    private readonly PlayerType playerType;
    private readonly bool isWhite;
    private readonly bool isLocal;
    private PlayerStatusContainer statusContainer = new PlayerStatusContainer();
    private List<Card> deck, handList, waitingList, discardList, holdingList;
    private int mana = DEFAULT_MANA;
    private List<ActionRecord> chessMoveRecords = new List<ActionRecord>();

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

    public bool isCardNonCost(int curretnTurnCount) => statusContainer.isCardNonCost(curretnTurnCount);

    public bool isDiscardAllHandCardOnTurnEnd(int curretnTurnCount) => statusContainer.isDiscardAllHandCardOnTurnEnd(curretnTurnCount);

    public int getMana() => mana;

    public void addMana(int amount)
    {
        mana += amount;
    }

    public int getHandCardCount() => handList.Count;

    public int getDeckCardCount() => deck.Count;

    public int getHoldCardCount() => holdingList.Count;

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

    public int getMoveCount() => chessMoveRecords.Count;

    public PlayerStatusContainer getStatusContainer() => statusContainer;

    public List<ActionRecord> getChessMoveRecords() => chessMoveRecords;

    public int getPieceMoveCount(ChessPiece piece)
    {
        return chessMoveRecords.Count(record => record.piece == piece);
    }

    public void doChessMoved(ChessPiece chess, Vector2Int from, Vector2Int to, ActionType actionType, int currentTurnCount)
    {
        chessMoveRecords.Add(new ActionRecord(chess, from, to, actionType));
        mana -= chess.getMoveCost(currentTurnCount) + statusContainer.getGlobalExtraMoveCost(currentTurnCount);
    }

    public void resetForNewTurn(int currentTurnCount)
    {
        mana = DEFAULT_MANA + statusContainer.getBonusManaCount(currentTurnCount);
        chessMoveRecords.Clear();
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
        holdingList = new List<Card>();

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

        if (holdingList.Count > 0)
        {
            result = holdingList[0];
            holdingList.Remove(result);
            handList.Add(result);
        }
        else
        {
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
        }

        return result;
    }

    public Card materializeCard(int cardId, bool noConsumption)
    {
        Card result = new Card(cardId, true, noConsumption);
        handList.Add(result);
        return result;
    }

    public Card retrieveCard(Card card)
    {
        if (!discardList.Contains(card))
            return null;

        handList.Add(card);
        discardList.Remove(card);

        return card;
    }

    public void retrieveAllHoldingCard()
    {
        foreach (Card card in holdingList)
            deck.Add(card);

        holdingList.Clear();
    }

    public bool canHoldCard()
    {
        return handList.Count > 0 && mana >= DEFAULT_HOLD_COST;
    }

    public Card holdCard(Card card)
    {
        if (!handList.Contains(card))
            return null;

        holdingList.Add(card);
        handList.Remove(card);
        consumeMana(DEFAULT_HOLD_COST);

        return card;
    }

    public Card rechargeCard(Card card)
    {
        if (!handList.Contains(card))
            return null;

        deck.Add(card);
        handList.Remove(card);

        return card;
    }

    public Card drawRandomCardFromDeck(int[] validCardIds)
    {
        List<Card> validCards = new List<Card>();
        HashSet<int> validIdSet = new HashSet<int>(validCardIds);

        foreach (Card card in deck)
            if (validIdSet.Contains(card.getId()))
                validCards.Add(card);

        if (validCards.Count == 0 && waitingList.Count > 0)
        {
            replenish();
            foreach (Card card in deck)
                if (validIdSet.Contains(card.getId()))
                    validCards.Add(card);
        }

        if (validCards.Count == 0)
            return null;

        Card targetCard = validCards[Random.Range(0, validCards.Count)];

        deck.Remove(targetCard);
        handList.Add(targetCard);

        return targetCard;
    }

    public bool hasValidCardInDeckOrWaiting(int[] validCardIds)
    {
        HashSet<int> validIdSet = new HashSet<int>(validCardIds);

        foreach (Card card in deck)
            if (validIdSet.Contains(card.getId()))
                return true;

        foreach (Card card in waitingList)
            if (validIdSet.Contains(card.getId()))
                return true;

        return false;
    }

    public void useCard(Card card, int currentTurnCount)
    {
        if (!isCardNonCost(currentTurnCount))
            consumeMana(card.getCost());

        handList.Remove(card);

        if (!card.isExileOnUseCard())
            discardList.Add(card);
    }

    public void discardAllHandCards()
    {
        foreach (Card card in handList)
            discardList.Add(card);

        handList.Clear();
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

    public int getExtraMoveCost(int currentTurnCount)
    {
        return statusContainer.getGlobalExtraMoveCost(currentTurnCount);
    }

    public int getExtraCardCost(int currentTurnCount)
    {
        return statusContainer.getGlobalExtraCardCost(currentTurnCount);
    }
}
