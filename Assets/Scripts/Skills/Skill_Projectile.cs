using UnityEngine;
using static PlayerController;

[CreateAssetMenu(fileName = "Skill_Projectile", menuName = "ScriptableObjects/Skills/Skill_Projectile", order = 1)]
public class Skill_Projectile : Skill
{

    public int skillProjectileVFX = -1;
    public int skillCastingVFX = -1;
    public AnimationSkillCasting castingAnimationIndex ;
    public int spellIndicatorIndex = 1;
    public SpellIndicatorType spellIndicatorType = SpellIndicatorType.FromSource;
    public GameObject projectileHitbox;

    private IEntityControlls controller;
    private GameObject castingVFX;
    private GameObject spellReadyVFX;
    private bool castAnimation = false;

    public override void OnEnter(GameObject source)
    {
        castAnimation = false;
        StartProjectileCast(source.GetComponent<IEntityControlls>());
    }

    public override void OnUpdate(GameObject source)
    {
        controller.CastRotation();
        if(Time.time - onEnterTime > castTime - 0.1f && !castAnimation)
        {
            controller.GetAnimator().SetInteger("SkillCasting", (int)castingAnimationIndex + 100);
            castAnimation = true;
        }
        if (CastingFinsihed())
        {
            SpellIndicators.DismissSpellIndicator();
            Destroy(castingVFX);
            GameObject projectile = Instantiate(projectileHitbox, controller.VfxTransform.position, controller.GetGameObject().transform.rotation);
            GameObject projectileVFX = VFXManager.Instance.PlayFeedback(skillProjectileVFX, projectile.transform);
            projectileVFX.transform.parent = projectile.transform;
            projectileVFX.transform.localPosition = Vector3.zero;
            projectileVFX.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            DamageObject damageObject = projectile.GetComponent<DamageObject>();
            if (damageObject != null)
            {
                damageObject.source = controller.StatusManager;
            }
            controller.SwitchState(PlayerState.Controlling);
        }
    }

    public void StartProjectileCast(IEntityControlls entityControlls)
    {
        controller = entityControlls;
        onEnterTime = Time.time;
        SpellIndicators.CallSpellIndicator(spellIndicatorType, spellIndicatorIndex, controller);
        castingVFX = VFXManager.Instance.PlayFeedback(skillCastingVFX,entityControlls.VfxTransform);
        controller.GetAnimator().SetInteger("SkillCasting", (int)castingAnimationIndex);
    }

}