[System.Serializable]
public class Card
{
    private static int serialNumber = 0;
    private readonly bool isVirtual;
    private readonly int uniqueId;
    private int id;
    private bool isExileOnUse = false;
    private int currentCost;

    private static int getNextSerialNumber() => System.Threading.Interlocked.Increment(ref serialNumber);

    public Card(int id, bool isExileOnUse = false, bool isVirtual = false, bool noConsumption = false)
    {
        this.uniqueId = getNextSerialNumber();
        this.id = id;
        this.isExileOnUse = isExileOnUse;

        if (noConsumption)
            currentCost = 0;
        else
            currentCost = JsonReader.getCardCost(id);

        this.isVirtual = isVirtual;
    }

    private Card(Card cardToClone)
    {
        this.uniqueId = cardToClone.uniqueId;
        this.id = cardToClone.id;
        this.isExileOnUse = cardToClone.isExileOnUse;
        this.currentCost = cardToClone.currentCost;
        this.isVirtual = true;
    }

    public Card Clone()
    {
        return new Card(this);
    }

    public int getUniqueId() => uniqueId;

    public bool IsVirtual() => isVirtual;

    public void resetCost()
    {
        currentCost = JsonReader.getCardCost(id);
    }

    public int getCost() => currentCost;

    public string getName() => JsonReader.getCardName(id, GameManager.getCurrentLanguage());

    public string getDescription() => JsonReader.getCardDescription(id, GameManager.getCurrentLanguage());

    public bool isExileOnUseCard() => isExileOnUse;

    public int getId() => id;

}
