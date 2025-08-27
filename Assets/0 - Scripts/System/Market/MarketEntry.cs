using System;
using UnityEngine;

[Serializable]
public class MarketEntry
{
    public EquipmentData data;
    public int unitPrice;
    public int quantity;
    public int totalPrice => unitPrice * quantity;
}
