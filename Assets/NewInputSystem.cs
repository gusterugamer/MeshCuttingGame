using BlastProof;
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

    private Circle circleBig;
    private Circle circleSmall;
    private Circle currentCircle;

    public delegate void CutDelegate();
    public delegate void ObjectCutDelegate();

    public event CutDelegate OnCutDone;
    public event ObjectCutDelegate OnObjectCut;

    private IntersectionPoint firstInterPoint = IntersectionPoint.zero;

    //public LayerMask layer;

    private Vector3 _startPos;
    private Vector3 _endPos;

    private float distanceFromCam;

    private bool hasStarted = false;
    private bool hasEnded = false;

    void Start()
    {
        circleBig = new Circle(transform.position, 1.0f);
        circleSmall = new Circle(transform.position, 0.2f);
        _mainCam = Camera.main;
        _startPos = cbm.PolygonCenter;
        _endPos = cbm.PolygonCenter;
        polygon = cbm.ToArray();
        cutter = new Cutter();
        distanceFromCam = Vector3.Distance(_mainCam.transform.position, cbm.PolygonCenter);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(currentCircle.Center, currentCircle.Radius);
    }

    private void Update()
    {
        Vector3 position = Input.mousePosition;
        position.z = distanceFromCam;
        position = _mainCam.ScreenToWorldPoint(position);
        transform.position = position;

        circleBig.UpdatePosition(position);
        circleSmall.UpdatePosition(position);


        if (Mathematics.PointInPolygon(position, polygon))
        {
            currentCircle = circleBig;
        }
        else
        {
            currentCircle = circleSmall;
            hasStarted = false;
        }


        if (Input.GetMouseButton(0))
        {
            var intersections = currentCircle.GetIntersections(polygon);
            if (intersections.Count > 0)
            {
                if (!Mathematics.PointInPolygon(position, polygon))
                {
                    if (!hasStarted)
                    {
                        firstInterPoint = new IntersectionPoint(intersections[0]);
                        _startPos = position;
                        hasStarted = true;
                        hasEnded = false;
                        Debug.Log("Pos: " + _startPos);
                    }
                }
                else if (Mathematics.PointInPolygon(position, polygon))
                {
                    if (!hasStarted)
                    {
                        firstInterPoint = new IntersectionPoint(intersections[0]);
                        Vector3 dirToIntersection = (intersections[0]._pos - transform.position).normalized;
                        _startPos = transform.position + 2 * currentCircle.Radius * dirToIntersection;                       
                        hasStarted = true;
                        hasEnded = false;
                        Debug.Log("Pos: " + _startPos);
                    }
                }               
            }
            if (!hasEnded)
            {
                if (Mathematics.IsPointInPolygon(position, polygon))
                {
                    _endPos = position;
                    Vector3 cutDirection = (_endPos - _startPos).normalized;
                    _endPos = position + cutDirection * 2 * currentCircle.Radius;                   
                    Debug.Log(_endPos);
                    Debug.DrawLine(_startPos, _endPos);
                    if (cutter.Cut(victim, capMat, _startPos, _endPos, LM.Obstacles))
                    {
                        polygon = cbm.ToArray();
                        firstInterPoint = IntersectionPoint.zero;
                        hasEnded = true;
                        hasStarted = false;
                        OnCutDone?.Invoke();
                    }
                }
            }
        }
        else
        {
            hasStarted = false;
            hasEnded = false;
        }

    }

    private bool hasAtMostOneCommonEdge(IntersectionPoint point1, IntersectionPoint point2)
    {
        bool diffNextdiffPrev1 = point1._nextBoundaryPoint == point2._nextBoundaryPoint && point1._previousBoundaryPoint == point2._previousBoundaryPoint;
        bool diffNextdiffPrev2 = point1._nextBoundaryPoint == point2._previousBoundaryPoint && point1._previousBoundaryPoint == point2._nextBoundaryPoint;

        return !diffNextdiffPrev1 && !diffNextdiffPrev2;
    }

    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    List<Vector3> points = new List<Vector3>();
    //    foreach (var contact in collision.contacts)
    //    {
    //        points.Add(contact.point);
    //    }
    //    if (!hasStarted)
    //    {
    //        _startPos = transform.position;
    //        hasStarted = true;
    //    }
    //    else
    //    {
    //        hasStarted = false;
    //        _endPos = points[0];
    //        Vector3 cutDirection = (_endPos - _startPos).normalized;
    //        float radius = 2.0f;
    //        _endPos = transform.position + radius * cutDirection;

    //        Debug.Log("Start: " + _startPos + "End: " + _endPos);

    //        Time.timeScale = 0.0f;
    //        if (cutter.Cut(victim, capMat, _startPos, _endPos, LM.Obstacles))
    //        {
    //            OnCutDone?.Invoke();
    //            polygon = cbm.ToArray();
    //        }           
    //        _startPos = cbm.PolygonCenter;
    //        _endPos = cbm.PolygonCenter;
    //        Time.timeScale = 1.0f;
    //    }
    //}
}



