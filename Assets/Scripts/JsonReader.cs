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

    public LevelData loadedLevel;
   
    public void Load()
    {
        string path = Application.dataPath + "/" + fileName + ".json";
        using (StreamReader stream = new StreamReader(path))
        {
            string json = stream.ReadToEnd();
            levelsD = JsonUtility.FromJson<LevelsData>(json);
        }        
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
