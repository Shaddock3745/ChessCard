using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PieceVisualEntry
{
    public ChessType type;
    public Sprite whiteSprite;
    public Sprite blackSprite;
}

[CreateAssetMenu(fileName = "PieceSpriteLibrary", menuName = "Scriptable Objects/PieceSpriteLibrary")]
public class PieceSpriteLibrary : ScriptableObject
{
    public List<PieceVisualEntry> pieceVisuals;

    public Sprite getSprite(ChessType type, bool isWhite)
    {
        var entry = pieceVisuals.Find(v => v.type == type);
        return isWhite ? entry.whiteSprite : entry.blackSprite;
    }
}
