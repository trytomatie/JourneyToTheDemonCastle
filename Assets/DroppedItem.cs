using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

public class DroppedItem : MonoBehaviour
{
    private Rigidbody rb;
    public float force;
    public ItemBlueprint droppedItem;
    public int amount = 1;
    public SpriteRenderer spriteRenderer;
    public TextMeshPro amountText;
    public bool isKeyItem = false;
    private Transform target;
    public GameObject keyItemVFX;

    private bool hitGround = false;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Random.onUnitSphere + new Vector3(0,force,0);
        target = GameManager.Instance.player.transform;
    }

    public void SetUpDroppedItem(ItemBlueprint item, int amount)
    {
        spriteRenderer.sprite = item.itemIcon;
        droppedItem = item;
        this.amount = amount;
        isKeyItem = item.keyItem;
        keyItemVFX.SetActive(isKeyItem);
        amountText.text = "x" + amount.ToString();
    }

    private void Update()
    {
        if (!hitGround) return;

        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 1.5f)
        {
            if (distance < 0.1f)
            {
                PickUpItem();
            }
            Vector3 dir =  target.position- transform.position;
            transform.position += dir.normalized * 7 * Time.deltaTime;
        }
    }

    private void PickUpItem()
    {
        if (isKeyItem)
        {
            Item item = new Item(droppedItem.id, amount);
            target.GetComponent<Inventory>().AddItem(item);
            GameUI.instance.SetUpGetItemScreen(item);
        }
        else
        {
            target.GetComponent<Inventory>().AddItem(new Item(droppedItem.id, amount));
        }
        AudioManager.PlayGeneralSound(transform.position, 1);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        hitGround = true;
    }

}
