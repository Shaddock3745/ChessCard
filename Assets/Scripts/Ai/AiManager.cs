using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    private static AiManager instance;

    private static readonly List<int> DEFAULT_AI_DECK_LIST = new List<int>(new int[] { 1, 1, 1, 1, 1, 2, 2, 3, 3, 4, 4, 5 });
    private static readonly int OPTION_BUFFER_TIME_MILLIS = 1000;

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

    public static AiOption getBestMove(PlayerData playerData)
    {
        List<AiOption> options = new List<AiOption>();

        if (!playerData.chessMovedThisTurn())
        {
            List<(ChessPiece, Vector2Int)> pieceList = BoardManager.getAllChessPieces(playerData.IsWhite());

            foreach (var (piece, pos) in pieceList)
                if (playerData.canMoveChess(piece))
                    options.AddRange(AiEvaluator.getMoveOptionsByChessPiece(piece));
        }

        List<Card> handCardList = playerData.getHandList();
        int currentMana = playerData.getMana();

        foreach (Card card in handCardList)
            options.AddRange(AiEvaluator.getCardUseOptionsByCard(playerData, card, currentMana));

        AiOption bestMove = null;

        foreach (AiOption moveOption in options)
            if (bestMove == null || moveOption.getScore() > bestMove.getScore())
                bestMove = moveOption;

        return bestMove;
    }

    public static async Task doBestMove(PlayerData playerData)
    {
        AiOption bestMove;

        while ((bestMove = getBestMove(playerData)) != null)
        {
            while (HandViewManager.isPlayingAnimation())
                await Task.Delay(100);

            await Task.Delay(OPTION_BUFFER_TIME_MILLIS);

            string result = "AI ";

            if (bestMove is CardUseOption cardUseOption)
                result += string.Format("use card : {0}", cardUseOption.getCardname());
            else if (bestMove is ChessMoveOption chessMoveOption)
                result += string.Format("move {0}", chessMoveOption.getChessName());

            Debug.Log(result);

            await bestMove.execute(playerData);
        }
    }

    public static PlayerData initAIPlayerData(bool isLocal, bool isWhite) => new PlayerData(true, isLocal, isWhite, DEFAULT_AI_DECK_LIST);

}
