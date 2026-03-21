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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void processCardEffect(Card card)
    {
        int cost = card.getCost();

        // To do : check if player has enough mana to use the card, if not, return

        switch (card.getId())
        {
            case 1: // Summon Pawn
                InputManager.readyToSummonChess(ChessType.Pawn, GameManager.IsPlayerWhite());
                break;
            case 2: // Summon Bishop
                InputManager.readyToSummonChess(ChessType.Bishop, GameManager.IsPlayerWhite());
                break;
            case 3: // Summon Knight
                InputManager.readyToSummonChess(ChessType.Knight, GameManager.IsPlayerWhite());
                break;
            case 4: // Summon Rook
                InputManager.readyToSummonChess(ChessType.Rook, GameManager.IsPlayerWhite());
                break;
            case 5: // Summon Queen
                InputManager.readyToSummonChess(ChessType.Queen, GameManager.IsPlayerWhite());
                break;
            default:
                Debug.LogWarning("Card effect not implemented for card id: " + card.getId());
                break;
        }
    }
}
