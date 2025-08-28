using System;
using UnityEngine;

[Serializable]
public class MarketEntry
{
    public Item data;
    public int unitPrice;
    public int quantity;
    public int totalPrice => unitPrice * quantity;
}
