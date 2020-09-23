using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
    private int id = 0;

    [SerializeField] private JsonReader jr;

    private void Awake()
    {
        //DontDestroyOnLoad(transform.gameObject);          
    }

    public void StartLevel()
    {
        jr.LoadLevel(id);
        SceneManager.LoadSceneAsync("SelectedLevelScene", LoadSceneMode.Single);           
    } 
}
