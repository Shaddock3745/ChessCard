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

    public static async Task readyToProcess(bool isPlayer, Card card)
    {
        if (isPlayer)
        {
            switch (card.getId())
            {
                case 1: // Summon Pawn
                    await processSummonCard(card, ChessType.Pawn);
                    break;
                case 2: // Summon Bishop
                    await processSummonCard(card, ChessType.Bishop);
                    break;
                case 3: // Summon Knight
                    await processSummonCard(card, ChessType.Knight);
                    break;
                case 4: // Summon Rook
                    await processSummonCard(card, ChessType.Rook);
                    break;
                case 5: // Summon Queen
                    await processSummonCard(card, ChessType.Queen);
                    break;
                default:
                    Debug.LogWarning("Card effect not implemented for card id: " + card.getId());
                    break;
            }
        }
        else
        {

        }
    }

    private static async Task processSummonCard(Card card, ChessType chessType)
    {
        BoardTile selectedTile = await InputManager.waitForTargetSelection(BoardManager.getSummonableTiles(chessType, GameManager.IsPlayerWhite()));

        if (selectedTile != null)
        {
            MatchDataManager.useCard(true, card);
            BoardManager.summonChessPiece(chessType, GameManager.IsPlayerWhite(), selectedTile.getPos());
            HandViewManager.destoryUsingCard();
        }
        else
        {
            HandViewManager.rearrangeUsingCard();
        }
    }
}