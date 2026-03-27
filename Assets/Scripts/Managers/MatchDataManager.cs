using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MatchDataManager : MonoBehaviour
{
    private static MatchDataManager instance;

    [SerializeField] private TMPro.TextMeshProUGUI deckCount, waitingCount, discardCount, manaCount;

    private PlayerData player, enemy;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            initPlayer();
            initEnemy(AIManager.initAIPlayerData());
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
                HandViewManager.drawCard(result);
            instance.updateCardCountUi();

            return result;
        }
        else
        {
            Card result = instance.enemy.drawCard();

            return result;
        }
    }

    public static bool useCard(bool isPlayer, Card card)
    {
        if (isPlayer)
        {
            bool result = instance.player.useCard(card);

            if (result)
                CardEffectManager.processCardEffect(true, card);

            instance.updateCardCountUi();
            instance.updateManaUi();
            return result;
        }
        else
        {
            return instance.enemy.useCard(card);
        }
    }

    public static void flushHandCards(bool isPlayer)
    {
        if (isPlayer)
        {
            instance.player.flushHandCards();
            HandViewManager.flush();
            instance.updateCardCountUi();
        }
        else
        {
            instance.enemy.flushHandCards();
        }
    }

    private void updateCardCountUi()
    {
        deckCount.text = instance.player.getDeckCardCount().ToString();
        waitingCount.text = instance.player.getWaitingCardCount().ToString();
        discardCount.text = instance.player.getDiscardCardCount().ToString();
    }

    private void updateManaUi()
    {
        manaCount.text = instance.player.getMana().ToString();
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
            HandViewManager.flush();
            instance.updateCardCountUi();
            instance.updateManaUi();
        }
        else
        {
            instance.enemy.resetForEndTurn();
            instance.enemy.flushHandCards();
        }
    }

    public static bool canMoveChess(bool isPlayer, ChessPiece chess)
    {
        if (isPlayer)
            return instance.player.canMoveChess(chess);
        else
            return instance.enemy.canMoveChess(chess);
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