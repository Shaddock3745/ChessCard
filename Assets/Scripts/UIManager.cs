using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private enum PositionType
    {
        None = 0,
        Moveable = 1,
        Attackable = 2,
        Target = 3
    }

    [Serializable]
    private struct TypeSpriteConfig
    {
        public PositionType type;
        public Sprite sprite;
    }

    private static UIManager instance;

    private static readonly float tileSize = 0.96f;
    private static readonly float startOffeset = -3.36f;

    [Header("Grid Settings (8x8)")]
    [SerializeField] private GameObject positionHintItemPrefab;
    [SerializeField] private List<TypeSpriteConfig> typeSpriteConfigs;

    private PositionType[,] boardTypes = new PositionType[8, 8];
    private GameObject[,] boardItems = new GameObject[8, 8];
    private Dictionary<PositionType, Sprite> typeSpriteMap;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        initDictionary();
    }

    private void initDictionary() {
        typeSpriteMap = new Dictionary<PositionType, Sprite>();

        foreach (var config in typeSpriteConfigs)
            typeSpriteMap[config.type] = config.sprite;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initBoard();
    }

    private void initBoard()
    {
        for(int x = 0; x < 8; x++)
        {
            for(int y = 0; y < 8; y++)
            {
                GameObject item = Instantiate(positionHintItemPrefab, transform);
                item.transform.position = new Vector2(startOffeset + x * tileSize, startOffeset + y * tileSize);
                item.name = $"PositionHint_{x}_{y}";
                boardItems[x,y] = item;

                setBlock(x, y, PositionType.None);
            }
        }
    }

    private void setBlock(int x, int y, PositionType type)
    {
        boardTypes[x, y] = type;

        if(type == PositionType.None)
        {
            boardItems[x,y].GetComponent<SpriteRenderer>().sprite = null;
            boardItems[x,y].GetComponent<SpriteRenderer>().color = new Color(1,1,1,0);
        }
        else
        {
            boardItems[x,y].GetComponent<SpriteRenderer>().sprite = typeSpriteMap[type];
            boardItems[x,y].GetComponent<SpriteRenderer>().color = Color.white;
        }

    }

    private void clearAll()
    {
        for(int x = 0; x < 8; x++)
        {
            for(int y = 0; y < 8; y++)
            {
                setBlock(x, y, PositionType.None);
            }
        }   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void setMoveableHint(int x, int y)
    {
        instance.setBlock(x, y, PositionType.Moveable);
    }

    public static void setAttackableHint(int x, int y)
    {
        instance.setBlock(x, y, PositionType.Attackable);
    }

    public static void setTargetHint(int x, int y)
    {
        instance.setBlock(x, y, PositionType.Target);
    }

    public static void clearHint(int x, int y)
    {
        instance.setBlock(x, y, PositionType.None);
    }

    public static void clearAllHints()
    {
        instance.clearAll();
    }
}
