using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int gameTicks;
    public float gameTickRate = 1;
    public delegate void GameTick();
    public static event GameTick OnGameTick;
    public GameObject player;

    [Header("Prefabs")]
    public GameObject droppedItem;
    public GameObject[] grassPrefabs;
    public GameObject resourceBlock;

    [Header("Grid")]
    public Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();
    public LayerMask groundLayer;

    [Header("GameWorld")]
    public GameObject overWorld;
    public GameObject dungeonWorld;


    [Header("Modifiers")]
    public float generalItemProductionMultiplier = 1;
    public bool globalStashUnlocked = false;
    public int lootDropAmount = 1;
    // Singleton
    private static GameManager instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        Instance.player = GameObject.FindObjectOfType<PlayerController>().gameObject;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GameClock());
        CheckGridInARadius(Vector3.zero, 360);
    }

    private IEnumerator GameClock()
    {
        while (true)
        {
            yield return new WaitForSeconds(gameTickRate);
            try
            {
                if(OnGameTick != null)
                {
                    OnGameTick.Invoke();
                }

            }
            catch(Exception e)
            {
                Debug.LogException(e, this);
            }
            gameTicks++;
        }
    }

    public void CheckGridInARadius(Vector3 pos, float radius)
    {
        for (int x = Mathf.RoundToInt(pos.x - radius); x < Mathf.RoundToInt(pos.x + radius); x++)
        {
            for (int z = Mathf.RoundToInt(pos.z - radius); z < Mathf.RoundToInt(pos.z + radius); z++)
            {
                CheckGridAt(new Vector3(x, 0, z));
            }
        }
    }

    public void CheckGridAt(Vector3 pos)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
        bool assigned = false;
        if(grid.ContainsKey(gridPos))
        {
            assigned = true;
        }
        else
        {
            grid.Add(gridPos, null);
        }
        pos.y = 10;
        Vector3 posRounded = new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));
        RaycastHit hit;
        if (Physics.Raycast(pos + new Vector3(0.5f, 0, -0.5f), Vector3.down, out hit, 100, groundLayer))
        {

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                print(assigned);
                if (!assigned)
                {
                    grid[gridPos] = Instantiate(resourceBlock, hit.point, Quaternion.identity);
                    grid[gridPos].transform.parent = hit.collider.transform.root;
                }
            }
            else
            {
                if(assigned)
                {
                    Destroy(grid[gridPos]);
                    grid.Remove(gridPos);
                }
            }
        }
    }

    public void SpawnItem(ItemBlueprint item,int amount, Vector3 pos)
    {
        GameObject itemObj = Instantiate(droppedItem, pos, Quaternion.identity);
        itemObj.GetComponent<DroppedItem>().SetUpDroppedItem(item, amount);
    }


    public static GameManager Instance { get 
        { 
            return instance; 
        } 
        set => instance = value; }
}
