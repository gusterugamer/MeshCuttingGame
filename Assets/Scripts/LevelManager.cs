using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using System;
using BlastProof;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private NewInputSystem InputSystem;
    [SerializeField] private JsonReader _jr;

    private SpriteShapeController sprite;

    private Camera _mainCam;

    private static Score score;
    private List<GameObject> obstacles = new List<GameObject>();
    private List<GameObject> cuttedObjects = new List<GameObject>();

    private Material textureMat;

    public Score Score { get => score; private set => score = value; }
    public List<GameObject> Obstacles { get => obstacles; private set => obstacles = value; }

    public delegate void ScoreChangeDelegate();
    public delegate void ResetSceneDelegate();
    public delegate void CutOBjectDelegate();

    public event ScoreChangeDelegate OnScoreChange;
    public event CutOBjectDelegate OnCuttingObject;
    public event ResetSceneDelegate OnResetScene;

    private void Awake()
    {
        _mainCam = Camera.main;
        Application.targetFrameRate = 60;
        textureMat = Resources.Load("Material/random") as Material;
        InputSystem.UpdateMats(textureMat);
        CreateSprite();        
    }
    void Start()
    {
        Score = new Score(cbm.Area);
        CreateObjectsInScene();
        _mainCam.transform.position = new Vector3(cbm.PolygonCenter.x, cbm.PolygonCenter.y, -Mathf.Max(cbm.MaxX, cbm.MaxY));
        WidthToWorld();
        Debug.Log(_jr.loadedLevel.points[0]);
    }

    private void WidthToWorld()
    {
        float distance = Vector3.Distance(cbm.PolygonCenter, _mainCam.transform.position);

        Vector3 viewport = new Vector3(1, 1, distance);
        Vector3 viewportCenter = new Vector3(0, 0, distance);
        Vector3 viewPortToWorld = _mainCam.ViewportToWorldPoint(viewport);
        Vector3 viewPortCenterToWorld = _mainCam.ViewportToWorldPoint(viewportCenter);

        float WidthToWorld = Mathf.Abs(Mathf.Abs(viewPortToWorld.x) - Mathf.Abs(viewPortCenterToWorld.x));        
    }

    private void CreateSprite()
    {
        sprite = cbm.GetComponent<SpriteShapeController>();
        sprite.spline.Clear();

        bool isClockWise = _jr.loadedLevel.isClockWise;  

        if (!isClockWise)
        {
            int j = 0;
            for (int i = _jr.loadedLevel.points.Length-1; i >=0 ; i--)
            {
                sprite.spline.InsertPointAt(j, _jr.loadedLevel.points[i]);
                j++;
            }
        }
        else
        {
            for (int i = 0; i < _jr.loadedLevel.points.Length; i++)
            {
                sprite.spline.InsertPointAt(i, _jr.loadedLevel.points[i]);
            }
        }

        sprite.spriteShape.fillTexture = textureMat.mainTexture as Texture2D;
        cbm.TextureSize(textureMat.mainTexture.width);
    }  

    public void UpdateScore()
    {
        Score.UpdateCurrentScore(cbm.Area);
        OnScoreChange?.Invoke();
    }

    private void CreateObjectsInScene()
    {
        for (int i = -1; i < 2; i++)
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
        OnResetScene?.Invoke();

        cbm.ResetShape();
        score.Reset();

        OnScoreChange?.Invoke();

        InputSystem.ReEnable();
    }
}
