using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private static AIManager instance;

    private PlayerData playerData;

    private static readonly List<int> DEFAULT_AI_DECK_LIST = new List<int>(new int[] { 1, 1, 1, 1, 1, 2, 2, 3, 3, 4, 4, 5 }) ;

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

    public static List<int> getAIDeckList()
    {
        return new List<int>(DEFAULT_AI_DECK_LIST);
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

    public static MoveOption getBestMove()
    {
        List<(ChessPiece,Vector2Int)> pieceList = BoardManager.getAllChessPieces(isWhite());

        List<MoveOption> moveOptions = new List<MoveOption>();

        foreach ((ChessPiece piece, Vector2Int pos) in pieceList)
            moveOptions.AddRange(AIEvaluator.getMoveByChessPiece(piece, pos));

        MoveOption bestMove = null;

        foreach (MoveOption moveOption in moveOptions)
            if (bestMove == null || moveOption.score > bestMove.score)
                bestMove = moveOption;

        return bestMove;
    }

    public static async Task doBestMove()
    {
        MoveOption bestMove = getBestMove();

        if (bestMove != null)
        {
            if(BoardManager.isEmpty(bestMove.targetPos))
                await BoardManager.movePieceWithAnimation(bestMove.currentPos, bestMove.targetPos);
            else
                await BoardManager.attackPieceWithAnimation(bestMove.currentPos, bestMove.targetPos);
        }
    }
}
