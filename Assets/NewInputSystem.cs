using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class NewInputSystem : MonoBehaviour
{
    private Camera _mainCam;

    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private SpriteShapeController victim;
    [SerializeField] private Material capMat;
    [SerializeField] private LevelManager LM;

    private Cutter cutter;
    private Vector2[] polygon;

    public delegate void CutDelegate();
    public delegate void ObjectCutDelegate();

    public event CutDelegate OnCutDone;
    public event ObjectCutDelegate OnObjectCut;

    //public LayerMask layer;

    private Vector3 _startPos;
    private Vector3 _endPos;

    private bool hasStarted = false;

    void Start()
    {
        _mainCam = Camera.main;
        _startPos = cbm.PolygonCenter;
        _endPos = cbm.PolygonCenter;
        polygon = cbm.ToArray();
        cutter = new Cutter();
    }

    private void Update()
    {
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        Vector3 position;
        Physics.Raycast(ray, out var hit);
        position = hit.point;
        transform.position = position;
        if (_startPos != cbm.PolygonCenter)
        {
            _endPos = position;
            float dist = Vector2.Distance(_startPos, _endPos);
            LayerMask layer = LayerMask.NameToLayer("Obstacles");
            RaycastHit2D hit1;
            if (hit1 = Physics2D.Linecast(_startPos, _endPos))
            {
                if (hit1.collider.tag == "Obstacle")
                {
                    OnObjectCut?.Invoke();
                }
            }
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        List<Vector3> points = new List<Vector3>();
        foreach (var contact in collision.contacts)
        {
            points.Add(contact.point);
        }
        if (!hasStarted)
        {
            _startPos = transform.position;
            hasStarted = true;
        }
        else
        {
            _endPos = points[0];   
            
            Debug.Log("Start: " + _startPos + "End: " + _endPos);

            Time.timeScale = 0.0f;
            if (cutter.Cut(victim, capMat, _startPos, _endPos, LM.Obstacles))
            {
                OnCutDone?.Invoke();
                polygon = cbm.ToArray();
            }
            hasStarted = false;
            _startPos = cbm.PolygonCenter;
            _endPos = cbm.PolygonCenter;
            Time.timeScale = 1.0f;
        }
    }
}

    

