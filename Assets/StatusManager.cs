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
    [SerializeField] private int maxHp = 30;
    [SerializeField] private int hp = 30;
    [SerializeField] private int maxStamina = 0;
    [SerializeField] private int stamina = 0;
    [SerializeField] private int staminaRegenPerSecond = 5;
    [SerializeField] private int baseAttackDamage = 1;


    public int weaponAttackDamage = 0;

    public static Dictionary<Faction, List<StatusManager>> factionMembers = new Dictionary<Faction, List<StatusManager>>();

    public UnityEvent OnDeath;
    public UnityEvent OnDamage;

    public int AttackDamage { get => baseAttackDamage + weaponAttackDamage;}

    // Start is called before the first frame update
    public virtual void Start()
    {
        if(maxStamina > 0) 
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
                OnDamage.AddListener(() => AudioManager.PlayHitSound(transform.position, HitType.Wood));
                break;
            case HitType.Stone:
                OnDamage.AddListener(() => AudioManager.PlayHitSound(transform.position, HitType.Stone));
                break;
            default:
                OnDamage.AddListener(() => AudioManager.PlayHitSound(transform.position, HitType.Entity));
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
        float regenFloat = 0;
        while (true)
        {
            yield return new WaitForFixedUpdate();

            if (stamina < maxStamina)
            {
                regenFloat += staminaRegenPerSecond * Time.fixedDeltaTime;
                if (regenFloat >= 1)
                {
                    regenFloat -= 1;
                    stamina++;
                }
            }
        }
    }

    public void ApplyDamage(int damage)
    {
        hp -= damage;
        FloatingTextSpawner.instance.SpawnFloatingText(damage.ToString(), transform);
        OnDamage.Invoke();
        if (hp <= 0)
        {
            OnDeath.Invoke();
        }
    }

    public void UpdateHPBar(MMProgressBar progressbar)
    {
        progressbar.UpdateBar01(hp / (float)maxHp);
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
}
