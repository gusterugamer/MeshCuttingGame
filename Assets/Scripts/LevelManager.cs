using UnityEngine;
using System;
using System.Collections.Generic;
using BlastProof;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private NewInputSystem InputSystem;

    private static Score score;
    private List<GameObject> obstacles = new List<GameObject>();
    private List<GameObject> cuttedObjects = new List<GameObject>();   

    public Score Score { get => score; private set => score = value; }
    public List<GameObject> Obstacles { get => obstacles; private set => obstacles = value; }   

    public delegate void ScoreChangeDelegate();
    public delegate void ResetSceneDelegate();
    public delegate void CutOBjectDelegate();

    public event ScoreChangeDelegate OnScoreChange;
    public event CutOBjectDelegate OnCuttingObject;
    public event ResetSceneDelegate OnResetScene;
   
    void Start()
    {
        //startShape = new CustomBoundryBox(cbm);
        Score = new Score(cbm.Area);         
        CreateObjectsInScene();
    } 

    public void UpdateScore()
    {
        Score.UpdateCurrentScore(cbm.Area);
        OnScoreChange?.Invoke();
    }

    private void CreateObjectsInScene()
    {        
        for (int i=-1;i<2;i++)
        {
            GameObject prefabcube = Resources.Load("Prefab/Cube") as GameObject;
            GameObject cube = Instantiate(prefabcube);
            cube.name = "object" + i;
            cube.transform.localScale = Vector3.one;
            Vector3 newPosition = new Vector3(cbm.PolygonCenter.x, cbm.PolygonCenter.y + i * 6.5f, cbm.PolygonCenter.z);
            cube.transform.position = newPosition;
            cube.AddComponent<LevelObstacle>().SetStartPosition(newPosition);
            cube.tag = "Obstacle";           
            Obstacles.Add(cube);
        }      
    }

    public void AddPieceToList(ref GameObject piece)
    {
        cuttedObjects.Add(piece);
    }

    public void CollidedWithObject()
    {
        Debug.Log("TRUE");
        Time.timeScale = 0.0f;       
        OnCuttingObject?.Invoke();
    }

    public void ResetScene()
    {
        foreach (GameObject go in cuttedObjects)
        {
            Destroy(go);
        }

        cuttedObjects.Clear();

        foreach (GameObject go in obstacles)
        {
            go.GetComponent<LevelObstacle>().Reset();
        }

        OnScoreChange?.Invoke();
        OnResetScene?.Invoke();

        cbm.ResetShape();
        score.Reset();
       
        InputSystem.ReEnable();
        Time.timeScale = 1.0f;       
    }
}
