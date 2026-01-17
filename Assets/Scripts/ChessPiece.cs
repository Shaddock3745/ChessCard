using System.Collections.Generic;
using UnityEngine;
using static MovementStrategy;

public class ChessPiece : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PieceData pieceBasicData;
    private MovementStrategy.MovementType movementType;

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

        switch (data.getChessType())
        {
            // todo
        }
    }

    public bool isWhite()
    {
        return pieceBasicData.isWhite();
    }

    public List<MoveAction> getAvailableMoves(Vector2Int currentPos)
    {
        return movementType.getAvailableMoves(currentPos, pieceBasicData.isWhite());
    }
}
