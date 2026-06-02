using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AiManager : MonoBehaviour
{
    private static AiManager instance;

    private static readonly List<int> DEFAULT_AI_DECK_LIST = new List<int>(new int[] { 1, 1, 1, 1, 1 });
    private static readonly int OPTION_BUFFER_TIME_MILLIS = 1000;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public static AiOption getBestMove(PlayerData playerData, int currentTurnCount)
    {
        List<AiOption> options = new List<AiOption>();

        if (!ActionValidator.reachMoveLimit(playerData, currentTurnCount))
        {
            List<ChessPiece> pieceList = BoardManager.getAllChessPieces(playerData.IsWhite());

            foreach (var piece in pieceList)
                options.AddRange(AiEvaluator.getMoveOptionsByChessPiece(playerData, piece, currentTurnCount));
        }

        List<Card> handCardList = playerData.getHandList();
        int currentMana = playerData.getMana();

        foreach (Card card in handCardList)
            options.AddRange(AiEvaluator.getCardUseOptionsByCard(playerData, card, currentMana, currentTurnCount));

        AiOption bestMove = null;

        foreach (AiOption moveOption in options)
            if (bestMove == null || moveOption.getScore() > bestMove.getScore())
                bestMove = moveOption;

        return bestMove;
    }

    public static async Task doBestMove(PlayerData playerData, int currentTurnCount)
    {
        AiOption bestMove;

        while (!GameManager.IsGameOver() && (bestMove = getBestMove(playerData, currentTurnCount)) != null)
        {
            while (HandViewManager.isPlayingAnimation())
                await Task.Delay(100);

            await Task.Delay(OPTION_BUFFER_TIME_MILLIS);

            await bestMove.execute(playerData);
        }

        while (HandViewManager.isPlayingAnimation())
            await Task.Delay(100);

        if (!GameManager.IsGameOver())
            GameManager.ChangeTurn();
    }

    public static PlayerData initAIPlayerData(bool isLocal, bool isWhite) => new PlayerData(true, isLocal, isWhite, DEFAULT_AI_DECK_LIST);

}
