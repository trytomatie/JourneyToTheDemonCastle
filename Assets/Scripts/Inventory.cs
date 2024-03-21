using System;
using System.Collections;

public class Inventory : Container
{
    public override void Init()
    {
        AddItem(new Item(7, 1));
        AddItem(new Item(8, 1));
        AddItem(new Item(17, 1,0));
        AddItem(new Item(17, 1, 1));
        AddItem(new Item(17, 1, 2));
        AddItem(new Item(17, 1, 3));
        AddItem(new Item(17, 1, 4));
        AddItem(new Item(17, 1, 5));
        AddItem(new Item(17, 1, 6));
        onItemCompletlyRemoved += RemoveHotbarItemIfRemoved;
        onInventoryUpdate += EquipNewHotBarItem;
    }
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

    public void RemoveHotbarItemIfRemoved(int i)
    {
        print(i + " " + currentHotbarIndex);
        if(i == currentHotbarIndex)
        {
            items[currentHotbarIndex].GetItemInteractionEffects.OnUnequip(gameObject, items[currentHotbarIndex]);
        }
    }

    public void EquipNewHotBarItem(int i)
    {
        if(i == currentHotbarIndex)
        {
            items[currentHotbarIndex].GetItemInteractionEffects.OnEquip(gameObject, items[currentHotbarIndex]);
        }
    }
}
