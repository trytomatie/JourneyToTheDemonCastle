using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalentSlotUI : MonoBehaviour
{
    public Image talentIcon;
    public Talent talentReference;

    public void SetTalent(Talent talent)
    {
        talentReference = talent;
        talentIcon.sprite = talent.talentIcon;
    }
}
