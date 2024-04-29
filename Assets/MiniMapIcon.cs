using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MiniMapIcon : MonoBehaviour
{
    public Sprite icon;
    public bool isStairs;

    private void Start()
    { 
        int calculatedFloor = Mathf.CeilToInt(transform.position.y / Generator3D.gridSize);
        if (isStairs)
        {
            MapUIController.Instance.GenerateMapIcon(icon, transform.position, new int[] { calculatedFloor, calculatedFloor+1 });
            return;
        }
        MapUIController.Instance.GenerateMapIcon(icon, transform.position, new int[] { calculatedFloor });
    }
}
