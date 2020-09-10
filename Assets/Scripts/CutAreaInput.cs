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
    private Vector3 prevStartPos = Vector3.zero;
    private Vector3 prevEndPos = Vector3.zero;

    private const float MIN_DISTANCE = 0.05f;
    private const float MAX_ANGLE = 179.0f;

    [SerializeField] private CustomBoundryBox cbm;

    public SpriteShapeController victim;
    public Material capMat;

    private IntersectionPoint startInterPnt = IntersectionPoint.zero;

    private Circle circle;

    private float distanceFromPolyCenter = 16.0f;

    private Vector2[] polygon;

    private float lastTime;

    private void Awake()
    {
        circle = new Circle(Vector3.zero, 1.0f);
    }

    private void Start()
    {
        polygon = cbm.ToArray();
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
                if (startInterPnt._nextBoundaryPoint == -1)
                {
                    _startPosition = intersect[0]._pos;
                    startInterPnt = intersect[0];
                }
                else 
                {
                    _endPostition = intersect[0]._pos;
                     MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition);
                    _startPosition = cbm.PolygonCenter;
                    _endPostition = cbm.PolygonCenter;
                    startInterPnt = IntersectionPoint.zero;
                    polygon = cbm.ToArray();
                }
            }

            else if (intersect.Count == 2 && distanceFromPolyCenter < currentDistance)
            {
                startInterPnt = IntersectionPoint.zero;
                if ((intersect[0]._nextBoundaryPoint == intersect[1]._previousBoundaryPoint) ||
                    (intersect[0]._previousBoundaryPoint == intersect[1]._nextBoundaryPoint))
                {
                    _startPosition = intersect[0]._pos;
                    _endPostition = intersect[1]._pos;

                    //if (Mathematics.IsVectorsAproximately(prevStartPos, _startPosition) || Mathematics.IsVectorsAproximately(prevEndPos, _endPostition))
                    //{
                    //    return;
                    //}
                    //else
                    //{
                    //    prevStartPos = _startPosition;
                    //    prevEndPos = _endPostition;
                    //}

                    bool distanceOverThreshold = Vector3.Distance(_startPosition, _endPostition) > MIN_DISTANCE;
                    bool angleUnderThreshold = Mathf.Abs(Vector3.Angle(_startPosition, _endPostition)) < MAX_ANGLE;

                    if (distanceOverThreshold && angleUnderThreshold && Time.unscaledTime - lastTime > 0.15f)
                    {
                        MeshCut.StartCutting(victim, capMat, _startPosition, _endPostition);
                        _startPosition = cbm.PolygonCenter;
                        _endPostition = cbm.PolygonCenter;
                        lastTime = Time.unscaledTime;                        
                        distanceFromPolyCenter = currentDistance;
                        polygon = cbm.ToArray();
                    }
                }
            }
            else
            {
                distanceFromPolyCenter = currentDistance;
                polygon = cbm.ToArray();
            }
        }
        else
        {
            startInterPnt = IntersectionPoint.zero;
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

    public void OnEndDrag(PointerEventData eventData)
    {
        _startPosition = cbm.PolygonCenter;
        _endPostition = cbm.PolygonCenter;
        startInterPnt = IntersectionPoint.zero;
    }
}
