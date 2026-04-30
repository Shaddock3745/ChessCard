using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MatchDataManager : MonoBehaviour
{
    private static MatchDataManager instance;

    private PlayerData local, enemy;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
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

    public static void addMana(bool isLoal, int amount)
    {
        if (isLoal)
            instance.local.addMana(amount);
        else
            instance.enemy.addMana(amount);

        instance.updateManaUi();
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
                HandViewManager.playerDrawCard(result);
            instance.updateCardCountUi();

            return result;
        }
        else
        {
            Card result = instance.enemy.drawCard();

            if (result != null)
                HandViewManager.enemyDrawCard();

            return result;
        }
    }

    public static void useCard(bool isLocal, Card card)
    {
        if (isLocal)
        {
            instance.local.useCard(card);
            instance.updateCardCountUi();
            instance.updateManaUi();
        }
        else
        {
            instance.enemy.useCard(card);
            HandViewManager.enemyUseCard(card);
        }
    }

    public static void flushHandCards(bool isLocal)
    {
        if (isLocal)
        {
            instance.local.flushHandCards();
            HandViewManager.flush(true);
            instance.updateCardCountUi();
        }
        else
        {
            instance.enemy.flushHandCards();
            HandViewManager.flush(false);
        }
    }

    private void updateCardCountUi()
    {
        UiManager.updateDeckCount(instance.local.getDeckCardCount());
        UiManager.updateWaitingCount(instance.local.getWaitingCardCount());
        UiManager.updateDiscardCount(instance.local.getDiscardCardCount());
    }

    private void updateManaUi()
    {
        UiManager.updateManaCount(instance.local.getMana());
    }

    public static void initLocal(PlayerData playerData)
    {
        instance.local = playerData;
        instance.updateCardCountUi();
        instance.updateManaUi();
    }

    public static void initEnemy(PlayerData playerData)
    {
        instance.enemy = playerData;
    }

    public static bool isAI(bool isLocal)
    {
        return isLocal ? instance.local.isAI() : instance.enemy.isAI();
    }

    public static PlayerData getPlayerData(bool isLocal)
    {
        return isLocal ? instance.local : instance.enemy;
    }

    public static void turnStart(bool isLocalTurn)
    {
        if (isLocalTurn)
        {
            instance.local.resetForNewTurn();
            int handCardCount = PlayerData.getDefaultHandCardCount() + instance.local.getBonusHandCardCount();

            for (int i = 0; i < handCardCount; i++)
                drawCard(true);

            instance.updateCardCountUi();
            instance.updateManaUi();
        }
        else
        {
            instance.enemy.resetForNewTurn();
            int handCardCount = PlayerData.getDefaultHandCardCount() + instance.enemy.getBonusHandCardCount();

            for (int i = 0; i < handCardCount; i++)
                drawCard(false);
        }
    }

    public static void turnEnd(bool isLocalTurn)
    {
        if (isLocalTurn)
        {
            instance.local.resetForEndTurn();
            instance.local.flushHandCards();
            HandViewManager.flush(true);
            instance.updateCardCountUi();
            instance.updateManaUi();
        }
        else
        {
            instance.enemy.resetForEndTurn();
            instance.enemy.flushHandCards();
            HandViewManager.flush(false);
        }
    }

    public static bool canMoveChess(bool isLocal, ChessPiece chess)
    {
        if (isLocal)
            return instance.local.canMoveChess(chess);
        else
            return instance.enemy.canMoveChess(chess);
    }


    public static bool canUseCard(bool isLocal, Card card)
    {
        if (isLocal)
            return instance.local.canUseCard(card);
        else
            return instance.enemy.canUseCard(card);
    }


    public static void doChessMoved(bool isLocal, ChessPiece chess)
    {
        if (isLocal)
            instance.local.doChessMoved(chess);
        else
            instance.enemy.doChessMoved(chess);

        instance.updateManaUi();
    }
}