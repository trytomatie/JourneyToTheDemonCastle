using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SpellIndicators : MonoBehaviour
{
    public GameObject[] spellIndicators;
    public DecalProjector[] castTimeIndicators;
    public int currentIndicatorIndex = 1;
    public SpellIndicatorType currentIndicatorType = SpellIndicatorType.None;
    

    public static SpellIndicators Instance;
    private IEntityControlls spellCaster;
    public LayerMask groundLayer;
    private float castTime = 0;
    private float currentCastTime = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Update()
    {
        if (currentIndicatorIndex != -1)
        {
            switch (currentIndicatorType)
            {
                case SpellIndicatorType.None:
                    break;
                case SpellIndicatorType.CursorPlaced:
                    Vector3 position = GameManager.GetPointerPosition();
                    position.y = 30;
                    spellIndicators[currentIndicatorIndex].transform.position = position;
                    if(currentCastTime > 0)
                    {
                        float size = Mathf.Lerp(1,0, currentCastTime / castTime);
                        castTimeIndicators[currentIndicatorIndex].size = new Vector3(size, size, 34);
                        currentCastTime -= Time.deltaTime;
                    }
                    break;
                case SpellIndicatorType.FromSource:
                    Vector3 pos1 = spellCaster.GetGameObject().transform.position;
                    pos1.y = 30;
                    spellIndicators[currentIndicatorIndex].transform.position = pos1;
                    spellIndicators[currentIndicatorIndex].transform.rotation = spellCaster.GetGameObject().transform.rotation;
                    break;
            }
        }
    }



    public static void CallSpellIndicator(SpellIndicatorType type,int indicatorIndex, IEntityControlls source)
    {
        Instance.spellCaster = source;
        Instance.currentIndicatorType = type;
        Instance.currentIndicatorIndex = indicatorIndex;
    }

    public static void CallSpellIndicator(SpellIndicatorType type, int indicatorIndex, IEntityControlls source,float castTime)
    {
        Instance.castTime = castTime;
        Instance.currentCastTime = castTime;
        CallSpellIndicator(type, indicatorIndex, source);
    }

    public static void DismissSpellIndicator()
    {
        Instance.spellIndicators[Instance.currentIndicatorIndex].transform.position = new Vector3(0,1000,0);
        Instance.currentIndicatorIndex = -1;
    }
}

public enum SpellIndicatorType
{
    None,
    CursorPlaced,
    FromSource,
}
