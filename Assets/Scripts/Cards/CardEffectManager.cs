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

    private static void initDictionary()
    {
        cardRegistry[1] = new SummonEffect(ChessType.Pawn);
        cardRegistry[2] = new SummonEffect(ChessType.Bishop);
        cardRegistry[3] = new SummonEffect(ChessType.Knight);
        cardRegistry[4] = new SummonEffect(ChessType.Rook);
        cardRegistry[5] = new SummonEffect(ChessType.Queen);
        cardRegistry[6] = new CompositeEffect(new ChessAmountCheckEffect(ChessType.Pawn, 3, true, false), new MaterializeCardEffect(1, 2, true));
        cardRegistry[28] = new CompositeEffect(new RemoveChessEffect(true, ChessType.Pawn, ChessType.Rook, ChessType.Bishop, ChessType.Knight, ChessType.Queen), new ManaChangeEffect(2), new DrawCardEffect(1));
    }

    public static CardEffect getCardEffect(Card card)
    {
        return cardRegistry.TryGetValue(card.getId(), out CardEffect effect) ? effect : null;
    }

    public static bool canActivateCard(bool isLocal, Card card)
    {
        if (cardRegistry.TryGetValue(card.getId(), out CardEffect effect))
            return effect.canActivate(isLocal, card);
        else
        {
            Debug.LogWarning("未註冊的卡片 ID: " + card.getId());
            return false;
        }
    }

    public static async Task processLocalPlayerCard(Card card)
    {
        if (!cardRegistry.TryGetValue(card.getId(), out CardEffect effect))
        {
            Debug.LogWarning("卡片效果未註冊");
            _ = HandViewManager.rearrangeUsingCard();
            return;
        }

        EffectTargetPayload payload = await effect.gatherTargets();

        if (payload == null)
        {
            _ = HandViewManager.rearrangeUsingCard();
            return;
        }

        MatchDataManager.useCard(true, card);
        HandViewManager.destroyUsingCard();

        await executeCardEffect(true, card, payload);

    }

    public static async Task processLocalAiCard(Card card, EffectTargetPayload targets)
    {
        MatchDataManager.useCard(true, card);
        await executeCardEffect(true, card, targets);
    }

    public static async Task processEnemyAiCard(Card card, EffectTargetPayload targets)
    {
        MatchDataManager.useCard(false, card);
        await executeCardEffect(false, card, targets);
    }

    private static async Task executeCardEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (cardRegistry.TryGetValue(card.getId(), out CardEffect effect))
            await effect.executeEffect(isLocal, card, targets);
        else
            Debug.LogWarning("未註冊的卡片 ID: " + card.getId());

        LogManager.createLog(new CardUseLog(isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite(), card));
    }
}