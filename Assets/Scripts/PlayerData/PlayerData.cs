using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

enum PlayerType
{
    Human,
    AI
}

public class PlayerData
{
    private static readonly int DEFAULT_MANA = 5, DEFAULT_HAND_CARD_COUNT = 5, DEFAULT_HOLD_COST = 1;
    private static readonly AiProfile DEFAULT_AI_PROFILE = new AiProfile(AiPersonality.Balanced, AiDifficulty.Easy);
    private static readonly System.Threading.ThreadLocal<System.Random> THREAD_SAVE_RNG = new System.Threading.ThreadLocal<System.Random>(() => new System.Random());

    private readonly bool isVirtual;
    private readonly PlayerType playerType;
    private readonly bool isWhite;
    private readonly bool isLocal;
    private int mana = DEFAULT_MANA;
    private AiProfile aiProfile;
    private PlayerStatusContainer statusContainer = new PlayerStatusContainer();
    private List<Card> deck, handList, waitingList, discardList, holdingList;
    private List<ActionRecord> chessMoveRecords = new List<ActionRecord>();

    public static int getDefaultMana()
    {
        return DEFAULT_MANA;
    }

    public static int getDefaultHandCardCount()
    {
        return DEFAULT_HAND_CARD_COUNT;
    }

    public PlayerData(bool isLocal, bool isWhite, List<int> cardIds, bool isAI, bool isVirtual = false)
    {
        playerType = isAI ? PlayerType.AI : PlayerType.Human;
        aiProfile = DEFAULT_AI_PROFILE;
        this.isLocal = isLocal;
        this.isWhite = isWhite;
        this.isVirtual = isVirtual;
        loadDeck(cardIds);
    }

    public PlayerData(bool isLocal, bool isWhite, List<int> cardIds, bool isAI, AiProfile aiProfile, bool isVirtual = false)
    {
        playerType = isAI ? PlayerType.AI : PlayerType.Human;
        this.aiProfile = aiProfile;
        this.isLocal = isLocal;
        this.isWhite = isWhite;
        this.isVirtual = isVirtual;
        loadDeck(cardIds);
    }

    private PlayerData(PlayerData source)
    {
        this.isVirtual = true;
        this.playerType = source.playerType;
        this.isWhite = source.isWhite;
        this.isLocal = source.isLocal;
        this.mana = source.mana;
        this.aiProfile = source.aiProfile; // AiProfile 為唯讀設定，淺拷貝即可

        // 注意：PlayerStatusContainer 也必須在內部實作 Clone()
        this.statusContainer = source.statusContainer.Clone();

        // 牌庫深度複製
        this.deck = source.deck.Select(c => c.Clone()).ToList();
        this.handList = source.handList.Select(c => c.Clone()).ToList();
        this.waitingList = source.waitingList.Select(c => c.Clone()).ToList();
        this.discardList = source.discardList.Select(c => c.Clone()).ToList();
        this.holdingList = source.holdingList.Select(c => c.Clone()).ToList();

        // 紀錄拷貝 (若 ActionRecord 為 struct，使用 List 建構子即可深拷貝；若為 class，則需 Select(r => r.Clone()))
        this.chessMoveRecords = new List<ActionRecord>(source.chessMoveRecords);
    }

    // 對外開放的深度複製方法
    public PlayerData Clone()
    {
        return new PlayerData(this);
    }

    public bool IsVirtual() => isVirtual;

    public bool isAI()
    {
        return playerType == PlayerType.AI;
    }

    public float getStatusContainerAiValue(int currentTurnCount)
    {
        return statusContainer.getStatusContainerAiValue(isLocal, currentTurnCount);
    }

    public AiProfile getAiProfile()
    {
        //if (playerType != PlayerType.AI)
        //    throw new System.Exception("Wrong Player Type!");

        return aiProfile;
    }

    public AiPersonality getAiPersonality()
    {
        //if (playerType != PlayerType.AI)
        //    throw new System.Exception("Wrong Player Type!");

        return aiProfile.getPersonality();
    }

    public AiDifficulty getAiDifficulty()
    {
        //if (playerType != PlayerType.AI)
        //    throw new System.Exception("Wrong Player Type!");

        return aiProfile.getDifficulty();
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

    public List<Card> getHoldingList() => holdingList;

    public List<Card> getWaitingList() => waitingList;

    public List<Card> getDiscardList() => discardList;

    private void consumeMana(int cost)
    {
        mana -= cost;
    }

    public bool canSummon(int currentTurnCount) => statusContainer.canSummon(currentTurnCount);

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
        System.Random rnd = THREAD_SAVE_RNG.Value;

        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = rnd.Next(i, deck.Count);
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

        Card targetCard = validCards[THREAD_SAVE_RNG.Value.Next(0, validCards.Count)];

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
            consumeMana(card.getCost() + getExtraCardCost(currentTurnCount));

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

    public bool canUseHoldAction()
    {
        return mana > DEFAULT_HOLD_COST && handList.Count > 0;
    }
}
