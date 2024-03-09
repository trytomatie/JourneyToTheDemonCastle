using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Boots_ItemEffect", menuName = "ScriptableObjects/ItemInteractionEffects/Boots_ItemEffect", order = 1)]
public class Boots_ItemEffect : ItemInteractionEffects
{
    public int dashLimit = 1;


    public override void OnEquip(GameObject source, Item item)
    {
        source.GetComponent<PlayerController>().dashLimit = dashLimit;
    }

    public override void OnUnequip(GameObject source, Item item)
    {
        source.GetComponent<PlayerController>().dashLimit = 0;
    }

    public override void OnDrop(GameObject source, Item item)
    {
    }
}