using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public int space = 20;
    public delegate void ContainerUpdated();
    public ContainerUpdated onInventoryUpdate;
    public Stash stash;

    private void Start()
    {
        AddItem(new Item(7, 1));
        AddItem(new Item(8, 1));
    }

    public bool AddItem(Item item)
    {
        int pos = ContainsItemAndHasSpace(item);
        if (pos != -1)
        {
            int maxStack = items[pos].maxStackSize;
            if (items[pos].amount + item.amount <= maxStack) // If the item can fit in the stack
            {
                items[pos].amount += item.amount;
                stash.AddItem(item, item.amount, this);
                onInventoryUpdate?.Invoke();
                return true;
            }
            else // If the item can't fit in the stack
            {
                item.amount -= maxStack - items[pos].amount;
                items[pos].amount = maxStack;
                if (items.Count < space)
                {
                    print(item.amount);
                    AddItem(item);
                    onInventoryUpdate?.Invoke();
                    return true;
                }
                else
                {
                    // TODO: Drop item on the ground
                }
            }
        }
        else if (items.Count < space)
        {
            items.Add(item);
            stash.AddItem(item, item.amount, this);
            onInventoryUpdate?.Invoke();
            return true;
        }
        return false;
    }


    public void RemoveItem(int id, int amount)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == id)
            {
                if (items[i].amount > amount)
                {
                    items[i].amount -= amount;
                    stash.RemoveItem(id, amount, this);
                    onInventoryUpdate.Invoke();
                    return;
                }
                else if (items[i].amount == amount)
                {
                    items.RemoveAt(i);
                    stash.RemoveItem(id, amount, this);
                    onInventoryUpdate.Invoke();
                    return;
                }
                else
                {
                    amount -= items[i].amount;
                    stash.RemoveItem(id, items[i].amount, this);
                    items.RemoveAt(i);
                }
            }
        }
    }

    public int ContainsItemAndHasSpace(Item item)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].id == item.id)
            {
                if (items[i].maxStackSize != items[i].amount)
                {
                    return i;
                }
            }
        }
        return -1;
    }
}
