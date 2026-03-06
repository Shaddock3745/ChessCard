using UnityEngine;

public class CardEffectManager : MonoBehaviour
{
    private static CardEffectManager instance;

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

    public static void processCardEffect(Card card)
    {
        Debug.Log("Processing card effect: " + card.getName());
    }
}
