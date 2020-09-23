using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
    private int id;

    [SerializeField] private JsonReader jr;

    private void Awake()
    {
        id = int.Parse(transform.name) - 1;           
    }

    public void StartLevel()
    {
        jr.LoadLevel(id);
        SceneManager.LoadSceneAsync("SelectedLevelScene", LoadSceneMode.Single);           
    } 
}
