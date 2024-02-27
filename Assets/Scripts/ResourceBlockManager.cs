using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceBlockManager : MonoBehaviour
{
    private List<ResourceBlock> resourceBlocksInScene = new List<ResourceBlock>();
    public List<ResoruceBlockData> resourceBlockData;

    private int gameTickInterval = 10;

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
        GameManager.OnGameTick += SpawnResoruces;
    }

    private void OnDisable()
    {
        GameManager.OnGameTick -= SpawnResoruces;
    }

    public void SpawnResoruces()
    {
        if(GameManager.Instance.gameTicks % gameTickInterval != 0)
        {
            return;
        }
        // Pick a random ResourceBlock in the scene
        ResourceBlock resourceBlock = resourceBlocksInScene[Random.Range(0, resourceBlocksInScene.Count)];
        int maxWeight = 0;
 
        if(resourceBlock.spawnedResoruce != null)
        {
            return;
        }

        // Get the total weight of all the resource blocks
        foreach (ResoruceBlockData data in resourceBlockData)
        {
            maxWeight += data.spawnWeight;
        }

        int randomWeight = Random.Range(0, maxWeight+resourceBlock.spawnResistance);

        if(randomWeight < maxWeight) 
        {
            // Spawn a resource
            foreach (ResoruceBlockData data in resourceBlockData)
            {
                randomWeight -= data.spawnWeight;
                if(randomWeight <= 0)
                {
                    resourceBlock.spawnedResoruce = Instantiate(data.resourceBlockPrefab, resourceBlock.transform.position, Quaternion.identity);
                    resourceBlock.spawnedResoruce.GetComponent<ResourceController>().SetVisual(Random.Range(0, resourceBlockData.Count));
                    break;
                }
            }
        }
        else
        {
            // Do Nothing I guess
        }
    }
}
