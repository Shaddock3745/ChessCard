[System.Serializable]
public class Card
{
    private readonly bool isVirtual;
    private int id;
    private bool isExileOnUse = false;
    private int currentCost;

    public Card(int id)
    {
        this.id = id;
        currentCost = JsonReader.getCardCost(id);
    }

    public Card(int id, bool isExileOnUse, bool isVirtual = false)
    {
        this.id = id;
        this.isExileOnUse = isExileOnUse;
        currentCost = JsonReader.getCardCost(id);
        this.isVirtual = isVirtual;
    }

    public Card(int id, bool isExileOnUse, bool noConsumption, bool isVirtual = false)
    {
        this.id = id;
        this.isExileOnUse = isExileOnUse;

        if (noConsumption)
            currentCost = 0;
        else
            currentCost = JsonReader.getCardCost(id);

        this.isVirtual = isVirtual;
    }

    public bool IsVirtual() => isVirtual;

    public void resetCost()
    {
        currentCost = JsonReader.getCardCost(id);
    }

    public int getCost()
    {
        return currentCost;
    }

    public string getName()
    {
        return JsonReader.getCardName(id);
    }

    public string getDescription()
    {
        return JsonReader.getCardDescription(id);
    }

    public bool isExileOnUseCard()
    {
        return isExileOnUse;
    }

    public int getId()
    {
        return id;
    }

    public Card Clone()
    {
        Card clone = new Card(this.id, this.isExileOnUse, true);
        clone.currentCost = this.currentCost;
        return clone;
    }
}
