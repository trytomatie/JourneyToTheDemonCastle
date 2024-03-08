using UnityEngine;
using static Unity.VisualScripting.Member;

[CreateAssetMenu(fileName = "Boots_ItemEffect", menuName = "ScriptableObjects/ItemInteractionEffects/Boots_ItemEffect", order = 1)]
public class Boots_ItemEffect : ItemInteractionEffects
{
    public int dashCount = 1;


    public override void OnEquip(GameObject source, Item item)
    {
        // Increase Dash count
    }

    public override void OnUnequip(GameObject source, Item item)
    {

    }

    public override void OnDrop(GameObject source, Item item)
    {
    }
}