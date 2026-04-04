using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    private static AiManager instance;

    private PlayerData playerData;

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
        instance.playerData = new PlayerData(true, false, DEFAULT_AI_DECK_LIST);
        return instance.playerData;
    }

    public static PlayerData getAIPlayerData()
    {
        return instance.playerData;
    }

    public static bool isWhite()
    {
        return instance.playerData.IsWhite();
    }

    public static AiOption getBestMove()
    {
        List<Vector2Int> pieceList = BoardManager.getAllChessPiecesPos(isWhite());

        List<ChessMoveOption> moveOptions = new List<ChessMoveOption>();

        foreach (Vector2Int pos in pieceList)
            moveOptions.AddRange(AiEvaluator.getMoveOptionsByChessPiece(pos));

        AiOption bestMove = null;

        foreach (ChessMoveOption moveOption in moveOptions)
            if (bestMove == null || moveOption.getScore() > bestMove.getScore())
                bestMove = moveOption;

        return bestMove;
    }

    public static async Task doBestMove()
    {
        AiOption bestMove = getBestMove();

        if (bestMove != null)
        {
            await bestMove.execute();
        }
    }
}
