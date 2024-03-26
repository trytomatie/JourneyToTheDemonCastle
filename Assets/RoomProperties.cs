using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomProperties : MonoBehaviour
{
    public Vector3 roomSize;

    public void CalculateRoomSize()
    {
        float roomSizeX = 0;
        float roomSizeY = 1;
        float roomSizeZ = 0;
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {             
            if (col.transform.position.x > roomSizeX)
            {
                roomSizeX = col.transform.position.x;
            }
            if (col.transform.position.y > roomSizeY)
            {
                roomSizeY = col.transform.position.y;
            }
            if (col.transform.position.z > roomSizeZ)
            {
                roomSizeZ = col.transform.position.z;
            }
        }
        roomSize = new Vector3(Mathf.RoundToInt(roomSizeX /4), Mathf.RoundToInt(roomSizeY /4)+1, Mathf.RoundToInt(roomSizeZ/4));
        
    }
}

[CustomEditor(typeof(RoomProperties))]
public class RoomPropertiesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        RoomProperties roomProperties = (RoomProperties)target;
        if (GUILayout.Button("Calculate Room Size"))
        {
            roomProperties.CalculateRoomSize();
            // Set Editor to dirty so that the changes are saved
            EditorUtility.SetDirty(roomProperties);
        }

    }
}
