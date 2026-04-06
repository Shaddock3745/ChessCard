using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    private static CardEffectManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
    }

    void Update()
    {
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

    public static async Task processPlayerCard(Card card)
    {
        List<Vector2Int> targetPositions = new List<Vector2Int>();
        List<Card> targetCards = new List<Card>();
        bool isWhite = GameManager.IsPlayerWhite();

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

    public static async Task processAiCard(Card card, List<Vector2Int> targetPositions, List<Card> targetCards)
    {
        MatchDataManager.useCard(false, card);

        await executeCardEffect(false, card, targetPositions, targetCards);
    }

    private static async Task executeCardEffect(bool isPlayer, Card card, List<Vector2Int> targetPositions, List<Card> targetCards)
    {
        bool isWhite = isPlayer ? GameManager.IsPlayerWhite() : !GameManager.IsPlayerWhite();

        switch (card.getId())
        {
            case 1:
            case 2:
            case 3:
            case 4:
            case 5:
                if (isPlayer)
                    processSummonWithoutAnimation(getChessTypeByCard(card), isWhite, targetPositions[0]);
                else
                {
                    await BoardManager.showTargetWithAnimation(targetPositions);
                    await processSummonWithAnimation(getChessTypeByCard(card), isWhite, targetPositions[0]);
                    BoardManager.removeAllHint();
                }
                break;
            default:
                Debug.LogWarning("Card effect not implemented for card id: " + card.getId());
                break;
        }
    }

    private static void processSummonWithoutAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        BoardManager.summonChessPiece(type, isWhite, position);
    }

    private static async Task processSummonWithAnimation(ChessType type, bool isWhite, Vector2Int position)
    {
        await BoardManager.summonChessPieceWithAnimation(type, isWhite, position);
    }
}