using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class LifeLineHud : MonoBehaviour
{
    public ParticleSystem mainParticleSystem;
    public ParticleSystem[] secondarySystems;
    public ParticleSystem hurtParticleSystem;
    [Range(0, 1)]
    public float hpPercentage = 1;
    public TextMeshProUGUI hpText;
    public AnimationCurve hpCurve;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
# if UNITY_EDITOR // Only update in editor for testing purposes
        UpdateParticleSystem();
#endif
    }

    public void UpdateParticleSystem()
    {
        // Rotation over time 
        if (mainParticleSystem != null)
        {
            ParticleSystem.SizeOverLifetimeModule sizeOverLifetimeModule = mainParticleSystem.sizeOverLifetime;
            ParticleSystem.MainModule mainModule = mainParticleSystem.main;
            Keyframe key = hpCurve.keys[1];
            key.time = Mathf.Clamp(hpPercentage,0.01f, 0.99f);
            hpCurve.MoveKey(1, key);
            sizeOverLifetimeModule.size = new ParticleSystem.MinMaxCurve(1, hpCurve);
            //hpText.text = Mathf.RoundToInt(coreFillPercentage * 100) + "%";
        }

        foreach(ParticleSystem system in secondarySystems)
        {
            ParticleSystem.MainModule mainModule = system.main;
            mainModule.startLifetime = Mathf.Lerp(0.01f, 5f, hpPercentage);
        }
    }

    public void DamageVFX()
    {
        StartCoroutine(DamageCoorutine());
    }

    private IEnumerator DamageCoorutine()
    {
        // Particlesystem Renderer
        ParticleSystemRenderer renderer = mainParticleSystem.GetComponent<ParticleSystemRenderer>();
        hurtParticleSystem.Play();
        renderer.trailMaterial.SetColor("_BaseColor", Color.white);
        yield return new WaitForSeconds(0.1f);
        renderer.trailMaterial.SetColor("_BaseColor", Color.red);
    }
}
