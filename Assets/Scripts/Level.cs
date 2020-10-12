using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level : MonoBehaviour
{
    private int _id;

    [SerializeField] private JsonReader _jr;

    private void Awake()
    {
        _id = int.Parse(transform.name);           
    }

    public void StartLevel()
    {
        _jr.Load(_id);
        SceneManager.LoadSceneAsync("SelectedLevelScene", LoadSceneMode.Single);           
    } 
}
