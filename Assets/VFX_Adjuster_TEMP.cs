using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_Adjuster_TEMP : MonoBehaviour
{
    public MadraVFX_Info vfxInfo;
    private ParticleSystem[] particleSystems;
    // Start is called before the first frame update
    void Start()
    {
        if (vfxInfo == null) return;
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            ParticleSystem main = ps;
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            ParticleSystem.CustomDataModule customData = ps.customData;
            // Renderer
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (vfxInfo.materialOverrideMain != null)
            {
                renderer.material = vfxInfo.materialOverrideMain;
            }
            if (vfxInfo.customColoerOverride != Color.black)
            {
                customData.SetColor(ParticleSystemCustomData.Custom2, vfxInfo.customColoerOverride);
            }
            colorOverLifetime.color = vfxInfo.colorOverLifetimeOverride;
        }
    }
}
