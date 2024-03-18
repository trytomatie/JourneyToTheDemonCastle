using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemBlueprint", menuName = "ScriptableObjects/ItemBlueprint", order = 1)]
public class ItemBlueprint : ScriptableObject
{
    public int id;
    public ItemType itemType = ItemType.None;
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
    public int maxStackSize;
    public bool keyItem = false;
    public ItemInteractionEffects itemInteractionEffects;

}

[Serializable]
public class Item
{
    public int id;
    public int amount;
    public int maxStackSize => ItemDatabase.GetItem(id).maxStackSize;

    public Item(int id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }

    public ItemBlueprint BluePrint
    {
        get
        {
            return ItemDatabase.GetItem(id);
        }
    }

    public ItemInteractionEffects GetItemInteractionEffects
    {
        get
        {
            return ItemDatabase.GetItem(id).itemInteractionEffects;
        }
    }
}


