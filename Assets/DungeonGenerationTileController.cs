using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.AI;
using UnityEngine;
using static Highlighters.HighlighterTrigger;

public class DungeonGenerationTileController : MonoBehaviour
{
    public bool wallsCheck = false;
    public bool floorCheck = false;
    public bool stairCheck = false;
    public bool checkRailing = false;
    public bool doorsCheck = false;

    [Header("Debug")]
    public bool generateOnStart = false;

    private static List<DungeonGenerationTileController> dungeonGenerationTileControllers = new List<DungeonGenerationTileController>();

    public static void GenerateAll(MonoBehaviour source)
    {
        source.StartCoroutine(GenerateAllCoorutine());

    }

    private static IEnumerator GenerateAllCoorutine()
    {
        print("GENERATE");
        yield return new WaitForSeconds(4);
        foreach (DungeonGenerationTileController dungeonGenerationTileController in dungeonGenerationTileControllers)
        {
            dungeonGenerationTileController.StartCoroutine(dungeonGenerationTileController.Generate());
        }
        dungeonGenerationTileControllers.Clear();
        yield return new WaitForSeconds(4);
        NavMeshBuilder.BuildNavMesh();
    }

    private void Start()
    {
        dungeonGenerationTileControllers.Add(this);
        if (generateOnStart)
        {
            StartCoroutine(Generate());
        }
    }
    IEnumerator Generate()
    {
        yield return new WaitForFixedUpdate();
        if (wallsCheck)
        {
            foreach (GameObject wall in CheckForDeletion())
            {
                Destroy(wall);
            }
        }
        yield return new WaitForFixedUpdate();
        if (floorCheck)
        {
            foreach (GameObject floor in CheckFloors())
            {
                floor.SetActive(false);
                Destroy(floor);
            }
        }
        yield return new WaitForFixedUpdate();
        if (stairCheck)
        {
            CheckForStairs();
        }
        yield return new WaitForFixedUpdate();
        if(checkRailing)
        {
            CheckRailing();
        }
        yield return new WaitForFixedUpdate();
        if (wallsCheck)
        {
            ExtendWalls();
        }
    }

    private void CheckRailing()
    {
        List<Transform> railings = new List<Transform>();
        for(int i = 0; i < transform.childCount; i++)
        {
            railings.Add(transform.GetChild(i));
        }
        foreach (Transform rail in railings)
        {
            if(Physics.CheckBox(rail.position, new Vector3(0.1f, 0.1f, 0.1f), rail.rotation))
            {
                Destroy(rail.gameObject);
                continue;
            }
            RaycastHit hit;
            if (!Physics.Raycast(rail.position - rail.forward, Vector3.down, out hit, 1.5f))
            {
                rail.gameObject.SetActive(true);
            }
            else
            {
                Destroy(rail.gameObject);
            }
        }
    }

    private void ExtendWalls()
    {
        Collider[] walls = transform.GetComponentsInChildren<Collider>();
        // Raycast upwards from the top of the wall collision
        foreach (Collider wall in walls)
        {
            Vector3 center = wall.bounds.center;
            RaycastHit[] hits = Physics.RaycastAll(center, Vector3.up, 4);
            if (hits.Length > 0)
            {
                bool hasHit = false;
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.gameObject == wall)
                    {
                        continue;
                    }
                    if (hit.collider.gameObject.tag == "Floor" || hit.collider.gameObject.tag == "Stair" || hit.collider.gameObject.tag == "Wall")
                    {
                        hasHit = true;
                        break;
                    }
                }
                if(!hasHit) 
                { 
                    Instantiate(wall.gameObject, wall.transform.position + new Vector3(0, 2, 0), wall.transform.rotation, transform); 
                }
            }
            else
            { 
                Instantiate(wall.gameObject, wall.transform.position + new Vector3(0, 2, 0), wall.transform.rotation, transform);             
            }
        }
    }

    private GameObject[] CheckFloors()
    {
        Collider[] floors = transform.GetComponentsInChildren<Collider>();
        List<GameObject> deletionList = new List<GameObject>();
        foreach (Collider floor in floors)
        {
            RaycastHit[] hits = Physics.RaycastAll(floor.bounds.center, Vector3.down, 44.9f);
            if (hits.Length > 0)
            {
                foreach(RaycastHit hit in hits)
                {
                    if(hit.collider != null) print(hit.collider.gameObject.name);

                    if (hit.collider != null && hit.collider.gameObject.tag == "Stair")
                    {
                        deletionList.Add(floor.gameObject);
                        break;
                    }
                }
            }
        }
        return deletionList.ToArray();
    }

    private void CheckForStairs()
    {
        Transform floor_BottomCheck = transform.GetChild(0);
        Transform floor_UpperCheck = transform.GetChild(1);
        Collider[] coliders;
        RaycastHit hit;
        bool deletion = true;
        coliders = Physics.OverlapSphere(floor_UpperCheck.position, 0.5f);
        if (coliders.Length > 0)
        {
            foreach (Collider colider in coliders)
            {
                if (colider.gameObject.tag == "Floor")
                {
                    deletion = false;
                    break;
                }
            }
        }
        if (deletion)
        {
            gameObject.SetActive(false);
            //Destroy(gameObject, 3f);
        }
    }

    private GameObject[] CheckForDeletion()
    {
        Collider[] walls = transform.GetComponentsInChildren<Collider>();
        List<GameObject> deletionList = new List<GameObject>();
        foreach (Collider wall in walls)
        {
            Vector3 center = wall.bounds.center;
            // Raycast behind from the center 0.1f units
            RaycastHit hit;
            if (Physics.Raycast(center, -wall.transform.forward, out hit, 0.1f))
            {
                if (hit.collider.gameObject.tag == "Wall")
                {
                    //deletionList.Add(wall.gameObject);
                    continue;
                }
            }
            // Check if wall is inside another wall
            Collider[] hitColliders = Physics.OverlapBox(center, new Vector3(0.3f,0.3f,0.3f), wall.transform.rotation);
            foreach (Collider hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.tag == "Wall" && !walls.Contains(hitCollider))
                {
                    //float distance = Vector3.Distance(center, hitCollider.bounds.center);
                    //if(distance < 0.001f)
                    //{
                    //    print($"Distance: {distance} between {wall.gameObject.name} and {hitCollider.gameObject.name}");
                    //    print($"Center: {wall.GetInstanceID()} and {hitCollider.GetInstanceID()}");
                    //}
                    //Debug.DrawLine(center, hitCollider.bounds.center, Color.red, 33f);
                    deletionList.Add(wall.gameObject);
                    break;
                }
            }

        }
        return deletionList.ToArray();
    }

}
