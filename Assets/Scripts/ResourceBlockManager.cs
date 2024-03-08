using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBlockManager : MonoBehaviour
{
    public List<ResourceBlock> resourceBlocksInScene = new List<ResourceBlock>();
    public List<ResoruceBlockData> resourceBlockData;

    private int spawnIntervall = 10;

    // Singleton
    public static ResourceBlockManager Instance { get; private set; }
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    public void AddResourceBlockToSceneList(ResourceBlock resourceBlock)
    {
        resourceBlocksInScene.Add(resourceBlock);
    }

    private void OnEnable()
    {
        //GameManager.OnGameTick += SpawnResoruces;
    }

    private void OnDisable()
    {
        //GameManager.OnGameTick -= SpawnResoruces;
    }

   
}
