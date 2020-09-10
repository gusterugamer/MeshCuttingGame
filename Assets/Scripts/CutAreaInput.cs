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

        if (Physics2D.Raycast(position, Vector2.zero, 0f))
        {
            Debug.Log("IN");
            float currentDistance = Vector3.Distance(cbm.PolygonCenter, position);
            if (intersect.Count == 1)
            {
                if (Mathematics.IsVectorsAproximately(_startPosition,cbm.PolygonCenter))
                {
                    _startPosition = intersect[0]._pos;
                    startInterPnt = intersect[0];
                }
                else
                {
                    if (!IsSameEdge(startInterPnt,intersect[0]) && startInterPnt != IntersectionPoint.zero)
                    {
                        _endPostition = intersect[0]._pos;
                        MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition);
                        _startPosition = cbm.PolygonCenter;
                        _endPostition = cbm.PolygonCenter;
                        startInterPnt = IntersectionPoint.zero;
                        polygon = cbm.ToArray();
                        Debug.Log("HERE!!!");
                    }
                }
            }

            else if (intersect.Count == 2)
            {                
                if (AreEdgesConnected(intersect[0],intersect[1]) && oldDistanceFromPolyCenter < currentDistance)
                {
                    _startPosition = intersect[0]._pos;
                    _endPostition = intersect[1]._pos;  

                    if (Time.unscaledTime - lastTime > 0.15f)
                    {
                        MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition);
                        _startPosition = cbm.PolygonCenter;
                        _endPostition = cbm.PolygonCenter;
                        lastTime = Time.unscaledTime;
                        oldDistanceFromPolyCenter = currentDistance;
                        polygon = cbm.ToArray();
                    }
                }            
            }
            else
            {
                oldDistanceFromPolyCenter = currentDistance;             
            }
        }
        else
        {
            
        }

        //if (Physics2D.Raycast(position, Vector2.zero, 0f))
        //{
        //    KeyValuePair<int, int> pair;
        //    var y = Mathematics.ClosestDistanceToPolygon(cbm.ToArray(), position, ref pair);
        //    //Debug.Log(y);
        //    if (y < 2.0f)
        //    {
        //        var x = Mathematics.ProjectPointOnLineSegment(cbm.m_CustomBox[pair.Key].m_pos, cbm.m_CustomBox[pair.Value].m_pos, position);
        //        _endPostition = x;
        //        if (MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition))
        //        {
        //            _startPosition = cbm.PolygonCenter;
        //            _endPostition = cbm.PolygonCenter;
        //        }
        //    }
        //}
        //else
        //{
        //    _startPosition = position;
        //    _endPostition = cbm.PolygonCenter;
        //}
    }

    private bool IsSameEdge(IntersectionPoint point1, IntersectionPoint point2)
    {
        return (point1._previousBoundaryPoint == point2._previousBoundaryPoint) &&
               (point1._nextBoundaryPoint == point2._nextBoundaryPoint);
    }

    private bool AreEdgesConnected(IntersectionPoint point1, IntersectionPoint point2)
    {
        bool prevNext = point1._previousBoundaryPoint == point2._nextBoundaryPoint;
        bool nextPrev = point2._previousBoundaryPoint == point1._nextBoundaryPoint;

        return prevNext || nextPrev;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;
        startInterPnt = IntersectionPoint.zero;
    }
}
