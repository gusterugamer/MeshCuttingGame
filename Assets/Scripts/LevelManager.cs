using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using BlastProof;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private CutAreaInput cai;

    private static Score score;
    private List<GameObject> obstacles = new List<GameObject>();

    public Score Score { get => score; private set => score = value; }

    public delegate void ScoreChangeDelegate();
    public event ScoreChangeDelegate OnScoreChange;
   
    void Start()
    {
        Score = new Score(cbm.Area);
        cai.OnCutDone += UpdateScore;
        cai.OnObjectCut += CollidedWithObject;
        CreateObjectsInScene();
    } 

    private void UpdateScore()
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
            cube.AddComponent<LevelObstacle>();
            cube.tag = "Obstacle";           
            obstacles.Add(cube);
        }      
    }

    private void CollidedWithObject()
    {
        Debug.Log("TRUE");
    }

    public bool IsObjectsOnSameSide(Plane plane, out int _count)
    {
        int count = 0;
        foreach (var go in obstacles)
        {
            count += plane.GetSide(go.transform.position) ? 1 : -1;
        }
        _count = count;
        return Math.Abs(count) == obstacles.Count;
    }
}
