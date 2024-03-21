using UnityEngine;
using static PlayerController;
using UnityEngine.InputSystem;
using static Unity.VisualScripting.Member;
using Unity.VisualScripting;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "Skill_FireBolt", menuName = "ScriptableObjects/Skills/Skill_FireBolt", order = 1)]
public class Skill_FireBolt : Skill
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

        castingVFX = VFXManager.Instance.PlayFeedback(13, controller.CastingPivot, controller.CastingPivot.transform.rotation);
        castingVFX.transform.parent = controller.CastingPivot;
        castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
        ParticleSystem.MainModule main= castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().main;
        main.duration = castTime-0.4f;
        castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        controller.GetAnimator().SetBool("chrageStaff", true);
        spellReadyVFX = null;
    }
    public override void OnUpdate(GameObject source)
    {
        controller.ManualMovement();
        controller.CastRotation();
        if(CastingFinsihed() && spellReadyVFX== null)
        {
            spellReadyVFX = VFXManager.Instance.PlayFeedback(12, controller.GetGameObject().transform);
            spellReadyVFX.transform.parent = controller.VfxTransform;
        }
        if (!controller.HoldingSkill)
        {
            if(CastingFinsihed())
            {
                GameObject go = Instantiate(fireBoltProjectile, controller.VfxTransform.position, controller.GetGameObject().transform.rotation);
                //go.GetComponent<FireBoltProjectile>().SetOwner(controller.GetGameObject());
                go.SetActive(true);
                Destroy(go, lifeTime);
                controller.SkillColldowns[controller.SkillIndex] = Time.time;
                castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
                spellReadyVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
                Destroy(spellReadyVFX, 3);
                Destroy(castingVFX, 3);
                controller.SwitchState(PlayerState.Controlling);
            }
            else
            {
                controller.SkillColldowns[controller.SkillIndex] = Time.time - skillCooldown+1;
                castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
                Destroy(castingVFX, 3);
                controller.SwitchState(PlayerState.Controlling);
            }
            castingVFX.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            Destroy(castingVFX, 3);
            controller.SwitchState(PlayerState.Controlling);
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
