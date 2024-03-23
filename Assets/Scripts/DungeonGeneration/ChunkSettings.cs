using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class ChunkSettings : MonoBehaviour
{
    public ChunkType chunkType;
    public bool concluded = false;
    public bool upIsOpen = true;
    public bool downIsOpen = true;
    public bool rightIsOpen = true;
    public bool leftIsOpen = true;
    public bool forceOpenings = false;
    public string mapDebugInfo = "";
    public bool[] adjustedExits = new bool[4];
    public GameObject[] exits;
    public System.Random myRandom;
    public int weight = 100;
    public UnityEvent conclusionEvent;
    public bool isFertile = false;


    public enum ChunkType
    {
        Standard,
        Boss,
        Treasure,
        Special,
        Trader,
        Spawn
    }
    private void Start()
    {

    }
    // Start is called before the first frame update
    public bool CheckOpeningsAvaiable(bool up, bool down, bool right, bool left)
    {
        if (forceOpenings)
        {
            if (up != upIsOpen || down != downIsOpen || right != rightIsOpen || left != leftIsOpen)
            {
                return false;
            }
        }
        else
        {
            if (up && !upIsOpen)
            {
                return false;
            }
            if (down && !downIsOpen)
            {
                return false;
            }
            if (right && !rightIsOpen)
            {
                return false;
            }
            if (left && !leftIsOpen)
            {
                return false;
            }
        }
        return true;
    }

    public void AdjustExits(bool up, bool down, bool right, bool left)
    {
        adjustedExits = new bool[4];
        adjustedExits[0] = up;

        adjustedExits[1] = down;

        adjustedExits[2] = right;

        adjustedExits[3] = left;
    }

    public bool Concluded
    {
        get
        {
            return concluded;
        }

        set
        {
            conclusionEvent.Invoke();
            concluded = value;
        }
    }
}
