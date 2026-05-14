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
    private bool chessMoved = false;

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

    public bool canMoveChessThisTurn(ChessPiece chess, int currentTurnCount)
    {
        return !chessMoved && chess.isWhite() == isWhite && statusContainer.canMoveChess(chess.getChessType(), currentTurnCount) && chess.canMove(currentTurnCount)
            && mana >= chess.getMoveCost(currentTurnCount) + getExtraMoveCost(currentTurnCount);
    }

    public bool canAttackGloballyThisTurn(int currentTurnCount)
    {
        return !chessMoved && statusContainer.canAttack(currentTurnCount);
    }

    public bool canAttackChessThisTurn(ChessPiece attacker, ChessPiece target, int currentTurnCount)
    {
        return canMoveChessThisTurn(attacker, currentTurnCount) && canAttackGloballyThisTurn(currentTurnCount) && attacker.canAttack(currentTurnCount) && target.canBeAttacked(currentTurnCount);
    }

    public bool chessMovedThisTurn() => chessMoved;

    public bool canSummonThisTurn(int currentTurnCount) => statusContainer.canSummon(currentTurnCount);

    public bool canUseCard(Card card, int currentTurnCount)
    {
        return mana >= (card.getCost() + getExtraCardCost(currentTurnCount)) && CardEffectManager.canActivateCard(isLocal, card, currentTurnCount);
    }

    public int getExtraMoveCost(int currentTurnCount)
    {
        return statusContainer.getGlobalExtraMoveCost(currentTurnCount);
    }

    public int getExtraCardCost(int currentTurnCount)
    {
        return statusContainer.getGlobalExtraCardCost(currentTurnCount);
    }

    public void doChessMoved(ChessPiece chess, int currentTurnCount)
    {
        chessMoved = true;
        mana -= chess.getMoveCost(currentTurnCount) + statusContainer.getGlobalExtraMoveCost(currentTurnCount);
    }

    public void resetForNewTurn(int currentTurnCount)
    {
        mana = DEFAULT_MANA + statusContainer.getBonusManaCount(currentTurnCount);
        chessMoved = false;
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
