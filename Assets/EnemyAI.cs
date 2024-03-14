using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(StatusManager))]
public class EnemyAI : MonoBehaviour, IEntityControlls
{

    public LootTable lootTable;

    public EnemyControllState currentState;

    private EnemyState[] states = new EnemyState[10];

    // References
    public Transform attackPivot;
    [HideInInspector] public StatusManager sm;
    [HideInInspector] public Animator anim;
    [HideInInspector] public NavMeshAgent agent;

    [Header("Tracking")]
    public float detectionRadius = 30f;
    private float detectionInterval = 5f;
    private List<StatusManager> enemyList = new List<StatusManager>();
    [HideInInspector]public StatusManager target = null;

    [Header("Attack Parameter")]
    public float attackDashSpeed = 5;
    public float attackPrepareTime = 0.8f;
    public AnimationCurve attackCurve;
    public float attackDuration = 0.5f;
    public float attackCooldown = 2f;
    public float attackEndlag = 1.5f;

    [Header("Audio Parameters")]
    public float audioInterval = 5f;
    private float audioTimer = 0;

    [Header("UI")]
    public TextMeshProUGUI nameAndLevel;

    public GameObject attackHitbox;

    [HideInInspector]public float attackCooldownTimer = 0;

    private float detectionTimer = 0;

    public static List<EnemyAI> enemyAIList = new List<EnemyAI>();
    public Skill[] skills;
    public int skillIndex;

    private void Awake()
    {
        sm = GetComponent<StatusManager>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        states[(int)EnemyControllState.Idle] = new Idle();
        states[(int)EnemyControllState.Wander] = new Wander();
        states[(int)EnemyControllState.Attack] = new Attack();
        states[(int)EnemyControllState.Chase] = new Chase();
        states[(int)EnemyControllState.PrepareAttack] = new PrepareAttack();
        states[(int)EnemyControllState.AttackEndlag] = new AttackEndlag();
        states[(int)EnemyControllState.Death] = new Death();
        states[(int)EnemyControllState.UsingSkill] = new UsingSkill();
        states[(int)currentState].OnEnter(this);
        sm.OnDeath.AddListener(() => SwitchState(EnemyControllState.Death));
        enemyAIList.Add(this);
    }

    private void Start()
    {
        nameAndLevel.text = sm.name + " Lv." + sm.level;
    }

    public void Update()
    {
        states[(int)currentState].OnUpdate(this);
    }

    public void SwitchState(EnemyControllState newState)
    {
        CurrentState = newState;
    }



    public void SetDestinaton(Vector3 destination)
    {
        if (transform.position.y < -20) return;
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(destination, path);
        if (path.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(path);
        }
    }

    public void Animations()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }

    public void PlayAttackAnim()
    {
        anim.SetTrigger("Attack");
    }

    public void UpdateEnemyList()
    {
        if (Time.time >= detectionTimer)
        {
            detectionTimer = Time.time + detectionInterval;
            enemyList = StatusManager.GetEnemies(sm.faction);
        }
    }

    public void LookForEnemies()
    {
        foreach (StatusManager enemy in enemyList)
        {
            if (Vector3.Distance(transform.position, enemy.transform.position) < detectionRadius)
            {
                target = enemy;
                SwitchState(EnemyControllState.Chase);
                return;
            }
        }
        target = null;
    }

    public void DashAttack(float delta)
    {
        float attackCurve = this.attackCurve.Evaluate(delta / attackDashSpeed);
        Vector3 attackVector = transform.forward * attackDashSpeed * attackCurve;
        agent.Move(attackVector * Time.deltaTime);
    }

    public void HandleGeneralSound()
    {
        if (Time.time >= audioTimer)
        {
            audioTimer = Time.time + audioInterval + Random.Range(0,5f);
            AudioManager.PlaySound(transform.position, SoundType.Dog_Bark);
        }
    }

    public void SpawnAttackHitbox()
    {
        GameObject go = Instantiate(attackHitbox, attackPivot.position, attackPivot.rotation, transform);
        go.SetActive(true);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private EnemyControllState CurrentState
    {
        get => currentState;
        set
        {
            if (currentState != value)
            {
                states[(int)currentState].OnExit(this);
                states[(int)value].OnEnter(this);
                currentState = value;
            }
        }
    }

    public float[] SkillColldowns { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public int SkillIndex { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    public void DropLoot()
    {
        VFXManager.Instance.PlayFeedback(3, transform);
        lootTable.DropLoot(transform.position);
    }
    #region IEntityControlls
    public Animator GetAnimator()
    {
        return anim;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public Vector3 GetMovmentDirection()
    {
        return agent.velocity.normalized;
    }

    public void Movement(Vector3 movement)
    {
        Movement(movement);
    }

    public void ManualMovement()
    {
        // Not used for npcs
    }

    public void SwitchState(PlayerController.PlayerState controlling)
    {
        EnemyControllState state;
        switch(controlling)
        { 
            case PlayerController.PlayerState.Controlling:
                state = EnemyControllState.Idle;
                break;
            case PlayerController.PlayerState.PlayerUsingSkill:
                state = EnemyControllState.Attack;
                break;
            default:
                state = EnemyControllState.Idle;
                break;
        }
        SwitchState(state);
    }
    #endregion
}

public enum EnemyControllState
{
    Idle,
    Wander,
    Chase,
    Attack,
    PrepareAttack,
    AttackEndlag,
    Death,
    UsingSkill
}
public interface EnemyState
{
    void OnEnter(EnemyAI pc);
    void OnExit(EnemyAI pc);
    void OnUpdate(EnemyAI pc);
}
public class Idle : EnemyState
{
    private float enterTime = 0f;
    public void OnEnter(EnemyAI pc)
    {
        enterTime = Time.time + Random.Range(0,4f);
    }

    public void OnExit(EnemyAI pc)
    {

    }

    public void OnUpdate(EnemyAI pc)
    {
        pc.Animations();
        pc.UpdateEnemyList();
        pc.LookForEnemies();
        if(enterTime < Time.time)
        {
            pc.SwitchState(EnemyControllState.Wander);
        }
        pc.UpdateEnemyList();
    }
}

public class Wander: EnemyState
{
    private Vector3 destination;
    private float wanderRadius = 5f;
    private float enterTime = 0f;
    public void OnEnter(EnemyAI pc)
    {
        enterTime = Time.time;
        // Generate random point in wander radius
        int sampleCount = 0;
        NavMeshHit hit = new NavMeshHit();
        while (sampleCount < 5)
        {
            destination = pc.transform.position + Random.insideUnitSphere * wanderRadius;
            if (!NavMesh.SamplePosition(destination, out hit, wanderRadius, pc.agent.areaMask))
            {
                sampleCount++;
            }
            else 
            {
                break;
            }

        }

        destination = hit.position;
        pc.SetDestinaton(destination);

    }

    public void OnExit(EnemyAI pc)
    {

    }

    public void OnUpdate(EnemyAI pc)
    {
        pc.UpdateEnemyList();
        pc.LookForEnemies();
        pc.Animations();
        // If close to destination, switch to idle and then back to wander
        if (Vector3.Distance(pc.transform.position, destination) < 1f || enterTime + 5 < Time.time )
        {
            pc.SwitchState(EnemyControllState.Idle);
        }
    }
}

public class Attack : EnemyState
{
    private float enterTime = 0f;
    public void OnEnter(EnemyAI pc)
    {
        VFXManager.Instance.PlayFeedback(2, pc.attackPivot,pc.transform.rotation);
        enterTime = Time.time;
        //pc.Invoke("PlayAttackAnim", 0.43f);
        pc.agent.destination = pc.transform.position;
        pc.agent.isStopped = true;
        pc.agent.ResetPath();
        pc.SpawnAttackHitbox();
        pc.SwitchState(EnemyControllState.UsingSkill);
    }

    public void OnExit(EnemyAI pc)
    {

    }

    public void OnUpdate(EnemyAI pc)
    {
        return;
        pc.Animations();
        pc.DashAttack((Time.time-enterTime) / pc.attackDuration);
        if (enterTime +pc.attackDuration< Time.time)
        {
            pc.SwitchState(EnemyControllState.AttackEndlag);
        }
    }
}

public class UsingSkill : EnemyState
{
    public void OnEnter(EnemyAI pc)
    {
        pc.skills[pc.skillIndex].OnEnter(pc.gameObject);
    }

    public void OnExit(EnemyAI pc)
    {
        pc.skills[pc.skillIndex].OnExit(pc.gameObject);
    }

    public void OnUpdate(EnemyAI pc)
    {
        pc.skills[pc.skillIndex].OnUpdate(pc.gameObject);
    }
}

public class Chase : EnemyState
{
    public void OnEnter(EnemyAI pc)
    {
        pc.SetDestinaton(pc.target.transform.position);
    }

    public void OnExit(EnemyAI pc)
    {

    }

    public void OnUpdate(EnemyAI pc)
    {
        pc.HandleGeneralSound();
        pc.SetDestinaton(pc.target.transform.position);
        pc.Animations();
        if(Vector3.Distance(pc.transform.position, pc.target.transform.position) < 2.8f && pc.attackCooldownTimer < Time.time)
        {
            pc.SetDestinaton(pc.transform.position);
            pc.SwitchState(EnemyControllState.PrepareAttack);
        }
    }
    
}

public class PrepareAttack : EnemyState
{
    private float enterTime = 0f;
    public void OnEnter(EnemyAI pc)
    {
        pc.SetDestinaton(pc.transform.position);
        enterTime = Time.time + pc.attackPrepareTime + Random.Range(0,1f);
        pc.anim.SetBool("PrepareAttack", true);
    }

    public void OnExit(EnemyAI pc)
    {
        pc.anim.SetBool("PrepareAttack", false);
    }

    private Quaternion lastRotation;

    public void OnUpdate(EnemyAI pc)
    {
        // face the target
        Vector3 direction = pc.target.transform.position - pc.transform.position;
        direction.y = 0;
        pc.transform.rotation = Quaternion.RotateTowards(pc.transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 120 * Time.deltaTime);
        lastRotation = pc.transform.rotation;
        pc.Animations();
        if (enterTime < Time.time)
        {
            pc.SwitchState(EnemyControllState.Attack);
        }
    }
}

public class AttackEndlag : EnemyState
{
    private float enterTime = 0f;
    public void OnEnter(EnemyAI pc)
    {
        enterTime = Time.time + pc.attackEndlag;
    }

    public void OnExit(EnemyAI pc)
    {
        pc.attackCooldownTimer = Time.time + pc.attackCooldown;
    }

    public void OnUpdate(EnemyAI pc)
    {
        pc.Animations();
        if (enterTime < Time.time)
        {
            pc.SwitchState(EnemyControllState.Idle);
        }
    }
}

public class Death : EnemyState
{
    private float enterTime = 0f;
    public void OnEnter(EnemyAI pc)
    {
        enterTime = Time.time;
        pc.agent.isStopped = true;
        pc.agent.ResetPath();
        pc.anim.SetTrigger("Death");
        pc.agent.enabled = false;
        pc.GetComponent<Collider>().enabled = false;
        GameManager.Instance.player.GetComponent<PlayerExp>().AddExperience(pc.transform.position, pc.sm.experienceDrop);
        EnemyAI.enemyAIList.Remove(pc);
    }

    public void OnExit(EnemyAI pc)
    {

    }

    public void OnUpdate(EnemyAI pc)
    {
        if(enterTime + 2 < Time.time)
        {
            pc.DropLoot();
            GameObject.Destroy(pc.gameObject);
        }
    }
}

