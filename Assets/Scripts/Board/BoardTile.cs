using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public class BoardTile : MonoBehaviour
{
    [SerializeField] private Sprite targetSprite;
    [SerializeField] private Sprite moveableSprite;
    [SerializeField] private Sprite attackableSprite;
    [SerializeField] private Sprite selectableSprite;

    private static readonly float FADE_DURATION = 0.5f;

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

    private void resetVisual()
    {
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

    public async Task showTargetWithAnimation()
    {
        spriteRenderer.DOKill();
        spriteRenderer.sprite = targetSprite;
        Color color = spriteRenderer.color;
        color.a = 0f;
        spriteRenderer.color = color;
        await spriteRenderer.DOFade(1f, FADE_DURATION).AsyncWaitForCompletion();
    }

    public void removeHint()
    {
        isMoveable = false;
        isAttackable = false;
        isSelectable = false;
        spriteRenderer.DOKill();
        spriteRenderer.sprite = null;
    }

    public bool isMoveableTile() => isMoveable;

    public bool isAttackableTile() => isAttackable;

    public bool isSelectableTile() => isSelectable;

    public int getX() => indexX;
    public int getY() => indexY;

    public Vector2Int getPosition() => pos;
}
