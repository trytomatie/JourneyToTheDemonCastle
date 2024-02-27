using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    public Image sprite;
    public TextMeshProUGUI amountText;
    public MMF_Player feedback;
    public int currentAmount;

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
        if (item == null)
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
}
