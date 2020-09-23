using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSceneLoader : MonoBehaviour
{
    [SerializeField] private JsonReader levels;

    private List<LevelData> levelData;
    
    private void Awake()
    {
       levels.Load();      
    }   
}
