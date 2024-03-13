using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    public Image skillIcon;
    public Image skillCooldown;
    public Image skillCooldownRadial;
    public TextMeshProUGUI skillCooldownText;
    public Skill skill;

    public void SetupSkill(Skill skill)
    {
        if(skill == null)
        {
            skillIcon.sprite = null;
            skillCooldown.gameObject.SetActive(false);
            skillCooldownRadial.fillAmount = 0;
            skillCooldownText.gameObject.SetActive(false);
            this.skill = null;
            return;
        }
        skillIcon.sprite = skill.skillIcon;
        skillCooldown.gameObject.SetActive(false);
        skillCooldownRadial.fillAmount = 0;
        skillCooldownText.gameObject.SetActive(false);
        this.skill = skill;
    }

    private void Update()
    {
        CheckCooldown();
    }

    public void CheckCooldown()
    {
        if (skill == null) return;
        if(skill.GetCooldown()>0)
        {
            skillCooldownRadial.gameObject.SetActive(true);
            skillCooldown.gameObject.SetActive(true);
            skillCooldownRadial.fillAmount = (skill.GetCooldown() / skill.skillCooldown);
            skillCooldownText.gameObject.SetActive(true);
            skillCooldownText.text = skill.GetCooldown().ToString("F1");
        }
        else
        {
            skillCooldown.gameObject.SetActive(false);
            skillCooldownRadial.gameObject.SetActive(false);
            skillCooldownText.gameObject.SetActive(false);
        }
    }
}
