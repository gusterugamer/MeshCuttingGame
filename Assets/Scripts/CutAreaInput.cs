using BlastProof;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class CutAreaInput : MonoBehaviour, IDragHandler, IEndDragHandler
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

    private void Awake()
    {
        circle = new Circle(Vector3.zero, 1.0f);
    }

    private void Start()
    {
        polygon = cbm.ToArray();
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        var position = Vector3.zero;
        Physics.Raycast(ray, out var hit);
        position = hit.point;

        circle.UpdatePosition(position);
        List<IntersectionPoint> intersect = circle.GetIntersections(polygon);
        var intersect2 = circle.GetIntersections2(polygon);

        if (Physics2D.Raycast(position, Vector2.zero, 0f))
        {
            if (intersect.Count != 0)
            {
                if (_startPosition == cbm.PolygonCenter)
                {
                    _startPosition = intersect[0]._pos;
                    startInterPnt = intersect[0];
                }
                else
                {
                    IntersectionPoint endIntersectionPoint = startInterPnt;
                    float maxDistance = -Mathf.Infinity;
                    for (int i = 0; i < intersect.Count;i++)
                    {
                        if (intersect[i] != startInterPnt)
                        {
                            float distance = Vector3.Distance(_startPosition, intersect[i]._pos);
                            if (distance > maxDistance)
                            {
                                maxDistance = distance;
                                endIntersectionPoint = intersect[i];
                            }
                        }
                    }
                    _endPostition = endIntersectionPoint._pos;
                    var x = MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition);
                    if (x)
                    {
                        _startPosition = cbm.PolygonCenter;
                        _endPostition = cbm.PolygonCenter;
                        polygon = cbm.ToArray();
                        startInterPnt = IntersectionPoint.zero;
                    }    
                    else
                    {
                        Debug.Log("FAILED!");
                    }
                }
                lastTime = Time.unscaledTime;
            }
        }
        else
        {
            _startPosition = position;
            _endPostition = _startPosition;
        }
  
    }

    private bool IsSameEdge(IntersectionPoint point1, IntersectionPoint point2)
    {
        return (point1._previousBoundaryPoint == point2._previousBoundaryPoint) &&
               (point1._nextBoundaryPoint == point2._nextBoundaryPoint);
    }

    private bool AreEdgesConnected(IntersectionPoint point1, IntersectionPoint point2)
    {
        bool prevNext = point1._nextBoundaryPoint == point2._previousBoundaryPoint && point2._nextBoundaryPoint != point1._previousBoundaryPoint;
        bool nextPrev = point2._nextBoundaryPoint == point1._previousBoundaryPoint && point1._nextBoundaryPoint != point2._previousBoundaryPoint;

        return prevNext || nextPrev;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;
        startInterPnt = IntersectionPoint.zero;
    }
}
