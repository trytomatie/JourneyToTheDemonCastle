using System.Collections;
using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SpawnDirector : MonoBehaviour
{
    private BoxCollider boxCollider;
    public List<ResourceBlock> blocks = new List<ResourceBlock>();
    public bool isDungeon = false;

    [Header("Enemies")]
    public List<ResoruceBlockData> spawnableEnemies;
    public int maxEnemies = 7;
    public int spawnedEnemies = 0;


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
    }

    private IEnumerator RegisterBlocksCoorutine()
    {
        while(awokenGameTick + 5 > GameManager.Instance.gameTicks)
        {
            yield return null;
        }
        RegisterBlocks();
        GameManager.OnGameTick += () => ResourceBlockManager.Instance.SpawnResoruces(blocks);
        GameManager.OnGameTick += () => SpawnEnemy();
    }

    private void OnDisable()
    {
        GameManager.OnGameTick -= () =>ResourceBlockManager.Instance.SpawnResoruces(blocks);
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
        if(GameManager.Instance.gameTicks % 30 == 0)
        {
            int enemiesToSpawn = Random.Range(0,maxEnemies - spawnedEnemies+1);
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

}
