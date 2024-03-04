using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SpawnDirector : MonoBehaviour
{
    private BoxCollider boxCollider;
    public List<ResourceBlock> blocks = new List<ResourceBlock>();
    public bool isDungeon = false;
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
    }

    private void OnDisable()
    {
        GameManager.OnGameTick -= () =>ResourceBlockManager.Instance.SpawnResoruces(blocks);
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

}
