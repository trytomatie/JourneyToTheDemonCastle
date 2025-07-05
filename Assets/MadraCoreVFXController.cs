using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MadraCoreVFXController : MonoBehaviour
{
    public float coreFillPercentage = 1;
    public float cyclingSpeed = 0.1f;
    public ParticleSystem madraParticleSystem;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateParticleSystem();
    }

    public void UpdateParticleSystem()
    {
        // Rotation over time 
        if (madraParticleSystem != null)
        {
            ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = madraParticleSystem.rotationOverLifetime;
            ParticleSystem.CustomDataModule customDataModule = madraParticleSystem.customData;
            rotationOverLifetime.zMultiplier = Mathf.Lerp(0, 10, cyclingSpeed);
            customDataModule.SetVector(ParticleSystemCustomData.Custom1,2, new ParticleSystem.MinMaxCurve(Mathf.Lerp(0.5f,0,coreFillPercentage)));
            ParticleSystem.MainModule mainModule = madraParticleSystem.main;
            mainModule.startSizeMultiplier = Mathf.Lerp(0.01f, 1, coreFillPercentage);
        }

    }
}
