using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ScriptableObject_MapSpawnCard", order = 1)]
public class ScriptableObject_MapSpawnCard : ScriptableObject
{
    public int numberOfTreasureChunks;
    public int numberOfStandardChunks;
    public int numberOfBossChunks;
    public int numberOfSpecialChunks;
    public int numberOfTraderChunks;
}
