using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[CreateAssetMenu(fileName = "MadraVFX_Info", menuName = "ScriptableObjects/MadraVFX_Info", order = 1)]
public class MadraVFX_Info : ScriptableObject
{
    public Material materialOverrideMain;
    [ColorUsage(true, true)]
    public Color customColoerOverride;
    public MinMaxGradient colorOverLifetimeOverride;
}
