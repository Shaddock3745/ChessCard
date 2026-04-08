using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MatchDataManager : MonoBehaviour
{
    private static MatchDataManager instance;

    private PlayerData player, enemy;

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
        initPlayer();
        initEnemy(AiManager.initAIPlayerData());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void addMana(bool isPlayer, int amount)
    {
        if (isPlayer)
            instance.player.addMana(amount);
        else
            instance.enemy.addMana(amount);

        instance.updateManaUi();
    }

    public static void shuffleDeck(bool isPlayer)
    {
        if (isPlayer)
            instance.player.shuffleDeck();
        else
            instance.enemy.shuffleDeck();
    }

    public static void replenish(bool isPlayer)
    {
        if (isPlayer)
            instance.player.replenish();
        else
            instance.enemy.replenish();
    }

    public static Card drawCard(bool isPlayer)
    {
        if (isPlayer)
        {
            Card result = instance.player.drawCard();

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

    public static void useCard(bool isPlayer, Card card)
    {
        if (isPlayer)
        {
            instance.player.useCard(card);
            instance.updateCardCountUi();
            instance.updateManaUi();
        }
        else
        {
            instance.enemy.useCard(card);
            HandViewManager.enemyUseCard(card);
        }
    }

    public static void flushHandCards(bool isPlayer)
    {
        if (isPlayer)
        {
            instance.player.flushHandCards();
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
        UiManager.updateDeckCount(instance.player.getDeckCardCount());
        UiManager.updateWaitingCount(instance.player.getWaitingCardCount());
        UiManager.updateDiscardCount(instance.player.getDiscardCardCount());
    }

    private void updateManaUi()
    {
        UiManager.updateManaCount(instance.player.getMana());
    }

    public static void initPlayer()
    {
        instance.player = new PlayerData(false, true, PlayerDataManager.getPlayerDeck());
        instance.updateCardCountUi();
        instance.updateManaUi();
    }

    public static void initEnemy(PlayerData playerData)
    {
        instance.enemy = playerData;
    }

    public static bool isEnemyAI()
    {
        return instance.enemy.isAI();
    }

    public static void turnStart(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            instance.player.resetForNewTurn();
            int handCardCount = PlayerData.getDefaultHandCardCount() + instance.player.getBonusHandCardCount();

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

    public static void turnEnd(bool isPlayer)
    {
        if (isPlayer)
        {
            instance.player.resetForEndTurn();
            instance.player.flushHandCards();
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

    public static bool canMoveChess(bool isPlayer, ChessPiece chess)
    {
        if (isPlayer)
            return instance.player.canMoveChess(chess);
        else
            return instance.enemy.canMoveChess(chess);
    }


    public static bool canUseCard(bool isPlayer, Card card)
    {
        if (isPlayer)
            return instance.player.canUseCard(card);
        else
            return instance.enemy.canUseCard(card);
    }


    public static void doChessMoved(bool isPlayer, ChessPiece chess)
    {
        if (isPlayer)
            instance.player.doChessMoved(chess);
        else
            instance.enemy.doChessMoved(chess);

        instance.updateManaUi();
    }
}