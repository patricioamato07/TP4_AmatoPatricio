using System;
using System.Collections.Generic;
using UnityEngine;

public class PartyInventory : MonoBehaviour
{
    [Serializable] public class Slot { public ItemData item; public int count; }
    public List<Slot> slots = new();

    public int Count(ItemData item)
    {
        foreach (var s in slots) if (s.item == item) return s.count;
        return 0;
    }

    public bool TryConsume(ItemData item)
    {
        foreach (var s in slots)
            if (s.item == item && s.count > 0) { s.count--; return true; }
        return false;
    }
}
