using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Level")]
public class JsonReader : ScriptableObject
{   
    private LevelData _loadedLevel;
    public LevelData loadedLevel { get => _loadedLevel; }
   
    public void Load(int i)
    {
        string path = "levels/" + i.ToString();
        string json = Resources.Load<TextAsset>(path).text;
        _loadedLevel = JsonUtility.FromJson<LevelData>(json);         
    }  
}
