using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Level")]
public class JsonReader : ScriptableObject
{   
    public LevelData levelsData;

    public LevelData loadedLevel;
   
    public LevelData Load(int i)
    {
        string path = "levels/" + i.ToString();
        string json = Resources.Load<TextAsset>(path).text;
        levelsData = JsonUtility.FromJson<LevelData>(json);
        loadedLevel = levelsData;
        return levelsData;
    }  
}
