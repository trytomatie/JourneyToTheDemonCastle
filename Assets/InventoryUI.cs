using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public List<ItemSlotUI> itemSlots;
    public Inventory syncedInventory;
    public RectTransform selectedInventorySlot;

    private void Start()
    {
        syncedInventory = GameManager.Instance.player.GetComponent<Inventory>();
        syncedInventory.onInventoryUpdate += UpdateUI;
        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (i < syncedInventory.items.Count)
            {
                itemSlots[i].SetItem(syncedInventory.items[i]);
            }
            else
            {
                itemSlots[i].ClearSlot();
            }
        }
    }

    public void SelectSlot(int index)
    {
        selectedInventorySlot.position = itemSlots[index].transform.position;
    }
}
