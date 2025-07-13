using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class StatusManager : MonoBehaviour
{
    public enum Faction
    {
        Player,
        Demon,
        Neutral
    }

    public Faction faction = Faction.Neutral;
    public HitType materialType = HitType.Entity;
    public SoundType deathSound;
    public int level = 1;
    public int maxHp = 30;
    private int hp = 30;
    [SerializeField] private float maxMadraCore = 100;
    [SerializeField] private float madraCore = 100;
    [SerializeField] private float maxMadra = 0;
    [SerializeField] private float madra = 0;
    [SerializeField] private float madraRegenPerSecond = 5;
    [SerializeField] private int baseAttackDamage = 1;
    public int bonusDefense = 0;

    public int experienceDrop = 1;

    public StatsScaling statsScaling;

    public int bonusAttackDamage = 0;
    public float bonusAttackDamageMultiplier = 1;


    public int weaponAttackDamage = 0;

    public static Dictionary<Faction, List<StatusManager>> factionMembers = new Dictionary<Faction, List<StatusManager>>();

    public UnityEvent OnDeath;
    public UnityEvent OnDamage;
    public UnityEvent OnHit;
    public UnityEvent<Vector4> OnMadraRegen;

    public int AttackDamage { get => Mathf.CeilToInt((baseAttackDamage + weaponAttackDamage + bonusAttackDamage) * bonusAttackDamageMultiplier); }
    public int Defense { get => bonusDefense; }

    // Start is called before the first frame update
    public virtual void Start()
    {
        if(statsScaling != null)
        {
            maxHp += statsScaling.hpGrowth * level-1;
            baseAttackDamage += statsScaling.attackGrowth * level-1;
            experienceDrop += statsScaling.expGrowth * level-1;

        }
        Hp = maxHp;
        if(maxMadra > 0) 
        { 
            StartCoroutine(RegenStamina());
        }
        // Stop all coroutines on death
        OnDeath.AddListener(() => StopAllCoroutines());
        OnDeath.AddListener(() => AddToFactionDictonary());
        OnDeath.AddListener(() => AudioManager.PlaySound(transform.position, deathSound));
        AssignHitsound();
    }

    public void AssignHitsound()
    {
        switch(materialType)
        {
            case HitType.Wood:
                OnHit.AddListener(() => AudioManager.PlayHitSound(transform.position, HitType.Wood));
                break;
            case HitType.Stone:
                OnHit.AddListener(() => AudioManager.PlayHitSound(transform.position, HitType.Stone));
                break;
            default:
                OnHit.AddListener(() => AudioManager.PlayHitSound(transform.position, HitType.Entity));
                break;
        }
    }

    private void OnEnable()
    {
        AddToFactionDictonary();
    }
    private void OnDisable()
    {
        factionMembers[faction].Remove(this);
    }

    private void AddToFactionDictonary()
    {
        if (!factionMembers.ContainsKey(faction))
        {
            factionMembers.Add(faction, new List<StatusManager>());
        }
        factionMembers[faction].Add(this);
    }

    private IEnumerator RegenStamina()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (Madra < maxMadra)
            {
                if(madraCore > 0)
                {
                    float regen = madraRegenPerSecond * Time.fixedDeltaTime;
                    Madra += regen;
                    Madra = Mathf.Clamp(Madra, 0, maxMadra);
                    madraCore -= regen;
                }


            }
        }
    }

    public void ApplyDamage(int damage)
    {
        int calculatedDamage = Mathf.Clamp(damage - bonusDefense,1, 9999);
        Hp -= calculatedDamage;
        FloatingTextSpawner.instance.SpawnFloatingText(calculatedDamage.ToString(), transform);
        if (Hp <= 0)
        {
            OnDeath.Invoke();
        }
    }

    public void UpdateHPBar(MMProgressBar progressbar)
    {
        progressbar.UpdateBar01(Hp / (float)maxHp);
    }

    public static List<StatusManager> GetEnemies(Faction faction)
    {
        // Hard coded for now
        switch(faction)
        {
            case Faction.Player:
                return factionMembers[Faction.Demon];
            case Faction.Demon:
                return factionMembers[Faction.Player];
            default:
                return new List<StatusManager>();
        }
    }

    public int Hp 
    { 
        get => hp;
        set
        {
            OnDamage.Invoke();
            hp = value;
        }
    }

    public float Madra { get => madra; 
        set
        {
            madra = value;
            OnMadraRegen.Invoke(new Vector4(madra, maxMadra, madraCore, maxMadraCore));

        }
    }
}
