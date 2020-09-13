using BlastProof;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CutAreaInput : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private CustomBoundryBox cbm;
    private Cutter cutter;
    [SerializeField] private SpriteShapeController victim;
    [SerializeField] private Material capMat;

    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _endPostition = Vector3.zero;

    private Vector2[] polygon;

    private IntersectionPoint startInterPnt = IntersectionPoint.zero;

    private Circle circle;

    private float oldDistanceFromPolyCenter = 16.0f;
    private float lastTime;
    
    private bool _isInCollider = false;

    public delegate void CutDelegate();

    public event CutDelegate OnCutDone;

    private void Start()
    {
        cutter = new Cutter();
        polygon = cbm.ToArray();
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;
    }

    public void Update()
    {
        GetCutPoints();
    }

    private void GetCutPoints()
    {
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        var position = Vector3.zero;
        if (Physics.Raycast(ray, out var hit))
        {
            position = hit.point;
        }
        else return;

        position.z = 0f;       

        if (Physics2D.Raycast(position, Vector2.zero, 0f))
        {
            _isInCollider = true;
            //_startPosition = position;       
        }
        else
        {
            if (!_isInCollider)
            {
                _startPosition = position;
                _endPostition = position;
            }
            else
            {
                _endPostition = position;

                if (cutter.Cut(victim, capMat, _startPosition, _endPostition))
                {
                    OnCutDone?.Invoke();
                }

                _isInCollider = false;
                _startPosition = Vector3.zero;
                _endPostition = Vector3.zero;
            }
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;  
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //GetCutPoints();
    }
}
