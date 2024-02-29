using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "StaffWeapon_ItemEffects", menuName = "ScriptableObjects/ItemInteractionEffects/StaffWeapon_ItemEffects", order = 1)]
public class StaffWeapon_ItemEffects : ItemInteractionEffects
{
    public int attackDamage = 1;
    public GameObject weaponPrefab;
    private GameObject instaniatedWeapon;
    public override void OnUse(GameObject source, Item item)
    {
        if(isUsing)
        {
            source.GetComponent<PlayerController>().HandleStaffCharge(true);
        }

    }

    public override void OnUseEnd(GameObject source,Item item)
    {
        source.GetComponent<PlayerController>().HandleStaffCharge(false);
    }


    public override void OnEquip(GameObject source, Item item)
    {
        Transform weaponPivot = source.GetComponent<PlayerController>().weaponPivot.transform;
        instaniatedWeapon = Instantiate(weaponPrefab, weaponPivot);
        instaniatedWeapon.transform.localScale = weaponPrefab.transform.localScale;
        source.GetComponent<StatusManager>().weaponAttackDamage = attackDamage;
        Debug.Log("Equipping " + item.id);
    }

    public override void OnUnequip(GameObject source, Item item)
    {
        source.GetComponent<StatusManager>().weaponAttackDamage = 0;
        Destroy(instaniatedWeapon);
    }

    public override void OnDrop(GameObject source, Item item)
    {
        Debug.Log("Dropping " + item.id);
    }
}