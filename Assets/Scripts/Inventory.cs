using System;
using System.Collections;

public class Inventory : Container
{

    private int currentHotbarIndex = 0;
    public Item CurrentHotbarItem
    {
        get
        {
            if (items.Count - 1 < currentHotbarIndex) return null;
            return items[currentHotbarIndex];
        }
    }
    public void SwitchHotbarItem(int index)
    {
        GameUI.instance.inventoryUI.SelectSlot(index);
        if (items.Count-1 >= currentHotbarIndex)
        {
            items[currentHotbarIndex].GetItemInteractionEffects.OnUnequip(gameObject, items[currentHotbarIndex]);
        }
        currentHotbarIndex = index;
        if(items.Count-1 >= currentHotbarIndex)
        {
            items[currentHotbarIndex].GetItemInteractionEffects.OnEquip(gameObject, items[currentHotbarIndex]);
        }

    }
}
