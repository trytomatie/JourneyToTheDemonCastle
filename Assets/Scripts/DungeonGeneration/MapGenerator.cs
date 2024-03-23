
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MapGenerator : MonoBehaviour
{
    private const int BASE_CHESTCOST = 12;
    public Transform mapParent;
    public List<GameObject> chunkTiles = new List<GameObject>();
    public ScriptableObject_MapSpawnCard mapSpawnCard;


    public System.Random seed = new System.Random();
    private System.Random xSeed;
    private System.Random ySeed;
    private System.Random negativeXSeed;
    private System.Random negativeYSeed;

    public bool debug_GenerateWholeMap = false;
    private string debugMessage = "";


    private List<int> xSeedMap = new List<int>();
    private List<int> ySeedMap = new List<int>();
    private List<int> negativeXSeedMap = new List<int>();
    private List<int> negativeYSeedMap = new List<int>();

    public Dictionary<Vector2, GameObject> chunkMap = new Dictionary<Vector2, GameObject>(); // Chunks that are generated
    public Dictionary<Vector2, GameObject> exploredChunks = new Dictionary<Vector2, GameObject>(); // The Chunks the player Has visited physicaly
    public Dictionary<Vector2, int> tunnelMap = new Dictionary<Vector2, int>(); // Map of the tunnels where chunks are supposed to be able to spawn
    public Vector2 currentCameraCoords = new Vector2();
    private float CHUNKSIZE_X = 16f;
    private float CHUNKSIZE_Y = 16f;
    public int currentFloor = 0;


    public Vector2 CurrentCameraCoords
    {
        get
        {
            return currentCameraCoords;
        }

        set
        {
            if(value != currentCameraCoords)
            {
                currentCameraCoords = value;
                StartCoroutine(LoadingAndUnloading(currentCameraCoords, value));
            }
        }
    }

    public Dictionary<Vector2, GameObject> ExploredChunks
    {
        get
        {
            return exploredChunks;
        }

        set
        {
            exploredChunks = value;
        }
    }

    IEnumerator LoadingAndUnloading(Vector2 toUnload,Vector2 toLoad)
    {
        if(!ExploredChunks.ContainsKey(toUnload))
        {
            if(chunkMap.ContainsKey(toUnload))
            { 
                ExploredChunks.Add(toUnload, chunkMap[toUnload]);
            }
            //GameObject.Find("RoomText").GetComponent<Text>().text = "Rooms Explored: " + ExploredChunks.Count;
        }
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -2; y <= 2; y++)
            {
                yield return new WaitForEndOfFrame();
                if (Vector2.Distance(toLoad,toUnload + new Vector2(x,y)) > 1.25f)
                {
                    UnloadChunk(toUnload + new Vector2(x, y));
                } 
            }
        }
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                yield return new WaitForEndOfFrame();
                if (Mathf.Abs(x) == Mathf.Abs(y) && x != 0)
                {
                    continue;
                }
                if(tunnelMap.ContainsKey(toLoad+new Vector2(x,y)))
                {
                    //print("Chunk loaded");
                    LoadChunk(toLoad + new Vector2(x, y), GetSeededChunkId(toLoad + new Vector2(x, y)) );
                }
            }
        }
    }
    
    public void UnloadChunk(Vector2 coords)
    {
        if (chunkMap.ContainsKey(coords))
        {
            chunkMap[coords].SetActive(false);
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        SetSeeds();
        GenerateNewMap();
        int x = (int)Mathf.Round((Camera.main.transform.position.x / CHUNKSIZE_X));
        int y = (int)Mathf.Round((Camera.main.transform.position.y / CHUNKSIZE_Y));
        CurrentCameraCoords = new Vector2(x, y);
    }

    public void GenerateNewMap()
    {
        DespawnIteractables();

        foreach (GameObject chunk in chunkMap.Values)
        {
            Destroy(chunk);
        }
        chunkMap = new Dictionary<Vector2, GameObject>();
        exploredChunks = new Dictionary<Vector2, GameObject>();
        tunnelMap = new Dictionary<Vector2, int>();
        tunnelMap = CreateCollisonMap(25, 25, mapSpawnCard.numberOfBossChunks + mapSpawnCard.numberOfSpecialChunks + mapSpawnCard.numberOfStandardChunks + mapSpawnCard.numberOfTreasureChunks);
        currentFloor++;
        if (debug_GenerateWholeMap)
        {
            foreach (Vector2 v in tunnelMap.Keys)
            {
                GenerateNewChunk(v, GetSeededChunkId(v));
            }
        }
    }

    private static void DespawnIteractables()
    {
        foreach (Interactable go in FindObjectsOfType<Interactable>())
        {
            Destroy(go.gameObject);
        }
    }

    void SetSeeds()
    {
        xSeed = new System.Random(seed.Next());
        negativeXSeed = new System.Random(seed.Next());
        ySeed = new System.Random(seed.Next());
        negativeYSeed = new System.Random(seed.Next());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
        {
            GenerateNewMap();
        }
        int x = (int) Mathf.Round((Camera.main.transform.position.x / CHUNKSIZE_X));
        int y = (int)Mathf.Round((Camera.main.transform.position.y / CHUNKSIZE_Y));
        CurrentCameraCoords = new Vector2(x, y);
    }

    private void LoadChunk(Vector2 coords, int id)
    {
        if(!chunkMap.ContainsKey(coords))
        {
            GenerateNewChunk(coords, id);
        }
        else
        {
            chunkMap[coords].SetActive(true);
        }
    }

    private void GenerateNewChunk(Vector2 coords, int id)
    {
        bool roomIsUp = false;
        bool roomIsDown = false;
        bool roomIsRight = false;
        bool roomIsLeft = false;
        GetRoomOpeningsNeeded(coords, ref roomIsUp, ref roomIsDown, ref roomIsRight, ref roomIsLeft);

        GameObject chunk = Instantiate(chunkTiles[id], new Vector3(coords.x * CHUNKSIZE_X, 0,coords.y * CHUNKSIZE_Y), Quaternion.identity, mapParent);
        chunk.GetComponent<ChunkSettings>().AdjustExits(roomIsUp, roomIsDown, roomIsRight, roomIsLeft);
        chunk.GetComponent<ChunkSettings>().mapDebugInfo += debugMessage;
        int x, y;
        GetSeedsOutOfSeedMap(coords, out x, out y);
        chunk.GetComponent<ChunkSettings>().myRandom = new System.Random(((int)x / 100) + ((int)y / 100));
        if(debug_GenerateWholeMap)
        { 
            chunk.SetActive(false);
        }
        chunkMap.Add(coords, chunk);
        debugMessage = "";
    }

    private void GetSeedsOutOfSeedMap(Vector2 coords, out int x, out int y)
    {
        x = 0;
        y = 0;
        if (coords.x >= 0)
        {
            x = xSeedMap[(int)coords.x];
        }
        else
        {
            x = negativeXSeedMap[(int)Mathf.Abs(coords.x) - 1];
        }
        if (coords.y >= 0)
        {
            y = ySeedMap[(int)coords.y];
        }
        else
        {
            y = negativeYSeedMap[(int)Mathf.Abs(coords.y) - 1];
        }
    }

    public int GetSeededChunkId(Vector2 coords)
    {
        if(!tunnelMap.ContainsKey(coords))
        {
            return 0;
        }
        debugMessage += "Likleyhood = ";
        CheckSeedMap(coords);
        int x = 0;
        int y = 0;
        if (coords.x >= 0)
        {
            x = xSeedMap[(int)coords.x];
        }
        else
        {
            x = negativeXSeedMap[(int)Mathf.Abs(coords.x) - 1];
        }
        if (coords.y >= 0)
        {
            y = ySeedMap[(int)coords.y];
        }
        else
        {
            y = negativeYSeedMap[(int)Mathf.Abs(coords.y) - 1];
        }
        bool roomIsUp = false;
        bool roomIsDown = false;
        bool roomIsRight = false;
        bool roomIsLeft = false;
        GetRoomOpeningsNeeded(coords, ref roomIsUp, ref roomIsDown, ref roomIsRight, ref roomIsLeft);

        List<int> compatibleChunks = new List<int>();
        float combinedChanceOfAllCompatibleChunks = 0;
        ChunkSettings.ChunkType searchedType = ChunkSettings.ChunkType.Standard;
        switch (tunnelMap[coords])
        {
            case 0:
                searchedType = ChunkSettings.ChunkType.Standard;
                break;
            case 1:
                searchedType = ChunkSettings.ChunkType.Standard;
                break;
            case 2:
                searchedType = ChunkSettings.ChunkType.Treasure;
                break;
            case 3:
                searchedType = ChunkSettings.ChunkType.Boss;
                break;
            case 4:
                searchedType = ChunkSettings.ChunkType.Special;
                break;
            case 5:
                searchedType = ChunkSettings.ChunkType.Spawn;
                break;
            case 6:
                searchedType = ChunkSettings.ChunkType.Trader;
                break;
        }
        for (int o = 0; o < chunkTiles.Count; o++)
        {
            if (chunkTiles[o].GetComponent<ChunkSettings>().CheckOpeningsAvaiable(roomIsUp, roomIsDown, roomIsRight, roomIsLeft))
            {
                if (chunkTiles[o].GetComponent<ChunkSettings>().chunkType == searchedType) 
                {
                    compatibleChunks.Add(o);
                    combinedChanceOfAllCompatibleChunks += chunkTiles[o].GetComponent<ChunkSettings>().weight;
                }
            }
        }
        int chunkId = -1;
        float likelyHood = new System.Random((x/100) + (y/100)).Next(0, (int)combinedChanceOfAllCompatibleChunks + 1);
        debugMessage += "Seed:" + x * y + " Chance: " +  likelyHood  ;
        int i = 0;
        foreach(int id in compatibleChunks)
        {
            if (chunkTiles[i].GetComponent<ChunkSettings>().CheckOpeningsAvaiable(roomIsUp, roomIsDown, roomIsRight, roomIsLeft))
            {
                if (likelyHood <= chunkTiles[id].GetComponent<ChunkSettings>().weight)
                {
                    chunkId = compatibleChunks[i];
                    break;
                }
                likelyHood -= chunkTiles[id].GetComponent<ChunkSettings>().weight;
            }
            i++;
        }
        debugMessage += "/"+ likelyHood;
        if (chunkId == -1)
        {
            chunkId = compatibleChunks[new System.Random(x - y).Next(0, compatibleChunks.Count)];   
        }
        return chunkId;
    }


    private bool CheckIfListContainsChunk(List<GameObject> chunkList,GameObject chunk)
    {
        foreach(GameObject g in chunkList)
        {
            if(g.name == chunk.name)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks all Room cardinals for entries needed
    /// </summary>
    /// <param name="coords"></param>
    /// <param name="roomIsUp"></param>
    /// <param name="roomIsDown"></param>
    /// <param name="roomIsRight"></param>
    /// <param name="roomIsLeft"></param>
    private void GetRoomOpeningsNeeded(Vector2 coords, ref bool roomIsUp, ref bool roomIsDown, ref bool roomIsRight, ref bool roomIsLeft)
    {
        if (tunnelMap.ContainsKey(new Vector2(coords.x + 1, coords.y)))
        {
            roomIsRight = true;
        }

        if (tunnelMap.ContainsKey(new Vector2(coords.x - 1, coords.y)))
        {
            roomIsLeft = true;
        }

        if (tunnelMap.ContainsKey(new Vector2(coords.x, coords.y + 1)))
        {
            roomIsUp = true;
        }

        if (tunnelMap.ContainsKey(new Vector2(coords.x, coords.y - 1)))
        {
            roomIsDown = true;
        }
    }

    private void GetRoomOpeningsNeeded(Vector2 coords, ref bool roomIsUp, ref bool roomIsDown, ref bool roomIsRight, ref bool roomIsLeft,Dictionary<Vector2,int> map)
    {
        if (map.ContainsKey(new Vector2(coords.x + 1, coords.y)))
        {
            roomIsRight = true;
        }

        if (map.ContainsKey(new Vector2(coords.x - 1, coords.y)))
        {
            roomIsLeft = true;
        }

        if (map.ContainsKey(new Vector2(coords.x, coords.y + 1)))
        {
            roomIsUp = true;
        }

        if (map.ContainsKey(new Vector2(coords.x, coords.y - 1)))
        {
            roomIsDown = true;
        }
    }

    public Vector2 GetClosestChunk(int chunkId, int radius,int ignoreRadius,List<Vector2>ignoreList)
    {
        int x = (int)CurrentCameraCoords.x;
        int y = (int)CurrentCameraCoords.y;
        Vector2 foundChunk = Vector2.zero;
        float distanceToClosestChunk = 10000;
        for(int sX = x+ radius; sX > x- radius; sX--)
        {
            for (int sY = y + radius; sY > y - radius; sY--)
            {
                
                Vector2 vector = new Vector2(sX, sY);
                if ((sX < x + ignoreRadius && sX > x - ignoreRadius) && (sY < y + ignoreRadius && sY > y - ignoreRadius)|| ignoreList.Contains(vector))
                {
                    continue;
                }
                if (chunkId == GetSeededChunkId(vector))
                {
                    float distanceToFoundChunk = Vector2.Distance(currentCameraCoords, vector);
                    if (distanceToFoundChunk < distanceToClosestChunk)
                    {
                        distanceToClosestChunk = distanceToFoundChunk;
                        foundChunk = vector;
                    }
                }
            }
        }
        return foundChunk;
    }

    public Dictionary<Vector2,int> GetAllChunksOfType(int chunkId,int radius,int ignoreRadius,Dictionary<Vector2,int> existingMap)
    {

        int x = (int)CurrentCameraCoords.x;
        int y = (int)CurrentCameraCoords.y;
        Dictionary<Vector2, int> foundChunks = new Dictionary<Vector2, int>();
        for (int sX = x + radius; sX > x - radius; sX--)
        {
            for (int sY = y + radius; sY > y - radius; sY--)
            {
                Vector2 vector = new Vector2(sX, sY);
                if ((sX < x + ignoreRadius && sX > x - ignoreRadius) && (sY < y + ignoreRadius && sY > y - ignoreRadius) || existingMap.ContainsKey(vector))
                {
                    continue;
                }
                if (chunkId == GetSeededChunkId(vector))
                {
                    existingMap.Add(vector, chunkId);
                    foundChunks.Add(vector, chunkId);
                }
            }
        }
        return foundChunks;
    }

    public Dictionary<Vector2, int> GetAllChunksOfType(List<int> chunkIds, int radius, int ignoreRadius, Dictionary<Vector2, int> existingMap)
    {

        int x = (int)CurrentCameraCoords.x;
        int y = (int)CurrentCameraCoords.y;
        Dictionary<Vector2, int> foundChunks = new Dictionary<Vector2, int>();
        for (int sX = x + radius; sX > x - radius; sX--)
        {
            for (int sY = y + radius; sY > y - radius; sY--)
            {
                Vector2 vector = new Vector2(sX, sY);
                if ((sX < x + ignoreRadius && sX > x - ignoreRadius) && (sY < y + ignoreRadius && sY > y - ignoreRadius) || existingMap.ContainsKey(vector) 
                    || (chunkMap.ContainsKey(vector) && chunkMap[vector].GetComponent<ChunkSettings>() != null && chunkMap[vector].GetComponent<ChunkSettings>().Concluded == true) || !tunnelMap.ContainsKey(vector))
                {
                    continue;
                }
                foreach(int chunkId in chunkIds)
                { 
                    if (chunkId == GetSeededChunkId(vector))
                    {
                        existingMap.Add(vector, chunkId);
                        foundChunks.Add(vector, chunkId);
                    }
                }
            }
        }
        return foundChunks;
    }
    private Vector2 GetClosestChunk(int chunkId, int radius, int ignoreRadius)
    {
        int x = (int)CurrentCameraCoords.x;
        int y = (int)CurrentCameraCoords.y;
        Vector2 foundChunk = Vector2.zero;
        float distanceToClosestChunk = 10000;
        for (int sX = x + radius; sX > x - radius; sX--)
        {
            for (int sY = y + radius; sY > y - radius; sY--)
            {
                Vector2 vector = new Vector2(sX, sY);
                if ((sX < x + ignoreRadius && sX > x - ignoreRadius) && (sY < y + ignoreRadius && sY > y - ignoreRadius))
                {
                    continue;
                }
                if (chunkId == GetSeededChunkId(vector))
                {
                    float distanceToFoundChunk = Vector2.Distance(currentCameraCoords, vector);
                    if (distanceToFoundChunk < distanceToClosestChunk)
                    {
                        distanceToClosestChunk = distanceToFoundChunk;
                        foundChunk = vector;
                    }
                }
            }
        }
        return foundChunk;
    }


    private Dictionary<Vector2,int> CreateCollisonMap(int x, int y, int minNumberOfRooms)
    {
        //if(CheckForSpecialSeed(Director.globalRandomSeed))
        //{
        //    return GenerateSpecialSeed(Director.globalRandomSeed);
        //}
        int numberOfRooms = 0;
        int repeats = 40 + minNumberOfRooms;
        Dictionary<Vector2, int> chunksEligableForSpawn = new Dictionary<Vector2, int>();
        do
        {
            //placeLocations(map.locations, map);


            int maxTunnels = 2; // max number of tunnels possible
            int maxLength = 4; // max length each tunnel can have

            int currentRow = 0;//(int)Mathf.Floor(Random.Range(15,x-15)); // our current row - start at a random spot
            int currentColumn = 0; //(int)Mathf.Floor(Random.Range(15, y-15)); // our current column - start at a random spot
            Vector2 lastDirection = Vector2.zero; // save the last direction we went
            Vector2 randomDirection; // next turn/direction - holds a value from directions

            // lets create some tunnels - while maxTunnels, dimentions, and maxLength  is greater than 0.

            while (maxTunnels > 0 && x > 0 && y > 0 && maxLength > 0 /*checkLocationConnection(map) < map.locations.Count-1*/)
            {

                // lets get a random direction - until it is a perpendicular to our lastDirection
                // if the last direction = left or right,
                // then our new direction has to be up or down,
                // and vice versa
                do
                {
                    int rnd = (int)Mathf.Round(seed.Next(-1, 2));
                    int apply = (int)Mathf.Ceil(seed.Next(-1, 2));
                    if (apply == 0)
                    {
                        randomDirection = new Vector2(0, rnd);
                    }
                    else
                    {
                        randomDirection = new Vector2(rnd, 0);
                    }
                } while ((randomDirection.x == -lastDirection.x && randomDirection.y == -lastDirection.y) || (randomDirection.x == lastDirection.x && randomDirection.y == lastDirection.y));

                int randomLength = (int)Mathf.Ceil(seed.Next(1, maxLength)); //length the next tunnel will be (max of maxLength)
                int tunnelLength = 0; //current length of tunnel being created

                // lets loop until our tunnel is long enough or until we hit an edge
                while (tunnelLength < randomLength)
                {


                    //break the loop if it is going out of the mapCoords
                    if (((currentRow == 130) && (randomDirection.x == -1)) ||
                        ((currentColumn == 130) && (randomDirection.y == -1)) ||
                        ((currentRow == x - 131) && (randomDirection.x == 1)) ||
                        ((currentColumn == y - 131) && (randomDirection.y == 1)))
                    {

                        break;
                    }
                    else
                    {
                        if (!chunksEligableForSpawn.ContainsKey(new Vector2(currentRow, currentColumn)))
                        {
                            chunksEligableForSpawn.Add(new Vector2(currentRow, currentColumn), 0);
                            numberOfRooms++;
                        }
                        currentRow += (int)randomDirection.x; //add the value from randomDirection to row and col (-1, 0, or 1) to update our location
                        currentColumn += (int)randomDirection.y;
                        tunnelLength++; //the tunnel is now one longer, so lets increment that variable

                    }
                }

                if (tunnelLength > 0)
                { // update our variables unless our last loop broke before we made any part of a tunnel
                    lastDirection = randomDirection; //set lastDirection, so we can remember what way we went
                    maxTunnels--; // we created a whole tunnel so lets decrement how many we have left to create
                }
            }
            repeats--;
        }
        while (repeats > 0 && numberOfRooms < minNumberOfRooms);
        SetUpTunnelMapChunkSpawns(chunksEligableForSpawn, mapSpawnCard);

        return chunksEligableForSpawn; 
    }

    private Dictionary<Vector2, int> GenerateSpecialSeed(int globalRandomSeed)
    {
        if(globalRandomSeed == 88)
        { 
            Dictionary<Vector2, int> chunksEligableForSpawn = new Dictionary<Vector2, int>();
            chunksEligableForSpawn.Add(new Vector2(0, 0), 0);
            chunksEligableForSpawn.Add(new Vector2(0, 1), 0);
            chunksEligableForSpawn.Add(new Vector2(0, 2), 0);
            chunksEligableForSpawn.Add(new Vector2(1, 2), 0);
            chunksEligableForSpawn.Add(new Vector2(2, 2), 0);
            chunksEligableForSpawn.Add(new Vector2(-1, 0), 0);
            chunksEligableForSpawn.Add(new Vector2(-2, 0), 0);
            chunksEligableForSpawn.Add(new Vector2(-2, 1), 0);
            chunksEligableForSpawn.Add(new Vector2(-2, 2), 0);
            chunksEligableForSpawn.Add(new Vector2(0, -1), 0);
            chunksEligableForSpawn.Add(new Vector2(0, -2), 0);
            chunksEligableForSpawn.Add(new Vector2(-1, -2), 0);
            chunksEligableForSpawn.Add(new Vector2(-2, -2), 0);
            chunksEligableForSpawn.Add(new Vector2(1, 0), 0);
            chunksEligableForSpawn.Add(new Vector2(2, 0), 0);
            chunksEligableForSpawn.Add(new Vector2(2, -1), 0);
            chunksEligableForSpawn.Add(new Vector2(2, -2), 0);
            SetUpTunnelMapChunkSpawns(chunksEligableForSpawn, mapSpawnCard);
            return chunksEligableForSpawn;
        }
        return null;
    }

    private bool CheckForSpecialSeed(int globalRandomSeed)
    {
        //if(globalRandomSeed == 88)
        //{
        //    return true;
        //}
        return false;
    }

    private void SetUpTunnelMapChunkSpawns(Dictionary<Vector2, int> myMap, ScriptableObject_MapSpawnCard map)
    {
        int numberOfBossChunks = map.numberOfBossChunks;
        int numberOfTreasureChunks = map.numberOfTreasureChunks;
        int numberOfStandardChunks = map.numberOfStandardChunks;
        int numberOfSpecialChunks = map.numberOfSpecialChunks;
        int numberOfTraderChunks = map.numberOfTraderChunks;
        bool spawnChunkSet = false;
        int timeout = 0;
        while (numberOfBossChunks > 0 && timeout < 1000)
        {
            Vector2 v = myMap.ElementAt(seed.Next(0, myMap.Count)).Key;
            if(myMap[v] == 0)
            { 
                bool roomIsUp = false;
                bool roomIsDown = false;
                bool roomIsRight = false;
                bool roomIsLeft = false;
                GetRoomOpeningsNeeded(v, ref roomIsUp, ref roomIsDown, ref roomIsRight, ref roomIsLeft,myMap);
                int openings = 0;
                if (roomIsUp) openings++;
                if (roomIsDown) openings++;
                if (roomIsRight) openings++;
                if (roomIsLeft) openings++;
                if (openings == 1)
                {
                    numberOfBossChunks--;
                    myMap[v] = 3;
                }
            }
            timeout++;
        }
        while (spawnChunkSet== false && timeout < 1000)
        {
            Vector2 v = myMap.ElementAt(seed.Next(0, myMap.Count)).Key;
            if (myMap[v] == 0)
            {
                myMap[v] = 5;
                spawnChunkSet = true;
                //GameObject.Find("Player").transform.position = new Vector2(v.x * CHUNKSIZE_X,v.y * CHUNKSIZE_Y);
                //GameObject.Find("Camera Holder").transform.position = new Vector2(v.x * CHUNKSIZE_X, v.y * CHUNKSIZE_Y);
                CurrentCameraCoords = new Vector2(1000, 1000);
            }
        }
        timeout = 0;
        while (numberOfTreasureChunks > 0 && timeout < 1000)
        {
            Vector2 v = myMap.ElementAt(seed.Next(0, myMap.Count)).Key;
            if (myMap[v] == 0)
            {
                numberOfTreasureChunks--;
                myMap[v] = 2;
            }
            timeout++;
        }
        timeout = 0;
        while (numberOfSpecialChunks > 0 && timeout < 1000)
        {
            Vector2 v = myMap.ElementAt(seed.Next(0, myMap.Count)).Key;
            if (myMap[v] == 0)
            {
                numberOfSpecialChunks--;
                myMap[v] = 4;
            }
            timeout++;
        }
        timeout = 0;
        while (numberOfTraderChunks > 0 && timeout < 1000)
        {
            Vector2 v = myMap.ElementAt(seed.Next(0, myMap.Count)).Key;
            if (myMap[v] == 0)
            {
                numberOfTraderChunks--;
                myMap[v] = 6;
            }
            timeout++;
        }
    }

    private int[,] CreateArray(int num, int x, int y)
    {
        int[,] array = new int[x, y];
        for (int xZ = 0; xZ < x; xZ++)
            for (int yZ = 0; yZ < y; yZ++)
            {
                array[xZ, yZ] = 1;

            }
        return array;
    }

    private void CheckSeedMap(Vector2 coords)
    {
        if (coords.x >= 0)
        {
            while (xSeedMap.Count < coords.x + 1)
            {
                WriteSeedIntoSeedMap(xSeedMap, xSeed);
            }
        }
        else
        {
            while (negativeXSeedMap.Count < Mathf.Abs(coords.x) + 1)
            {
                WriteSeedIntoSeedMap(negativeXSeedMap, negativeXSeed);
            }
        }
        if (coords.y >= 0)
        {
            while (ySeedMap.Count < coords.y + 1)
            {
                WriteSeedIntoSeedMap(ySeedMap, ySeed);
            }
        }
        else
        {
            while (negativeYSeedMap.Count < Mathf.Abs(coords.y) + 1)
            {
                WriteSeedIntoSeedMap(negativeYSeedMap, negativeYSeed);
            }
        }
    }

    private int WriteSeedIntoSeedMap(List<int> seedMap,System.Random seed)
    {
        seedMap.Add(seed.Next());
        return 0;
    }
}

