using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SpawnDirector : MonoBehaviour
{
    [HideInInspector]public BoxCollider boxCollider;
    [Header("Resources")]
    public List<ResourceBlock> blocks = new List<ResourceBlock>();
    public int spawnedResources = 0;
    public int resourceLimit = 10;

    [Header("Enemies")]
    public List<ResoruceBlockData> spawnableEnemies;
    public int maxEnemies = 7;
    public int spawnedEnemies = 0;
    public bool isDungeon = false;
    public int mobSpawnLevel = 1;


    private int awokenGameTick = 0;
    public Vector3 Dimensions
    {
        get
        {
            return boxCollider.size;
        }
    }
    // Use this for initialization
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        awokenGameTick = GameManager.Instance.gameTicks;
        StartCoroutine(RegisterBlocksCoorutine());
        ResourceBlockManager.Instance.spawnDirectors.Add(this);


    }

    private IEnumerator RegisterBlocksCoorutine()
    {
        while(awokenGameTick + 5 > GameManager.Instance.gameTicks)
        {
            yield return null;
        }
        RegisterBlocks();
        while (spawnedResources < resourceLimit)
        {
            ResourceBlock resourceBlock = blocks[Random.Range(0, blocks.Count)];
            SpawnResource(resourceBlock, ResourceBlockManager.Instance.resourceBlockData[Random.Range(0, ResourceBlockManager.Instance.resourceBlockData.Count)]);
        }
        GameManager.OnGameTick += () => SpawnResoruces(blocks);
        GameManager.OnGameTick += () => SpawnEnemy();
    }

    private void OnDisable()
    {
        GameManager.OnGameTick -= () => SpawnResoruces(blocks);
        GameManager.OnGameTick -= () => SpawnEnemy();
    }

    public void RegisterBlocks()
    {
        Vector3 position = transform.position;
        foreach(ResourceBlock block in ResourceBlockManager.Instance.resourceBlocksInScene)
        {
            if (block.transform.position.x > position.x - boxCollider.size.x / 2 && block.transform.position.x < position.x + boxCollider.size.x / 2 &&
                               block.transform.position.z > position.z - boxCollider.size.z / 2 && block.transform.position.z < position.z + boxCollider.size.z / 2)
            {
                blocks.Add(block);
            }
        }
    }

    public void SpawnEnemy()
    {
        if(GameManager.Instance.gameTicks % 300 == 0)
        {
            int enemiesToSpawn = 1;
            if(isDungeon)
            {
                enemiesToSpawn = Random.Range(0, maxEnemies - spawnedEnemies + 1);
            }
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                if (spawnedEnemies >= maxEnemies)
                {
                    return;
                }

                ResourceBlock resourceBlock = blocks[Random.Range(0, blocks.Count)];
                int maxWeight = 0;
                // Get the total weight of all the resource blocks
                foreach (ResoruceBlockData data in spawnableEnemies)
                {
                    maxWeight += data.spawnWeight;
                }

                int randomWeight = Random.Range(0, maxWeight + resourceBlock.spawnResistance);

                if (randomWeight < maxWeight)
                {
                    // Spawn an enemy
                    foreach (ResoruceBlockData data in spawnableEnemies)
                    {
                        randomWeight -= data.spawnWeight;
                        if (randomWeight <= 0)
                        {
                            resourceBlock.spawnedResoruce = Instantiate(data.resourceBlockPrefab, resourceBlock.transform.position, Quaternion.identity);
                            resourceBlock.spawnedResoruce.transform.parent = resourceBlock.transform;
                            resourceBlock.spawnedResoruce.GetComponent<StatusManager>().OnDeath.AddListener(() => spawnedEnemies--);
                            resourceBlock.spawnedResoruce.GetComponent<StatusManager>().level = mobSpawnLevel;
                            spawnedEnemies++;
                            // resourceBlock.spawnedResoruce.GetComponent<ResourceController>().SetVisual(Random.Range(0, resourceBlockData.Count));
                            break;
                        }
                    }
                }

            }
            return;
        }
    }

    private int spawnIntervall = 10;
    public void SpawnResoruces(List<ResourceBlock> blocks)
    {
        if(spawnedResources >= resourceLimit)
        {
            return;
        }
        if (GameManager.Instance.gameTicks % spawnIntervall != 0)
        {
            return;
        }
        int blockPercantage = Random.Range(1,3);
        print("Spawning " + blockPercantage + " resources");
        for (int i = 0; i < blockPercantage; i++)
        {
            // Pick a random ResourceBlock in the scene
            ResourceBlock resourceBlock = blocks[Random.Range(0, blocks.Count)];
            int maxWeight = 0;

            if (resourceBlock.spawnedResoruce != null)
            {
                return;
            }

            // Get the total weight of all the resource blocks
            foreach (ResoruceBlockData data in ResourceBlockManager.Instance.resourceBlockData)
            {
                maxWeight += data.spawnWeight;
            }

            int randomWeight = Random.Range(0, maxWeight + resourceBlock.spawnResistance + (50*(spawnedResources/blocks.Count)));

            if (randomWeight < maxWeight)
            {
                // Spawn a resource
                foreach (ResoruceBlockData data in ResourceBlockManager.Instance.resourceBlockData)
                {
                    randomWeight -= data.spawnWeight;
                    if (randomWeight <= 0)
                    {
                        SpawnResource(resourceBlock, data);
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

    public void SpawnResource(ResourceBlock resourceBlock, ResoruceBlockData data)
    {
        spawnedResources++;
        resourceBlock.spawnedResoruce = Instantiate(data.resourceBlockPrefab, resourceBlock.transform.position, Quaternion.identity);
        resourceBlock.spawnedResoruce.transform.parent = resourceBlock.transform;
        resourceBlock.spawnedResoruce.GetComponent<ResourceController>().SetVisual();
        resourceBlock.spawnedResoruce.GetComponent<StatusManager>().OnDeath.AddListener(() => spawnedResources--);
    }
}
