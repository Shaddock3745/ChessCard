using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    private static AiManager instance;

    private PlayerData aiPlayerData;

    private static readonly List<int> DEFAULT_AI_DECK_LIST = new List<int>(new int[] { 1, 1, 1, 1, 1, 2, 2, 3, 3, 4, 4, 5 });

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

    public static PlayerData initAIPlayerData()
    {
        instance.aiPlayerData = new PlayerData(true, false, DEFAULT_AI_DECK_LIST);
        return instance.aiPlayerData;
    }

    public static PlayerData getAIPlayerData()
    {
        return instance.aiPlayerData;
    }

    public static bool isWhite()
    {
        return instance.aiPlayerData.IsWhite();
    }

    public static AiOption getBestMove()
    {
        List<AiOption> options = new List<AiOption>();

        if (!instance.aiPlayerData.chessMovedThisTurn())
        {
            List<(ChessPiece, Vector2Int)> pieceList = BoardManager.getAllChessPieces(isWhite());

            foreach (var (piece, pos) in pieceList)
                if (instance.aiPlayerData.canMoveChess(piece))
                    options.AddRange(AiEvaluator.getMoveOptionsByChessPiece(piece, pos));
        }

        List<Card> handCardList = instance.aiPlayerData.getHandList();
        int currentMana = instance.aiPlayerData.getMana();

        foreach (Card card in handCardList)
            options.AddRange(AiEvaluator.getCardUseOptionsByCard(card, currentMana));

        AiOption bestMove = null;

        foreach (AiOption moveOption in options)
            if (bestMove == null || moveOption.getScore() > bestMove.getScore())
                bestMove = moveOption;

        return bestMove;
    }

    public static async Task doBestMove()
    {
        AiOption bestMove;

        while ((bestMove = getBestMove()) != null)
        {
            while (HandViewManager.isPlayingAnimation())
                await Task.Delay(100);

            await Task.Delay(300);

            string result = "AI ";

            if (bestMove is CardUseOption cardUseOption)
                result += string.Format("use card : {0}", cardUseOption.getCardname());
            else if (bestMove is ChessMoveOption chessMoveOption)
                result += string.Format("move {0}", chessMoveOption.getChessName());

            Debug.Log(result);

            await bestMove.execute();
        }
    }
}
