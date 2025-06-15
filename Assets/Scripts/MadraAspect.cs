using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "MadraAspect", menuName = "ScriptableObjects/MadraAspect", order = 1)]
public class MadraAspect : ScriptableObject
{
    public string mardraType;
    public string[] majorPerks;
    public string[] minorPerks;

    private void OnValidate()
    {
        mardraType = this.name;
    }
}
