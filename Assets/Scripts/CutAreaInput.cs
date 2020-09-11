using BlastProof;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CutAreaInput : MonoBehaviour
{
    [SerializeField] private Camera _mainCamera;

    private Vector3 _startPosition = Vector3.zero;
    private Vector3 _endPostition = Vector3.zero;

    [SerializeField] private CustomBoundryBox cbm;

    public SpriteShapeController victim;
    public Material capMat;

    private IntersectionPoint startInterPnt = IntersectionPoint.zero;

    private Circle circle;

    private float oldDistanceFromPolyCenter = 16.0f;

    private Vector2[] polygon;

    private float lastTime;

    private bool _isInCollider = false;

    private void Awake()
    {
        circle = new Circle(Vector3.zero, 1f);
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(circle.Center, circle.Radius);
    }

    private void Start()
    {
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

                MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition);

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
