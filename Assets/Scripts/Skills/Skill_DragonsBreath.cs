using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_DragonsBreath", menuName = "ScriptableObjects/Skills/Skill_DragonsBreath", order = 1)]
public class Skill_DragonsBreath : Skill
{
    private IEntityControlls controller;
    public GameObject fireBoltProjectile;
    private GameObject castingVFX;
    private GameObject spellReadyVFX;

    public int lifeTime = 10;


    public override void OnEnter(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        onEnterTime = Time.time;
        SpellIndicators.CallSpellIndicator(SpellIndicatorType.FromSource, 1, controller);
        castingVFX = null;
        controller.GetAnimator().SetBool("chrageStaff", true);
        spellReadyVFX = null;
    }
    public override void OnUpdate(GameObject source)
    {
        Debug.Log(onEnterTime);
        controller.ManualMovement();
        controller.CastRotation();
        if(CastingFinsihed() && spellReadyVFX== null)
        {
            spellReadyVFX = VFXManager.Instance.PlayFeedback(15, controller.GetGameObject().transform);
            spellReadyVFX.transform.parent = controller.VfxTransform;
        }
        if(CastingFinsihed())
        {
            VFXManager.Instance.PlayFeedback(15, controller.VfxTransform, controller.GetGameObject().transform.rotation);
            //GameObject go = Instantiate(fireBoltProjectile, controller.VfxTransform.position, controller.GetGameObject().transform.rotation);
            //go.GetComponent<FireBoltProjectile>().SetOwner(controller.GetGameObject());
            controller.SkillColldowns[controller.SkillIndex] = Time.time;
            spellReadyVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            Destroy(spellReadyVFX, 3);
            Destroy(castingVFX, 3);
            controller.SwitchState(PlayerState.Controlling);
            SpellIndicators.DismissSpellIndicator();
        }
        else
        {
            controller.SkillColldowns[controller.SkillIndex] = Time.time - skillCooldown+1;
            Destroy(castingVFX, 3);
        }
    }



    public override void OnExit(GameObject source)
    {
        controller.GetAnimator().SetBool("chrageStaff", false);
    }

    public override bool CheckSkillConditions(GameObject source)
    {
        controller = source.GetComponent<IEntityControlls>();
        if (controller.SkillColldowns[controller.SkillIndex] + skillCooldown < Time.time)
        {

                return true;

        }
        return false;
    }

    private bool CastingFinsihed()
    {
        return Time.time - onEnterTime > castTime;
    }
}
