using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private JsonReader _jr;

    public void LoadLevelsScene()
    {      
        SceneManager.LoadSceneAsync("LevelsScene", LoadSceneMode.Single);
    }
}
