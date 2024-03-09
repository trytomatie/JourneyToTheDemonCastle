using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemInteractionEffects.EquipmentType slotRestriction = ItemInteractionEffects.EquipmentType.None;
    public Image sprite;
    public TextMeshProUGUI amountText;
    public MMF_Player feedback;
    public Container syncedContainer;
    public int currentAmount;
    public int assignedIndex = 0;

    private void Awake()
    {
        ClearSlot();
    }
    public void ClearSlot()
    {
        sprite.enabled = false;
        sprite.sprite = null;
        amountText.text = "";
        currentAmount = 0;
    }
    public void SetItem(Item item)
    {
        if (item == null || item.id == 0)
        {
            ClearSlot();
            return;
        }

        sprite.enabled = true;
        sprite.sprite = ItemDatabase.GetItem(item.id).itemIcon;
        if (item.amount != currentAmount)
        {
            feedback.PlayFeedbacks();
            currentAmount = item.amount;
        }
        amountText.text = $"x{item.amount}";
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        InventoryUI.Instance.dragImage.sprite = sprite.sprite;
        InventoryUI.Instance.dragImage.enabled = true;
        InventoryUI.Instance.dragImage.transform.position = mousePos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        InventoryUI.Instance.dragImage.transform.position = mousePos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        ItemSlotUI dragedOverItemSlot = eventData.pointerCurrentRaycast.gameObject.GetComponent<ItemSlotUI>();
        if(dragedOverItemSlot != null)
        {
            if (dragedOverItemSlot.slotRestriction != ItemInteractionEffects.EquipmentType.None && dragedOverItemSlot.slotRestriction != ItemDatabase.GetItem(syncedContainer.items[assignedIndex].id).itemInteractionEffects.equipmentType)
            {
                return;
            }
            if(slotRestriction != ItemInteractionEffects.EquipmentType.None && slotRestriction != ItemDatabase.GetItem(dragedOverItemSlot.syncedContainer.items[dragedOverItemSlot.assignedIndex].id).itemInteractionEffects.equipmentType)
            {
                return;
            }
            Container.SwapItems(dragedOverItemSlot.syncedContainer, syncedContainer, dragedOverItemSlot.assignedIndex, assignedIndex);
        }
        InventoryUI.Instance.dragImage.enabled = false;
    }
}
