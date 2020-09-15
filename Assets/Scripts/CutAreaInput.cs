using BlastProof;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CutAreaInput : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private SpriteShapeController victim;
    [SerializeField] private Material capMat;
    [SerializeField] private LevelManager LM;

    private Cutter cutter;

    private Vector3 _startPosition;
    private Vector3 _endPostition;
    private Vector2[] polygon;

    private bool _isInCollider = false;
    private bool _collidedWithObject = false;

    public delegate void CutDelegate();
    public delegate void ObjectCutDelegate();

    public event CutDelegate OnCutDone;
    public event ObjectCutDelegate OnObjectCut;
    public GameObject circleObj;

    private CircleCollider2D circleCol;

    //public LayerMask layer;

    private void Start()
    {
        polygon = cbm.ToArray();
        cutter = new Cutter();
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;
        circleCol = circleObj.GetComponent<CircleCollider2D>();
    }

    public void Update()
    {
        GetCutPoints();
    }

    private void GetCutPoints()
    {
        if (Input.GetMouseButton(0))
        {
            //var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            //var position = Vector3.zero;
            //Physics.Raycast(ray, out var hit);
            //position = hit.point;                 
            //position.z = 0f;

            //circleCol.transform.position = position;
            
            //if (_startPosition == cbm.PolygonCenter)
            //{
                
            //}

            //if (Mathematics.IsPointInPolygon(position, polygon))
            //{
            //    _endPostition = position;
            //    if (_startPosition != cbm.PolygonCenter && _endPostition != cbm.PolygonCenter)
            //    {
            //        float dist = Vector2.Distance(_startPosition, _endPostition);
            //        LayerMask layer = LayerMask.NameToLayer("Obstacles");
            //        RaycastHit2D hit1;
            //        if (hit1 = Physics2D.Linecast(_startPosition, _endPostition))
            //        {
            //            if (hit1.collider.tag == "Obstacle")
            //            {
            //                OnObjectCut?.Invoke();
            //                _collidedWithObject = true;
            //            }
            //        }
            //    }

            //    if (!_collidedWithObject)
            //    {
            //        Time.timeScale = 0.0f;
            //        if (cutter.Cut(victim, capMat, _startPosition, _endPostition, LM.Obstacles))
            //        {
            //            OnCutDone?.Invoke();
            //        }
            //        _isInCollider = false;
            //        _startPosition = cbm.PolygonCenter;
            //        _endPostition = cbm.PolygonCenter;
            //        polygon = cbm.ToArray();
            //        Time.timeScale = 1.0f;
            //    }               
            //}
        }
        else
        {
            _startPosition = cbm.PolygonCenter;
            _endPostition = cbm.PolygonCenter;
        }
    }
}
