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
        cardRegistry.Clear();

        // 士兵
        cardRegistry[1] = new SummonEffect(ChessType.Pawn);

        // 主教
        cardRegistry[2] = new SummonEffect(ChessType.Bishop);

        // 騎士
        cardRegistry[3] = new SummonEffect(ChessType.Knight);

        // 城堡
        cardRegistry[4] = new SummonEffect(ChessType.Rook);

        // 皇后
        cardRegistry[5] = new SummonEffect(ChessType.Queen);

        // 徵兵令
        cardRegistry[6] = new CompositeEffect(new ChessAmountCheckEffect(true, 3, true, false, ChessType.Pawn), new MaterializeCardEffect(false, 1, 2, true));

        // 王車易位
        cardRegistry[7] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Rook), new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessCantMoveStatus(startTurn, startTurn + 1)),
            new SwapChessPositionEffect((payload) => { return (payload.getTargetPositions()[0], BoardManager.getPositionOfKing(GameManager.IsLocalTurn() == GameManager.IsLocalWhite())); }));

        // 政教合一
        cardRegistry[8] = new AddStatusToAllMatchingTypeChessEffect(true, 1, (startTurn, endTurn) => new ChessMovementOverrideStatus(new BishopMovement(), startTurn, endTurn), true, ChessType.King);

        // 咆嘯
        cardRegistry[9] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessPiece.getAllChessTypes(false)), new RoarEffect((payload) => payload.getTargetPositions()[0],
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right, new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(-1, -1), new Vector2Int(1, -1)));

        // 護盾
        cardRegistry[10] = new CompositeEffect(new ChessAmountCompareEffect(true, false), new AddSelectedChessSingleStatusEffect(true, 2, (startTurn, endTurn) => new ChessCantBeAttackedStatus(startTurn, endTurn), ChessPiece.getAllChessTypes(true)));

        // 蓄勢待發 (需要未移動判斷)
        cardRegistry[13] = new CompositeEffect(new AddPlayerStatusEffect(false, 1, (startTurn, endTurn) => new PlayerCantMoveStatus(startTurn, endTurn)), new AddPlayerStatusEffect(false, 1, 2, (startTurn, endTurn) => new PlayerBonusManaStatus(3, startTurn, endTurn)));

        // 鐐銬
        cardRegistry[16] = new CompositeEffect(new ChessAmountCheckEffect(false, 4, false, false, ChessPiece.getAllChessTypes(false)), new ChessAmountCompareEffect(true, false),
            new AddSelectedChessMultipleStatusEffect(false, (startTurn) => new ChessStatus[] { new ChessCantMoveStatus(startTurn, startTurn + 2), new ChessCantBeAttackedStatus(startTurn, startTurn + 3) }, ChessPiece.getAllChessTypes(true)));

        // 一換一
        cardRegistry[17] = new SequentialCompositeEffect(new RemoveChessEffect(true, ChessPiece.getAllChessTypes(true)), (firstPayload) => new RemoveChessEffect(false, BoardManager.getChessPieceAtPosition(firstPayload.getTargetPositions()[0]).getChessType()));

        // 施術準備
        cardRegistry[18] = new CompositeEffect(new DrawCardEffect(false, 2), new AddPlayerStatusEffect(false, 1, (startTurn, endTurn) => new PlayerCantUseSpecificCardStatus(18, startTurn, endTurn)));

        // 冊封
        cardRegistry[20] = new CompositeEffect(new ChessAmountCheckEffect(true, 1, true, false, ChessType.Knight), new ChessAmountCheckEffect(true, 2, false, true, ChessType.Pawn), new RemoveChessEffect(true, ChessType.Pawn), new MaterializeCardEffect(false, 3, 1, true));

        // 政治聯姻
        cardRegistry[22] = new CompositeEffect(new MaterializeCardEffect(false, 5, 1, true), new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new PlayerCantAttackStatus(startTurn, endTurn)), new AddPlayerStatusEffect(true, 2, (startTurn, endTurn) => new PlayerBonusManaStatus(2, startTurn, endTurn)));

        // 停戰協議
        cardRegistry[23] = new CompositeEffect(new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new PlayerCantAttackStatus(startTurn, endTurn)), new AddPlayerStatusEffect(true, 2, (startTurn, endTurn) => new PlayerCantAttackStatus(startTurn, endTurn)));

        // 大飢荒
        cardRegistry[26] = new CompositeEffect(new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new PlayerCantSummonStatus(startTurn, endTurn)), new AddPlayerStatusEffect(true, 3, (startTurn, endTurn) => new PlayerCantSummonStatus(startTurn, endTurn)));

        // 通貨膨脹
        cardRegistry[27] = new CompositeEffect(new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new IncreasePlayerCardCostStatus(1, startTurn, endTurn)), new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new IncreasePlayerMoveCostStatus(1, startTurn, endTurn)),
            new AddPlayerStatusEffect(true, 3, (startTurn, endTurn) => new IncreasePlayerCardCostStatus(1, startTurn, endTurn)), new AddPlayerStatusEffect(true, 3, (startTurn, endTurn) => new IncreasePlayerMoveCostStatus(1, startTurn, endTurn)));

        // 捨身取義
        cardRegistry[28] = new CompositeEffect(new RemoveChessEffect(true, ChessType.Pawn, ChessType.Rook, ChessType.Bishop, ChessType.Knight, ChessType.Queen), new ManaChangeEffect(2), new DrawCardEffect(false, 1));

        // 御駕親征 (攻擊判定未完成)
        cardRegistry[29] = new CompositeEffect(new AddStatusToAllMatchingTypeChessEffect(true, (startTurn) => new ChessStatus[] { new ChessMovementOverrideStatus(new QueenMovement(), startTurn, startTurn + 1),
            new ChessCantAttackSpecificTypeStatus(ChessType.King, startTurn, startTurn + 1) }, true, ChessType.King));
    }

    public static CardEffect getCardEffect(Card card)
    {
        return cardRegistry.TryGetValue(card.getId(), out CardEffect effect) ? effect : null;
    }

    public static bool canActivateCard(bool isLocal, Card card, int currentTurnCount)
    {
        if (cardRegistry.TryGetValue(card.getId(), out CardEffect effect))
            return effect.canActivate(isLocal, currentTurnCount, card);
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

        HandViewManager.recalculateCost();
        LogManager.createLog(new CardUseLog(isLocal ? GameManager.IsLocalWhite() : !GameManager.IsLocalWhite(), card));
    }
}