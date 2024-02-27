using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TalentManager : MonoBehaviour
{
    private List<Talent> activeTalents = new List<Talent>();

    public Talent[] talents;

    public void ActivateTalent(int index)
    {
        Talent talent = talents[index];
        talent.talentFunctionality.ApplyTalentEffect();
        activeTalents.Add(talent);
    }

    public void DeactivateTalent(int index)
    {
        Talent talent = talents[index];
        talent.talentFunctionality.RemoveTalentEffect();
        activeTalents.Remove(talent);
    }
}

public enum TalentType
{
    Hero,
    Mage,
    Warrior,
    Rogue,
}



public class Talent : ScriptableObject
{
    public TalentType talentType;
    public int tier = 1;
    public string talentName;
    public string talentDescription;
    public Sprite talentIcon;
    public TalentFunctionality talentFunctionality;
}

public class TalentFunctionality : ScriptableObject
{
    public virtual void ApplyTalentEffect()
    {
        Debug.Log("Talent effect applied");
    }

    public virtual void RemoveTalentEffect()
    {
        Debug.Log("Talent effect removed");
    }
}

