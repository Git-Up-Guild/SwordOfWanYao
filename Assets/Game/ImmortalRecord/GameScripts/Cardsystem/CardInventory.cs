using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]

public class CardInventory : ScriptableObject
{
    public List<Items> Items;

    public int Wave;
}
[Serializable]
public class Items
{
    public int ID;
    public int Quantity;
}