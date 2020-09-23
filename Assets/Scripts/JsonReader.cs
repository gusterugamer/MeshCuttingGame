using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Level")]
public class JsonReader : ScriptableObject
{
    [SerializeField] private string fileName;
    
    [SerializeField] private LevelsData levelsD;

    [SerializeField] private TextAsset jsonFile;

    public LevelData loadedLevel;
   
    public void Load()
    {        
        string json = jsonFile.text;
        levelsD = JsonUtility.FromJson<LevelsData>(json);              
    }     

    public void LoadLevel(int i)
    {
        loadedLevel = levelsD.levels[i];
    }

    [Serializable]
    private class LevelsData
    {
        public LevelData[] levels; 
    }
}
