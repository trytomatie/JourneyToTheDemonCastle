using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class Skill : ScriptableObject
{
    public string skillName;
    public Sprite skillIcon;
    public float skillCooldown = 3;
    public float skillColldownTimer = 0;
    public virtual void OnEnter(GameObject source)
    {

    }

    public virtual void OnUpdate(GameObject source)
    {

    }

    public virtual void OnExit(GameObject source)
    {

    }

    public virtual bool CheckSkillConditions(GameObject source)
    {
        return true;
    }

    public float GetCooldown() 
    { 
        return Mathf.Clamp(skillColldownTimer + skillCooldown - Time.time,0,999);
    }

    public void AssignSkill(GameObject source)
    {
        Skill instance = Instantiate(this);
        source.GetComponent<PlayerController>().skills[0] = instance;
        GameUI.instance.skillslots[0].SetupSkill(instance);
    }

    public void RemoveSkill(GameObject source)
    {
        source.GetComponent<PlayerController>().skills[0] = null;
        GameUI.instance.skillslots[0].SetupSkill(null);
    }

}