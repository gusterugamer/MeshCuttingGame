using System;
using UnityEngine;

[Serializable]
public class LevelData
{
    public Vector2[] points;
    public string[] objectsNames;   
    public Vector2[] objectsPositions;  
    public string materialName;
    public bool isClockWise;
}
