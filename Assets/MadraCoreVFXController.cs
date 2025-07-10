using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MadraCoreVFXController : MonoBehaviour
{
    public float coreFillPercentage = 1;
    public float madraFillPercentage = 1;
    public float cyclingSpeed = 0.1f;
    public ParticleSystem madraParticleSystem;
    public Image madraFillUI;
    public TextMeshProUGUI coreFillPercentageText;
    // Start is called before the first frame update

    public static MadraCoreVFXController Instance;
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR // Only update in editor for testing purposes
        UpdateParticleSystem();
#endif
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
            coreFillPercentageText.text = Mathf.RoundToInt(coreFillPercentage * 100) + "%";
        }
        madraFillUI.fillAmount = madraFillPercentage;

    }
}
