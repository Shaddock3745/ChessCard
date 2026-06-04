using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public static class AreaUtils
{
    public static Vector2Int[] getSquareArea(int radius, bool includeCenter)
    {
        List<Vector2Int> dirs = new List<Vector2Int>();
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (!includeCenter && x == 0 && y == 0) continue;
                dirs.Add(new Vector2Int(x, y));
            }
        }
        return dirs.ToArray();
    }

    public static bool isOppositeColor(Vector2Int posA, Vector2Int posB)
    {
        return Mathf.Abs(posA.x + posA.y) % 2 != Mathf.Abs(posB.x + posB.y) % 2;
    }
}

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

        // 亢奮
        cardRegistry[11] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Pawn), new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessMovementOverrideStatus(new BuffedPawnMovement(), startTurn, startTurn + 3)));

        // 神聖加護
        cardRegistry[12] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Bishop),
            new AddStatusToAreaChessEffect(true, 2, (startTurn, endTurn) => new ChessCantBeAttackedStatus(startTurn, endTurn), ChessPiece.getAllChessTypes(false), (isLocal, payload) => payload.getTargetPositions(), AreaUtils.getSquareArea(1, true)));

        // 蓄勢待發
        cardRegistry[13] = new CompositeEffect(new PlayerHadntMovedCheckEffect(false), new AddPlayerStatusEffect(false, 1, (startTurn, endTurn) => new PlayerCantMoveStatus(startTurn, endTurn)),
            new AddPlayerStatusEffect(false, 1, 2, (startTurn, endTurn) => new PlayerBonusManaStatus(3, startTurn, endTurn)));

        // 愈戰愈勇
        cardRegistry[14] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Pawn),
            new AddPlayerStatusByPayloadEffect(false, 1, (startTurn, endTurn, payload) => new PlayerLimitedToMoveSpecificChessStatus(BoardManager.getChessPieceAtPosition(payload.getTargetPositions()[0]), startTurn, endTurn)),
            new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessNonCostOnMoveStatus(startTurn, startTurn + 1)),
            new AddStatusToPayloadTargetChessEffect((startTurn) => new BattleHardenedPostAttackStatus(startTurn, startTurn + 1)));

        // 遣返
        cardRegistry[15] = new GetTargetPipelineEffect(new SelectReturnableChessEffect(false, ChessPiece.getAllChessTypes(true)), new ReturnToInitialPositionEffect());

        // 鐐銬
        cardRegistry[16] = new CompositeEffect(new ChessAmountCheckEffect(false, 4, false, false, ChessPiece.getAllChessTypes(false)), new ChessAmountCompareEffect(true, false),
            new AddSelectedChessMultipleStatusEffect(false, (startTurn) => new ChessStatus[] { new ChessCantMoveStatus(startTurn, startTurn + 2), new ChessCantBeAttackedStatus(startTurn, startTurn + 3) }, ChessPiece.getAllChessTypes(true)));

        // 一換一
        cardRegistry[17] = new GetTargetPipelineEffect(new SelectEqualButDifferentTeamChessEffect(ChessPiece.getAllChessTypes(true)), new RemoveChessAtTargetPositionEffect());

        // 施術準備
        cardRegistry[18] = new CompositeEffect(new DrawCardEffect(false, 2), new AddPlayerStatusEffect(false, 1, (startTurn, endTurn) => new PlayerCantUseSpecificCardStatus(18, startTurn, endTurn)));

        // 回收
        cardRegistry[19] = new RetrieveDiscardEffect(2, 19);

        // 冊封
        cardRegistry[20] = new CompositeEffect(new ChessAmountCheckEffect(true, 1, true, false, ChessType.Knight), new ChessAmountCheckEffect(true, 2, false, true, ChessType.Pawn),
            new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Pawn), new RemoveChessAtTargetPositionEffect(), new MaterializeCardEffect(false, 3, 1, true)));

        // 再填充
        cardRegistry[21] = new RedrawEffect(false);

        // 政治聯姻
        cardRegistry[22] = new CompositeEffect(new MaterializeCardEffect(false, 5, 1, true), new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new PlayerCantAttackStatus(startTurn, endTurn)),
            new AddPlayerStatusEffect(true, 2, (startTurn, endTurn) => new PlayerBonusManaStatus(2, startTurn, endTurn)));

        // 停戰協議
        cardRegistry[23] = new CompositeEffect(new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new PlayerCantAttackStatus(startTurn, endTurn)), new AddPlayerStatusEffect(true, 2, (startTurn, endTurn) => new PlayerCantAttackStatus(startTurn, endTurn)));

        // 求援
        cardRegistry[24] = new DrawRandomCardByIdsEffect(false, 1, 2, 3, 4, 5);

        // 千里馬
        cardRegistry[25] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Knight), new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessCantAttackSpecificTypeStatus(ChessType.King, startTurn, startTurn + 1)),
            new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessExtraMoveStatus(1, startTurn, startTurn + 1)), new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessNonCostOnMoveStatus(startTurn, startTurn + 1)));

        // 大飢荒
        cardRegistry[26] = new CompositeEffect(new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new PlayerCantSummonStatus(startTurn, endTurn)), new AddPlayerStatusEffect(true, 3, (startTurn, endTurn) => new PlayerCantSummonStatus(startTurn, endTurn)));

        // 通貨膨脹
        cardRegistry[27] = new CompositeEffect(new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new IncreasePlayerCardCostStatus(1, startTurn, endTurn)), new AddPlayerStatusEffect(false, 3, (startTurn, endTurn) => new IncreasePlayerMoveCostStatus(1, startTurn, endTurn)),
            new AddPlayerStatusEffect(true, 3, (startTurn, endTurn) => new IncreasePlayerCardCostStatus(1, startTurn, endTurn)), new AddPlayerStatusEffect(true, 3, (startTurn, endTurn) => new IncreasePlayerMoveCostStatus(1, startTurn, endTurn)));

        // 捨身取義
        cardRegistry[28] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessPiece.getAllChessTypes(true)), new RemoveChessAtTargetPositionEffect(), new ManaChangeEffect(false, 2), new DrawCardEffect(false, 1));

        // 御駕親征
        cardRegistry[29] = new CompositeEffect(new AddStatusToAllMatchingTypeChessEffect(true, (startTurn) => new ChessStatus[] { new ChessMovementOverrideStatus(new QueenMovement(), startTurn, startTurn + 1),
            new ChessCantAttackSpecificTypeStatus(ChessType.King, startTurn, startTurn + 1) }, true, ChessType.King));

        // 魔力爆發
        cardRegistry[30] = new CompositeEffect(new PlayerHadntMovedCheckEffect(false), new AddPlayerStatusEffect(false, 1, (startTurn, endTurn) => new PlayerCardNonCostStatus(startTurn, endTurn)),
            new AddPlayerStatusEffect(false, 1, (startTurn, endTurn) => new PlayerDiscardAllHandCardOnTurnEndStatus(startTurn, endTurn)));

        // 防禦陣型
        cardRegistry[31] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Pawn),
            new AddStatusToAreaChessEffect(true, 2, (startTurn, endTurn) => new ChessCantBeAttackedStatus(startTurn, endTurn), new ChessType[] { ChessType.Pawn }, (isLocal, payload) => payload.getTargetPositions(), AreaUtils.getSquareArea(1, true)));

        // 正面突破
        cardRegistry[32] = new AddStatusToAllMatchingTypeChessEffect(true, 1, (startTurn, endTurn) => new ChessMovementOverrideStatus(new FrontalBreakthroughPawnMovement(), startTurn, endTurn), false, ChessType.Pawn);

        // 猛撞
        cardRegistry[33] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Pawn), new AddStatusToPayloadTargetChessEffect((startTurn) => new PawnRammingStatus(startTurn, startTurn + 1)));

        // 踐踏
        cardRegistry[34] = new AddStatusToAllMatchingTypeChessEffect(true, 1, (startTurn, endTurn) => new KnightTrampleStatus(startTurn, endTurn), false, ChessType.Knight);

        // 回馬槍
        cardRegistry[35] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Knight), new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessPartingShotStatus(startTurn, startTurn + 1)));

        // 招降
        cardRegistry[36] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Bishop), new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessConversionStatus(startTurn, startTurn + 1)));

        // 宗教改革
        cardRegistry[37] = new GetTargetPipelineEffect(new SelectOrthogonalMoveEffect(ChessType.Bishop), new PipelineMovementEffect());

        // 異端審判
        cardRegistry[38] = new GetTargetPipelineEffect(new SelectChessEffect(true, ChessType.Bishop),
            new AddStatusToAreaChessEffect(false, 2, (startTurn, endTurn) => new IncreaseChessMoveCostStatus(1, startTurn, endTurn),
            ChessPiece.getAllChessTypes(false), (isLocal, payload) => payload.getTargetPositions(), (center, targetPosition) => AreaUtils.isOppositeColor(center, targetPosition), AreaUtils.getSquareArea(8, false)));

        // 堅固壁壘
        cardRegistry[39] = new AddStatusToAreaChessEffect(true, 2, (startTurn, endTurn) => new ChessCantBeAttackedStatus(startTurn, endTurn),
        ChessPiece.getAllChessTypes(false), (isLocal, payload) => { return BoardManager.getAllChessPieces(isLocal == GameManager.IsLocalWhite()).FindAll(c => c.getChessType() == ChessType.Rook).ConvertAll(c => c.getPosition()); }, AreaUtils.getSquareArea(1, false));

        // 割讓土地
        cardRegistry[40] = new GetTargetPipelineEffect(new SelectReturnableChessEffect(true, ChessType.Rook), new ReturnToInitialPositionEffect(), new AddStatusToPayloadTargetChessEffect((startTurn) => new ChessCantMoveStatus(startTurn, startTurn + 1)), new ManaChangeEffect(false, 2));

        // 威壓
        cardRegistry[41] = new AddStatusToAreaChessEffect(false, 2, (startTurn, endTurn) => new IncreaseChessMoveCostStatus(1, startTurn, endTurn),
            ChessPiece.getAllChessTypes(false), (isLocal, payload) => { return BoardManager.getAllChessPieces(isLocal == GameManager.IsLocalWhite()).FindAll(c => c.getChessType() == ChessType.Queen).ConvertAll(c => c.getPosition()); }, AreaUtils.getSquareArea(2, false));

        // 護駕
        cardRegistry[42] = new GetTargetPipelineEffect(new SelectOrthogonalMoveEffect(ChessType.Queen, ChessType.King), new PipelineMovementEffect(), new ConditionalSummonEffect(ChessType.Pawn, ChessType.Pawn, 8));

        // 大軍壓境
        cardRegistry[43] = new AddStatusToAllMatchingTypeChessEffect(true, (startTurn) => new ChessStatus[] { new ChessNonCostOnMoveStatus(startTurn, startTurn + 1), new ChessExtraMoveStatus(1, startTurn, startTurn + 1) }, false, ChessType.Pawn);

        // 雙管齊下
        cardRegistry[44] = new CompositeEffect(new AddPlayerStatusEffect(false, 1, (startTurn, endTurn) => new PlayerExtraMoveStatus(1, startTurn, endTurn)), new AddPlayerStatusEffect(false, 1, (startTurn, endTurn) => new PlayerCantAttackStatus(startTurn, endTurn)));

        // 干擾
        cardRegistry[45] = new CompositeEffect(new RetrieveAllHoldingCardEffect(true), new PlayerReplenishAndShuffleEffect(true), new AddPlayerStatusEffect(true, 1, 1, (startTurn, endTurn) => new PlayerBonusHandCardStatus(-1, startTurn, endTurn)));

        // 升變 - 騎士
        cardRegistry[46] = new GetTargetPipelineEffect(new SelectPromotablePawnEffect(true), new RemoveChessAtTargetPositionEffect(), new SummonChessAtTargetPositionEffect(ChessType.Knight));

        // 升變 - 城堡
        cardRegistry[47] = new GetTargetPipelineEffect(new SelectPromotablePawnEffect(true), new RemoveChessAtTargetPositionEffect(), new SummonChessAtTargetPositionEffect(ChessType.Rook));

        // 升變 - 主教
        cardRegistry[48] = new GetTargetPipelineEffect(new SelectPromotablePawnEffect(true), new RemoveChessAtTargetPositionEffect(), new SummonChessAtTargetPositionEffect(ChessType.Bishop));

        // 升變 - 皇后
        cardRegistry[49] = new GetTargetPipelineEffect(new SelectPromotablePawnEffect(true), new RemoveChessAtTargetPositionEffect(), new SummonChessAtTargetPositionEffect(ChessType.Queen));
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

        MatchDataManager.useCard(true, card, GameManager.getCurrentTurnCount());
        HandViewManager.destroyUsingCard();

        await executeCardEffect(true, card, payload);

    }

    public static async Task processLocalAiCard(Card card, EffectTargetPayload targets)
    {
        MatchDataManager.useCard(true, card, GameManager.getCurrentTurnCount());
        await executeCardEffect(true, card, targets);
    }

    public static async Task processEnemyAiCard(Card card, EffectTargetPayload targets)
    {
        MatchDataManager.useCard(false, card, GameManager.getCurrentTurnCount());
        await executeCardEffect(false, card, targets);
    }

    private static async Task executeCardEffect(bool isLocal, Card card, EffectTargetPayload targets)
    {
        if (cardRegistry.TryGetValue(card.getId(), out CardEffect effect))
            await effect.executeEffect(isLocal, card, targets);
        else
            Debug.LogWarning("未註冊的卡片 ID: " + card.getId());

        HandViewManager.tryRecalculateCost();
    }

    public static bool isRandomEffect(int cardId)
    {
        if (cardRegistry.TryGetValue(cardId, out CardEffect effect))
        {
            return effect.isRandomEffect();
        }
        return false;
    }

    public static int getCardIdByChessType(ChessType type)
    {
        switch (type)
        {
            case ChessType.Pawn:
                return 1;
            case ChessType.Bishop:
                return 2;
            case ChessType.Knight:
                return 3;
            case ChessType.Rook:
                return 4;
            case ChessType.Queen:
                return 5;
            default:
                return -1;
        }
    }
}