using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    private static CardEffectManager instance;

    private static Dictionary<int, CardEffect> cardRegistry = new Dictionary<int, CardEffect>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            initDictionary();
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
    }

    void Update()
    {
    }

    private static void initDictionary()
    {
        cardRegistry[1] = new SummonEffect(ChessType.Pawn);
        cardRegistry[2] = new SummonEffect(ChessType.Bishop);
        cardRegistry[3] = new SummonEffect(ChessType.Knight);
        cardRegistry[4] = new SummonEffect(ChessType.Rook);
        cardRegistry[5] = new SummonEffect(ChessType.Queen);
    }

    public static CardEffect getCardEffect(Card card)
    {
        return cardRegistry.TryGetValue(card.getId(), out CardEffect effect) ? effect : null;
    }

    private static ChessType getChessTypeByCard(Card card) => card.getId() switch
    {
        1 => ChessType.Pawn,
        2 => ChessType.Bishop,
        3 => ChessType.Knight,
        4 => ChessType.Rook,
        5 => ChessType.Queen,
        _ => ChessType.Pawn
    };

    public static async Task processLocalPlayerCard(Card card)
    {
        List<Vector2Int> targetPositions = new List<Vector2Int>();
        List<Card> targetCards = new List<Card>();
        bool isWhite = GameManager.IsLocalWhite();

        switch (card.getId())
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                BoardTile tile = await InputManager.waitForTargetSelection(BoardManager.getSummonableTiles(getChessTypeByCard(card), isWhite));

                if (tile == null)
                {
                    HandViewManager.rearrangeUsingCard();
                    return;
                }

                targetPositions.Add(tile.getPos());
                break;
        }

        MatchDataManager.useCard(true, card);
        HandViewManager.destroyUsingCard();

        await executeCardEffect(true, card, targetPositions, targetCards);
    }

    public static async Task processLocalAiCard(Card card, List<Vector2Int> targetPositions, List<Card> targetCards)
    {
        MatchDataManager.useCard(true, card);
        await executeCardEffect(true, card, targetPositions, targetCards);
    }

    public static async Task processEnemyAiCard(Card card, List<Vector2Int> targetPositions, List<Card> targetCards)
    {
        MatchDataManager.useCard(false, card);
        await executeCardEffect(false, card, targetPositions, targetCards);
    }

    private static async Task executeCardEffect(bool isLocal, Card card, List<Vector2Int> targetPositions, List<Card> targetCards)
    {
        if (cardRegistry.TryGetValue(card.getId(), out CardEffect effect))
            await effect.executeEffect(isLocal, card, targetPositions);
        else
            Debug.LogWarning("未註冊的卡片 ID: " + card.getId());
    }
}