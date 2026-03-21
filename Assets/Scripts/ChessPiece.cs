using System.Collections.Generic;
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PieceData pieceBasicData;
    private MovementType movementType;
    private int extraMoveCost = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPieceData(PieceData data)
    {
        pieceBasicData = data;
        spriteRenderer.sprite = pieceBasicData.getPieceSprite();
        gameObject.name = (pieceBasicData.isWhite()?"White_":"Black_") + pieceBasicData.getChessType().ToString();

        movementType = data.getChessType() switch
        {
            ChessType.Pawn => new PawnMovement(),
            ChessType.Rook => new RookMovement(),
            ChessType.Bishop => new BishopMovement(),
            ChessType.Knight => new KnightMovement(),
            ChessType.Queen => new QueenMovement(),
            ChessType.King => new KingMovement(),
            _ => null
        };        
    }

    public ChessType getChessType()
    {
        return pieceBasicData.getChessType();
    }

    public string getName()
    {
        return pieceBasicData.getChessType().ToString();
    }

    public bool isWhite()
    {
        return pieceBasicData.isWhite();
    }

    public List<MoveAction> getAvailableMoves(Vector2Int currentPos)
    {
        return movementType.getAvailableMoves(currentPos, pieceBasicData.isWhite());
    }

    public void moveToPoint(Vector3 point)
    {
        transform.position = point;
    }

    public int getExtraMoveCost()
    {
        return extraMoveCost;
    }

    public void setExtraMoveCost(int cost)
    {
        extraMoveCost = cost;
    }
}
