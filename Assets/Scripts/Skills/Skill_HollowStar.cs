using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_HollowStar", menuName = "ScriptableObjects/Skills/Skill_HollowStar", order = 1)]
public class Skill_HollowStar : Skill
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
        SpellIndicators.CallSpellIndicator(SpellIndicatorType.CursorPlaced, 0, controller,castTime);
        controller.GetAnimator().SetBool("chrageStaff", true);
        spellReadyVFX = null;
        castingVFX = VFXManager.Instance.PlayFeedback(16, controller.GetGameObject().transform);
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
            //GameObject go = Instantiate(fireBoltProjectile, controller.VfxTransform.position, controller.GetGameObject().transform.rotation);
            //go.GetComponent<FireBoltProjectile>().SetOwner(controller.GetGameObject());
            controller.SkillColldowns[controller.SkillIndex] = Time.time;
            spellReadyVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            Destroy(spellReadyVFX, 5);
            Destroy(castingVFX, 5);
            controller.SwitchState(PlayerState.Controlling);
            castingVFX.GetComponent<VFXController>().SpearDestination(GameManager.CurosrPosition);
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
