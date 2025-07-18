using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class VFX_Adjuster_TEMP : MonoBehaviour
{
    public MadraVFX_Info vfxInfo;
    private ParticleSystem[] particleSystems;
    public UnityEvent OnOverGrass;
    public UnityEvent OnOverSnow;
    // Start is called before the first frame update
    void Start()
    {
        CheckOverMaterial();
        if (vfxInfo == null) return;
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            if(ps.tag.Equals("Ignore")) 
                continue;

            ParticleSystem main = ps;
            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
            ParticleSystem.CustomDataModule customData = ps.customData;
            // Renderer
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (vfxInfo.materialOverrideMain != null)
            {
                renderer.material = vfxInfo.materialOverrideMain;
            }
            Color col = ps.tag.Equals("Primary") ? vfxInfo.customMainColorOverride : vfxInfo.customSecondaryColorOverride;
            Color col2 = ps.tag.Equals("Primary") ? vfxInfo.customSecondaryColorOverride : vfxInfo.customMainColorOverride;
            if (vfxInfo.customMainColorOverride != Color.black)
            {
                customData.SetColor(ParticleSystemCustomData.Custom2, col2);
            }
            colorOverLifetime.color = col;
            main.startColor = col;
        }
    }

    private void CheckOverMaterial()
    {
        //Raycast down
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 3, GameManager.Instance.groundLayer))
        {
            if (hit.collider != null && hit.collider.GetComponent<Renderer>())
            {
                Material mat = hit.collider.GetComponent<Renderer>().material;
                if (mat.name.Contains("Grass"))
                {
                    OnOverGrass.Invoke();
                }
                else if (mat.name.Contains("Snow"))
                {
                    OnOverSnow.Invoke();
                }
                else
                {
                }
            }
        }
    }
}
