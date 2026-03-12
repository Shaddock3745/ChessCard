using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private static AIManager instance;

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


    public static MoveOption getBestMove(bool isWhite)
    {
        List<(ChessPiece,Vector2Int)> pieceList = BoardManager.getAllChessPieces(isWhite);

        List<MoveOption> moveOptions = new List<MoveOption>();

        foreach ((ChessPiece piece, Vector2Int pos) in pieceList)
            moveOptions.AddRange(AIEvaluator.getMoveByChessPiece(piece, pos));

        MoveOption bestMove = null;

        foreach (MoveOption moveOption in moveOptions)
            if (bestMove == null || moveOption.score > bestMove.score)
                bestMove = moveOption;

        return bestMove;
    }

}
