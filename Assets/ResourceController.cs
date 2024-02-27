using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceController : MonoBehaviour
{
    public GameObject[] visuals;
    public ItemBlueprint itemDrop;
    public ShaderController shaderController;

    public void SetVisual(int index)
    {
        foreach (var visual in visuals)
        {
            visual.SetActive(false);
        }
        visuals[index].SetActive(true);
        visuals[index].transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        shaderController.TargetRenderer = visuals[index].GetComponentInChildren<Renderer>();
         shaderController.TargetMaterial = visuals[index].GetComponentInChildren<Renderer>().material;
        shaderController.enabled = true;

    }
    public void DropItem()
    {
        GameObject go = Instantiate(GameManager.Instance.droppedItem, transform.position + new Vector3(0,0.3f,0), Quaternion.identity);
        go.GetComponent<DroppedItem>().SetUpDroppedItem(itemDrop, 1);
    }

    private void Start()
    {
        GetComponent<ResourceStatusManager>().OnDeath.AddListener(AddExperience);
    }

    public void AddExperience()
    {
        GameManager.Instance.player.GetComponent<PlayerExp>().AddExperience(transform.position,1);
    }

}
