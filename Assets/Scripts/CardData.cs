using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "CardData")]
public class CardData : ScriptableObject
{
    [SerializeField] private int id;

    public int getCost()
    {
        return 0;
    }
}
