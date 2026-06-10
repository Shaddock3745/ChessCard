using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchDataManager : MonoBehaviour
{
    private static MatchDataManager instance;

    private readonly List<MoveAction> chessMoveBuffer = new List<MoveAction>(30);

    private PlayerData local, enemy;

    public static event Action<bool> OnPlayerHoldCard;
    public static event Action<bool, Card> OnPlayerUseCard;
    public static event Action OnLocalCardCountChange;
    public static event Action OnLocalManaCountChange;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);
    }

    void OnDestroy()
    {
        // 場景卸載/重讀時清掉本管理器擁有的靜態事件,避免舊場景訂閱者殘留(複本自毀不影響)。
        if (instance == this)
        {
            OnPlayerHoldCard = null;
            OnPlayerUseCard = null;
            OnLocalCardCountChange = null;
            OnLocalManaCountChange = null;
        }
    }

    public static void shuffleDeck(bool isLocal)
    {
        if (isLocal)
            instance.local.shuffleDeck();
        else
            instance.enemy.shuffleDeck();
    }

    public static void replenish(bool isLocal)
    {
        if (isLocal)
            instance.local.replenish();
        else
            instance.enemy.replenish();
    }

    public static Card drawCard(bool isLocal)
    {
        if (isLocal)
        {
            Card result = instance.local.drawCard();

            if (result != null)
            {
                _ = HandViewManager.playerDrawCard(result);
                OnLocalCardCountChange?.Invoke();
            }

            return result;
        }
        else
        {
            Card result = instance.enemy.drawCard();

            if (result != null)
                _ = HandViewManager.enemyDrawCard();

            return result;
        }
    }

    public static void materializeCard(bool isLocal, int cardId, bool noConsumption)
    {
        if (isLocal)
        {
            Card result = instance.local.materializeCard(cardId, noConsumption);

            if (result != null)
            {
                _ = HandViewManager.playerDrawCard(result);
                OnLocalCardCountChange?.Invoke();
            }
        }
        else
        {
            Card result = instance.enemy.materializeCard(cardId, noConsumption);

            if (result != null)
                _ = HandViewManager.enemyDrawCard();
        }
    }

    public static void retrieveCard(bool isLocal, Card card)
    {
        if (isLocal)
        {
            Card result = instance.local.retrieveCard(card);

            if (result != null)
            {
                _ = HandViewManager.playerRetrieveCard(result);
                OnLocalCardCountChange?.Invoke();
            }
        }
        else
        {
            Card result = instance.enemy.retrieveCard(card);

            if (result != null)
                _ = HandViewManager.enemyRetrieveCard();
        }
    }

    public static void retrieveAllHoldingCard(bool isLocal)
    {
        if (isLocal)
            instance.local.retrieveAllHoldingCard();
        else
            instance.enemy.retrieveAllHoldingCard();
    }

    public static void holdCard(bool isLocal, Card card)
    {
        if (isLocal)
        {
            Card result = instance.local.holdCard(card);

            if (result != null)
            {
                _ = HandViewManager.playerHoldCard(result);
                OnPlayerHoldCard(GameManager.IsLocalWhite());
                OnLocalCardCountChange?.Invoke();
                OnLocalManaCountChange?.Invoke();
            }

        }
        else
        {
            Card result = instance.enemy.holdCard(card);

            if (result != null)
            {
                _ = HandViewManager.enemyHoldCard();
                OnPlayerHoldCard(!GameManager.IsLocalWhite());
            }
        }
    }

    public static void rechargeCard(bool isLocal, Card card)
    {
        if (isLocal)
        {
            Card result = instance.local.rechargeCard(card);
            if (result != null)
            {
                _ = HandViewManager.playerRechargeCard(result);
                OnLocalCardCountChange?.Invoke();
            }
        }
        else
        {
            Card result = instance.enemy.rechargeCard(card);
            if (result != null)
            {
                _ = HandViewManager.enemyRechargeCard();
            }
        }
    }

    public static Card drawRandomCardFromDeck(bool isLocal, int[] validCardIds)
    {
        if (isLocal)
        {
            Card result = instance.local.drawRandomCardFromDeck(validCardIds);

            if (result != null)
            {
                _ = HandViewManager.playerDrawCard(result);
                OnLocalCardCountChange?.Invoke();
            }

            return result;
        }
        else
        {
            Card result = instance.enemy.drawRandomCardFromDeck(validCardIds);

            if (result != null)
                _ = HandViewManager.enemyDrawCard();

            return result;
        }
    }

    public static bool reachMoveLimit(bool isLocal, int currentTurnCount)
    {
        if (isLocal)
            return ActionValidator.reachMoveLimit(instance.local, currentTurnCount);
        else
            return ActionValidator.reachMoveLimit(instance.enemy, currentTurnCount);
    }

    public static bool hasValidCardInDeckOrWaiting(bool isLocal, int[] validCardIds)
    {
        if (isLocal)
            return instance.local.hasValidCardInDeckOrWaiting(validCardIds);
        else
            return instance.enemy.hasValidCardInDeckOrWaiting(validCardIds);
    }

    public static void useCard(bool isLocal, Card card, int currentTurnCount)
    {
        if (isLocal)
        {
            instance.local.useCard(card, currentTurnCount);
            OnLocalCardCountChange?.Invoke();
            OnLocalManaCountChange?.Invoke();
            OnPlayerUseCard(GameManager.IsLocalWhite(), card);
        }
        else
        {
            instance.enemy.useCard(card, currentTurnCount);
            _ = HandViewManager.enemyUseCard(card);
            OnPlayerUseCard(!GameManager.IsLocalWhite(), card);
        }
    }

    public static void flushHandCards(bool isLocal)
    {
        if (isLocal)
        {
            instance.local.flushHandCards();
            _ = HandViewManager.flush(true);
            OnLocalCardCountChange?.Invoke();
        }
        else
        {
            instance.enemy.flushHandCards();
            _ = HandViewManager.flush(false);
        }
    }

    public static void initLocal(PlayerData playerData)
    {
        instance.local?.onGameOver(); // 釋放上一局玩家的狀態容器(取消訂閱回合事件),避免重開局累積洩漏
        instance.local = playerData;
        OnLocalCardCountChange?.Invoke();
        OnLocalManaCountChange?.Invoke();
    }

    public static void initEnemy(PlayerData playerData)
    {
        instance.enemy?.onGameOver(); // 同上
        instance.enemy = playerData;
    }

    public static bool isAI(bool isLocal)
    {
        return isLocal ? instance.local.isAI() : instance.enemy.isAI();
    }

    public static int getPlayerMana(bool isLocal)
    {
        return isLocal ? instance.local.getMana() : instance.enemy.getMana();
    }

    public static void changePlayerMana(bool isLocal, int delta)
    {
        if (isLocal)
            instance.local.addMana(delta);
        else
            instance.enemy.addMana(delta);

        OnLocalManaCountChange?.Invoke();
    }

    public static int getPlayerDeckCardCount(bool isLocal)
    {
        return isLocal ? instance.local.getDeckCardCount() : instance.enemy.getDeckCardCount();
    }

    public static List<Card> getPlayerDeckList(bool isLocal)
    {
        return isLocal ? instance.local.getDeck() : instance.enemy.getDeck();
    }

    public static int getPlayerHandCardCount(bool isLocal)
    {
        return isLocal ? instance.local.getHandCardCount() : instance.enemy.getHandCardCount();
    }

    public static List<Card> getPlayerHandCardList(bool isLocal)
    {
        return isLocal ? instance.local.getHandList() : instance.enemy.getHandList();
    }

    public static int getPlayerWaitingCardCount(bool isLocal)
    {
        return isLocal ? instance.local.getWaitingCardCount() : instance.enemy.getWaitingCardCount();
    }

    public static List<Card> getPlayerWaitingCardList(bool isLocal)
    {
        return isLocal ? instance.local.getWaitingList() : instance.enemy.getWaitingList();
    }

    public static int getPlayerDiscardCardCount(bool isLocal)
    {
        return isLocal ? instance.local.getDiscardCardCount() : instance.enemy.getDiscardCardCount();
    }

    public static List<Card> getPlayerDiscardCardList(bool isLocal)
    {
        return isLocal ? instance.local.getDiscardList() : instance.enemy.getDiscardList();
    }

    public static int getPlayerHoldingCardCount(bool isLocal)
    {
        return isLocal ? instance.local.getHoldCardCount() : instance.enemy.getHoldCardCount();
    }

    public static List<Card> getPlayerHoldingCardList(bool isLocal)
    {
        return isLocal ? instance.local.getHoldingList() : instance.enemy.getHoldingList();
    }

    public static void turnStart(bool isLocalTurn)
    {
        int currentTurnCount = GameManager.getCurrentTurnCount();

        if (isLocalTurn)
        {
            instance.local.resetForNewTurn(currentTurnCount);
            int handCardCount = PlayerData.getDefaultHandCardCount() + instance.local.getBonusHandCardCount(currentTurnCount);

            if (handCardCount > 0)
                for (int i = 0; i < handCardCount; i++)
                    drawCard(true);

            OnLocalCardCountChange?.Invoke();
            OnLocalManaCountChange?.Invoke();
        }
        else
        {
            instance.enemy.resetForNewTurn(currentTurnCount);
            int handCardCount = PlayerData.getDefaultHandCardCount() + instance.enemy.getBonusHandCardCount(currentTurnCount);

            if (handCardCount > 0)
                for (int i = 0; i < handCardCount; i++)
                    drawCard(false);
        }
    }

    public static void turnEnd(bool isLocalTurn)
    {
        int currentTurnCount = GameManager.getCurrentTurnCount();

        if (isLocalTurn)
        {
            instance.local.resetForEndTurn(currentTurnCount);

            if (isDiscardAllHandCardOnTurnEnd(true, currentTurnCount))
                instance.local.discardAllHandCards();
            else
                instance.local.flushHandCards();

            _ = HandViewManager.flush(true);
            OnLocalCardCountChange?.Invoke();
            OnLocalManaCountChange?.Invoke();
        }
        else
        {
            instance.enemy.resetForEndTurn(currentTurnCount);

            if (isDiscardAllHandCardOnTurnEnd(false, currentTurnCount))
                instance.enemy.discardAllHandCards();
            else
                instance.enemy.flushHandCards();

            _ = HandViewManager.flush(false);
        }
    }

    public static bool canHoldCard(bool isLocal)
    {
        return ActionValidator.canHoldCard(isLocal ? instance.local : instance.enemy);
    }

    public static bool canMoveChess(bool isLocal, ChessPiece chess, int currentTurnCount)
    {
        instance.chessMoveBuffer.Clear();

        if (isLocal)
            ActionValidator.getLegalMoves(BoardManager.getRealBoardState(), instance.local, chess, currentTurnCount, instance.chessMoveBuffer);
        else
            ActionValidator.getLegalMoves(BoardManager.getRealBoardState(), instance.enemy, chess, currentTurnCount, instance.chessMoveBuffer);

        return instance.chessMoveBuffer.Count > 0;
    }

    public static bool canSummonThisTurn(bool isLocal, int currentTurnCount)
    {
        return ActionValidator.canSummon(isLocal ? instance.local : instance.enemy, currentTurnCount);
    }

    public static bool canUseCard(bool isLocal, Card card, int currentTurnCount)
    {
        return ActionValidator.canUseCard(isLocal ? instance.local : instance.enemy, card, currentTurnCount);
    }

    public static int getExtraMoveCost(bool isLocal, int currentTurnCount)
    {
        if (isLocal)
            return instance.local.getExtraMoveCost(currentTurnCount);
        else
            return instance.enemy.getExtraMoveCost(currentTurnCount);
    }

    public static int getExtraCardCost(bool isLocal, int currentTurnCount)
    {
        if (isLocal)
            return instance.local.getExtraCardCost(currentTurnCount);
        else
            return instance.enemy.getExtraCardCost(currentTurnCount);
    }

    public static bool isCardNonCost(bool isLocal, int currentTurnCount)
    {
        if (isLocal)
            return instance.local.isCardNonCost(currentTurnCount);
        else
            return instance.enemy.isCardNonCost(currentTurnCount);
    }

    public static bool isDiscardAllHandCardOnTurnEnd(bool isLocal, int currentTurnCount)
    {
        if (isLocal)
            return instance.local.isDiscardAllHandCardOnTurnEnd(currentTurnCount);
        else
            return instance.enemy.isDiscardAllHandCardOnTurnEnd(currentTurnCount);
    }

    public static void doChessMoved(bool isLocal, ChessPiece chess, Vector2Int from, Vector2Int to, ActionType actionType, int currentTurnCount)
    {
        if (isLocal)
        {
            instance.local.doChessMoved(chess, from, to, actionType, currentTurnCount);
            OnLocalManaCountChange?.Invoke();
        }
        else
            instance.enemy.doChessMoved(chess, from, to, actionType, currentTurnCount);
    }

    public static bool hadMovedThisTurn(bool isLocal)
    {
        if (isLocal)
            return instance.local.getMoveCount() > 0;
        else
            return instance.enemy.getMoveCount() > 0;
    }

    public static void addStatus(bool isLocal, PlayerStatus status)
    {
        if (isLocal)
            instance.local.addStatus(status);
        else
            instance.enemy.addStatus(status);
    }

    public static void cleanUpExpiredPlayerStatuses(int currentTotalTurn)
    {
        instance.local.cleanUpExpiredStatuses(currentTotalTurn);
        instance.enemy.cleanUpExpiredStatuses(currentTotalTurn);
    }

    public static void resetGame()
    {

    }

    public static PlayerData getPlayerData(bool isLocal) => isLocal ? instance.local : instance.enemy;

    public static AiProfile getAiProfile(bool isLocal)
    {
        if (isLocal)
            return instance.local.getAiProfile();
        else
            return instance.enemy.getAiProfile();
    }
}