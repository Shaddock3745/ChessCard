using UnityEngine;

public class BoardTile : MonoBehaviour
{
    [SerializeField] private Sprite targetSprite;
    [SerializeField] private Sprite moveableSprite;
    [SerializeField] private Sprite attackableSprite;
    [SerializeField] private Sprite selectableSprite;

    private SpriteRenderer spriteRenderer;
    private bool isMoveable = false;
    private bool isAttackable = false;
    private bool isSelectable = false;
    private int indexX = -1;
    private int indexY = -1;
    private Vector2Int pos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void initPos(int x, int y)
    {
        indexX = x;
        indexY = y;
        pos = new Vector2Int(x, y);
    }

    public void showMoveable()
    {
        isMoveable = true;
        spriteRenderer.sprite = moveableSprite;
    }

    public void showAttackable()
    {
        isAttackable = true;
        spriteRenderer.sprite = attackableSprite;
    }

    public void showSelectable()
    {
        isSelectable = true;
        spriteRenderer.sprite = selectableSprite;
    }

    public void showTarget()
    {
        spriteRenderer.sprite = targetSprite;
    }

    public void removeHint()
    {
        isMoveable = false;
        isAttackable = false;
        isSelectable = false;
        spriteRenderer.sprite = null;
    }

    public bool isMoveableTile() => isMoveable;

    public bool isAttackableTile() => isAttackable;

    public bool isSelectableTile() => isSelectable;

    public int getX() => indexX;
    public int getY() => indexY;

    public Vector2Int getPos() => pos;
}
