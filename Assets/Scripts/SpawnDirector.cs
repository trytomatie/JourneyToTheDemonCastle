using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class SpawnDirector : MonoBehaviour
{
    static public SpawnDirector instance;
    public DirectorShop directorShop;
    private int spawnCredits = 0;

    private void OnEnable()
    {
        GameManager.OnGameTick += CheckDirectorShop;
    }

    private void OnDisable()
    {
        GameManager.OnGameTick -= CheckDirectorShop;
    }

    private void CheckDirectorShop()
    {
        if (SpawnPoint.spawnPoints.Count == 0)
        {
            return;
        }
        // Get the spawn director the player is in
        if (GameManager.Instance.gameTicks % 1 == 0)
        {
            spawnCredits++;
        }
        if (GameManager.Instance.gameTicks % 20 != 0)
        {
            return;
        }
        int shopIndex = 0;
        directorShop.CheckForRestock();
        while (shopIndex != -1)
        {
            shopIndex = directorShop.GetRandomAffordableShopIndex(spawnCredits);
            if (shopIndex != -1)
            {
                Transform spawnpoint = SpawnPoint.spawnPoints[Random.Range(0, SpawnPoint.spawnPoints.Count)];
                ResoruceBlockData blockData = directorShop.shopItems[shopIndex].resourceBlockData;
                Instantiate(blockData.resourceBlockPrefab, spawnpoint.position, spawnpoint.rotation);
                directorShop.shopItems[shopIndex].amount--;
                spawnCredits -= directorShop.shopItems[shopIndex].cost;

            }
        }
    }
}
