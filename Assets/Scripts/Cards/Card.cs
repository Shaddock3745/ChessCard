using UnityEngine;

[System.Serializable]
public class Card
{
    private int id;
    private bool isExileOnUse = false;
    private int currentCost;

    public Card(int id)
    {
        this.id = id;
        currentCost = JsonReader.getCardCost(id);
    }

    public Card(int id, bool isExileOnUse)
    {
        this.id = id;
        this.isExileOnUse = isExileOnUse;
        currentCost = JsonReader.getCardCost(id);
    }

    public Card(int id, bool isExileOnUse, bool noConsumption)
    {
        this.id = id;
        this.isExileOnUse = isExileOnUse;

        if (noConsumption)
            currentCost = 0;
        else
            currentCost = JsonReader.getCardCost(id);
    }

    public void resentCost()
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
}
