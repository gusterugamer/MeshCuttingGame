using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using System;
using BlastProof;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private CustomBoundryBox _cb;
    [SerializeField] private NewInputSystem _inputSystem;
    [SerializeField] private JsonReader _jr;

    private SpriteShapeController _sprite;

    private Camera _mainCam;

    private static Score _score;
    private List<GameObject> _obstacles = new List<GameObject>();
    private List<GameObject> _cuttedObjects = new List<GameObject>();

    private Material _textureMat;

    public Score Score { get => _score;}
    public List<GameObject> Obstacles { get => _obstacles; }

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
        _textureMat = Resources.Load("Material/random") as Material;
        _inputSystem.UpdateMats(_textureMat);
        CreateSprite();        
    }
    void Start()
    {
        _score = new Score(_cb.Area);
        CreateObjectsInScene();
        _mainCam.transform.position = new Vector3(_cb.PolygonCenter.x, _cb.PolygonCenter.y, -Mathf.Max(_cb.MaxX, _cb.MaxY));               
    }   

    private void CreateSprite()
    {
        _sprite = _cb.GetComponent<SpriteShapeController>();
        _sprite.spline.Clear();

        bool isClockWise = _jr.loadedLevel.isClockWise;  

        if (!isClockWise)
        {
            int j = 0;
            for (int i = _jr.loadedLevel.points.Length-1; i >=0 ; i--)
            {
                _sprite.spline.InsertPointAt(j, _jr.loadedLevel.points[i]);
                j++;
            }
        }
        else
        {
            for (int i = 0; i < _jr.loadedLevel.points.Length; i++)
            {
                _sprite.spline.InsertPointAt(i, _jr.loadedLevel.points[i]);
            }
        }

        _sprite.spriteShape.fillTexture = _textureMat.mainTexture as Texture2D;
        _cb.TextureSize(_textureMat.mainTexture.width);
    }  

    public void UpdateScore()
    {
        Score.UpdateCurrentScore(_cb.Area);
        OnScoreChange?.Invoke();
    }

    private void CreateObjectsInScene()
    {
        if (_jr.loadedLevel.objectsPosition.Length != 0)
        {
            for (int i = 0; i < 3; i++)
            {
                GameObject prefabcube = Resources.Load("Prefab/Cube") as GameObject;
                GameObject cube = Instantiate(prefabcube);
                cube.name = "object" + i;
                cube.transform.localScale = Vector3.one;
                Vector3 newPosition = _jr.loadedLevel.objectsPosition[i];
                cube.transform.position = newPosition;
                cube.AddComponent<LevelObstacle>().SetStartPosition(newPosition);
                cube.tag = "Obstacle";
                _obstacles.Add(cube);
            }
        }
        else
        {
            for (int i = -1; i < 2; i++)
            {
                GameObject prefabcube = Resources.Load("Prefab/Cube") as GameObject;
                GameObject cube = Instantiate(prefabcube);
                cube.name = "object" + i;
                cube.transform.localScale = Vector3.one;
                Vector3 newPosition = new Vector3(_cb.PolygonCenter.x, _cb.PolygonCenter.y + i * 6.5f, _cb.PolygonCenter.z);
                cube.transform.position = newPosition;
                cube.AddComponent<LevelObstacle>().SetStartPosition(newPosition);
                cube.tag = "Obstacle";
                _obstacles.Add(cube);
            }
        }

    }

    public void AddPieceToList(ref GameObject piece)
    {
        _cuttedObjects.Add(piece);
    }

    public void CollidedWithObject()
    {        
        OnCuttingObject?.Invoke();
    }

    public void ResetScene()
    {
        foreach (GameObject go in _cuttedObjects)
        {
            Destroy(go);
        }

        _cuttedObjects.Clear();

        foreach (GameObject go in _obstacles)
        {
            go.GetComponent<LevelObstacle>().Reset();
        }
        OnResetScene?.Invoke();

        _cb.ResetShape();
        _score.Reset();

        OnScoreChange?.Invoke();

        _inputSystem.ReEnable();
    }
}
