using UnityEngine;

public class UiManager : MonoBehaviour
{
    private static UiManager instance;

    [SerializeField] private TMPro.TextMeshProUGUI deckCount, waitingCount, discardCount, manaCount;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
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

    public static void updateDeckCount(int count)
    {
        instance.deckCount.text = count.ToString();
    }

    public static void updateWaitingCount(int count)
    {
        instance.waitingCount.text = count.ToString();
    }

    public static void updateDiscardCount(int count)
    {
        instance.discardCount.text = count.ToString();
    }

    public static void updateManaCount(int count)
    {
        instance.manaCount.text = count.ToString();
    }
}
