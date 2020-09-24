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
        id = int.Parse(transform.name);           
    }

    public void StartLevel()
    {
        jr.Load(id);
        SceneManager.LoadSceneAsync("SelectedLevelScene", LoadSceneMode.Single);           
    } 
}
