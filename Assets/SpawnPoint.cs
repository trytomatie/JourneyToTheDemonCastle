using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    public static List<Transform> spawnPoints = new List<Transform>();
    // Start is called before the first frame update
    void Awake()
    {
        spawnPoints.Add(transform);
    }
}
