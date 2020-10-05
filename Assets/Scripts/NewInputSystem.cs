using BlastProof;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using System.Runtime.CompilerServices;
using MEC;
using Boo.Lang.Environments;

public class NewInputSystem : MonoBehaviour
{
    private Camera _mainCam;

    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private SpriteShapeController victim;
    [SerializeField] private Material capMat;
    [SerializeField] private LevelManager LM;

    public LayerMask obstacleLayer;
    public LayerMask polygonLayer;

    private Cutter cutter;
    private Vector2[] polygon;

    private Material _textureMat;

    private TrailRenderer trailrenderer;

    private Circle circle;

    private int o = 0;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _currentPos;

    private Vector3 pointlast;

    private List<IntersectionPoint> _intersectionPoints = new List<IntersectionPoint>();
    IntersectionPoint lastIntersectionPoint = IntersectionPoint.zero;
    IntersectionPoint correctedLastIntersectionPoint = IntersectionPoint.zero;

    private float distanceFromCam;
    private float lastCutTime = 0.0f;

    private const float _CIRCLE_RADIUS = 1.25f;
    private const float _COOLDOWN_TIME = 0.0f;
    private const float _PREDICTION_FACTOR = 2f;
    private const float _LOOP_TIME = 0.008333f;

    private bool hasStarted = false;
    private bool hasEnded = false;
    private bool isEnabled = true;
    private bool hasStartedOutside = false;

    void Start()
    {
        trailrenderer = GetComponent<TrailRenderer>();
        trailrenderer.forceRenderingOff = true;

        circle = new Circle(transform.position, _CIRCLE_RADIUS);

        _mainCam = Camera.main;
        polygon = cbm.ToArray();
        _startPos = cbm.PolygonCenter;
        _endPos = cbm.PolygonCenter;

        cutter = new Cutter();
        distanceFromCam = Mathf.Abs(_mainCam.transform.position.z - victim.transform.position.z);

    }

    private void FixedUpdate()
    {
        MoveBlade();
    }

    //Blade is generated when player swipes
    private void MoveBlade()
    {

        Vector3 position = Input.mousePosition;
        position.z = distanceFromCam - 0.5f;
        position = _mainCam.ScreenToWorldPoint(position);
        transform.position = position;

        circle.UpdatePosition(position);

        bool isOutside = (Mathematics.PointInPolygon(position, polygon) ? false : true);
        var intersections = circle.GetIntersections(polygon);

        if (lastIntersectionPoint != IntersectionPoint.zero &&
            Physics2D.Linecast(lastIntersectionPoint._pos, position, obstacleLayer) && !isOutside
            && position != Vector3.zero)
        {
            LM.CollidedWithObject();
            isEnabled = false;
        }
        trailrenderer.forceRenderingOff = false;

        Touch[] touch = Input.touches;      

        if (isEnabled && Input.GetMouseButton(0))
        {
            _startPos = _endPos;
            _endPos = position;
            _currentPos = position;

            if (_startPos != cbm.PolygonCenter && _endPos != cbm.PolygonCenter)
            {
                NewIntersections(_startPos, _endPos);
                Cut();
            }

            if (isOutside)
            {
                lastIntersectionPoint = IntersectionPoint.zero;
            }
        }
        else
        {
            _startPos = cbm.PolygonCenter;
            _endPos = cbm.PolygonCenter;
            hasStarted = false;
            lastIntersectionPoint = IntersectionPoint.zero;
            trailrenderer.forceRenderingOff = true;
            trailrenderer.Clear();
        }
    }

    private void CorrectLastIntersectionPoint(int removedPoints)
    {
        correctedLastIntersectionPoint._previousBoundaryPoint = lastIntersectionPoint._previousBoundaryPoint - removedPoints;
        correctedLastIntersectionPoint._nextBoundaryPoint = lastIntersectionPoint._nextBoundaryPoint - removedPoints;
        correctedLastIntersectionPoint._pos = lastIntersectionPoint._pos;
    }

    private void CorrectLastIntersectionPoint()
    {
        for (int i = 1; i <= cbm.m_CustomBox.Count; i++)
        {
            float distanceKJ = Vector3.Distance(lastIntersectionPoint._pos, cbm.m_CustomBox[i - 1].m_pos);
            float distanceKI = Vector3.Distance(lastIntersectionPoint._pos, cbm.m_CustomBox[i % cbm.m_CustomBox.Count].m_pos);
            float distanceIJ = Vector3.Distance(cbm.m_CustomBox[i - 1].m_pos, cbm.m_CustomBox[i % cbm.m_CustomBox.Count].m_pos);           

            //bool isCloseEnough = Mathematics.DistancePointLine2D(lastIntersectionPoint._pos, cbm.m_CustomBox[i - 1].m_pos, cbm.m_CustomBox[i % cbm.m_CustomBox.Count].m_pos) < 0.1f;

            if (Mathf.Approximately(distanceKI + distanceKJ, distanceIJ))
            {
                lastIntersectionPoint._previousBoundaryPoint = (i - 1);
                lastIntersectionPoint._nextBoundaryPoint = i;
            }
        }
    }

    private void CorrectIntersectionPointsLeft(int index)
    {
        for (; index < _intersectionPoints.Count; index++)
        {
            for (int i = 1; i <= cbm.m_CustomBox.Count; i++)
            {
                float distanceKJ = Vector3.Distance(_intersectionPoints[index]._pos, cbm.m_CustomBox[i - 1].m_pos);
                float distanceKI = Vector3.Distance(_intersectionPoints[index]._pos, cbm.m_CustomBox[i % cbm.m_CustomBox.Count].m_pos);
                float distanceIJ = Vector3.Distance(cbm.m_CustomBox[i - 1].m_pos, cbm.m_CustomBox[i % cbm.m_CustomBox.Count].m_pos);

                //bool isCloseEnough = Mathematics.DistancePointLine2D(lastIntersectionPoint._pos, cbm.m_CustomBox[i - 1].m_pos, cbm.m_CustomBox[i % cbm.m_CustomBox.Count].m_pos) < 0.1f;

                if (Mathf.Approximately(distanceKI + distanceKJ, distanceIJ))
                {
                    _intersectionPoints[index]._previousBoundaryPoint = (i - 1);
                    _intersectionPoints[index]._nextBoundaryPoint = i;
                }
            }
        }
    }

    private void NewIntersections(Vector3 startPos, Vector3 endPos)
    {
        _intersectionPoints.Clear();
        List<IntersectionPoint> ip = cbm.GetIntersections(startPos, endPos);

        List<IntersectionPoint> ipWithLastPoint;

        if (lastIntersectionPoint != IntersectionPoint.zero)
        {
            _intersectionPoints.Add(lastIntersectionPoint);
            ipWithLastPoint = cbm.GetIntersections(lastIntersectionPoint._pos, _currentPos);

            if (ipWithLastPoint.Count > 0)
            {
                for (int i = 0; i < ipWithLastPoint.Count; i++)
                {
                    IntersectionPoint tempPoint = ipWithLastPoint[i];
                    int count = 0;

                    for (int j = 0; j < ip.Count; j++)
                    {
                        if (tempPoint._previousBoundaryPoint != ip[j]._previousBoundaryPoint ||
                            tempPoint._nextBoundaryPoint != ip[j]._nextBoundaryPoint)
                        {
                            count++;
                        }
                    }

                    if (tempPoint != IntersectionPoint.zero &&
                        (tempPoint._previousBoundaryPoint != lastIntersectionPoint._previousBoundaryPoint ||
                        tempPoint._nextBoundaryPoint != lastIntersectionPoint._nextBoundaryPoint )&&
                        count == ip.Count
                        )
                    {
                        _intersectionPoints.Add(tempPoint);
                    }
                }
            }
        }

        for (int i = 0; i < ip.Count; i++)
        {
            _intersectionPoints.Add(ip[i]);
        }

        if (_intersectionPoints.Count > 0)
        {
            if (!hasStarted)
            {
                hasStarted = true;
            }

            lastIntersectionPoint = _intersectionPoints[_intersectionPoints.Count - 1];

            Debug.Log("Last Point: " + lastIntersectionPoint._previousBoundaryPoint.ToString() + "," + lastIntersectionPoint._nextBoundaryPoint.ToString());

            if (_intersectionPoints.Count > 0)
            {
               _intersectionPoints.Sort();
            }
        }
    }

    private void Cut()
    {
        for (int i = 1; i < _intersectionPoints.Count; i++)
        {
            Vector3 middlePoint = (_intersectionPoints[i - 1]._pos + _intersectionPoints[i]._pos) / 2f;


            if (_intersectionPoints[i - 1]._previousBoundaryPoint != _intersectionPoints[i]._previousBoundaryPoint ||
                _intersectionPoints[i - 1]._nextBoundaryPoint != _intersectionPoints[i]._nextBoundaryPoint)
            {
                bool isMidInside = Mathematics.PointInPolygon(middlePoint, polygon);

                if (isMidInside)
                {
                    List<IntersectionPoint> tempList = new List<IntersectionPoint>();
                    tempList.Add(_intersectionPoints[i - 1]);
                    tempList.Add(_intersectionPoints[i]);

                    //int j = 0;
                    //foreach (var item in tempList)
                    //{
                    //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //    cube.transform.position = item._pos;

                    //    var name = "POINT: " + " " + j.ToString() + " " + item._previousBoundaryPoint.ToString() + "," + item._nextBoundaryPoint.ToString();

                    //    Debug.Log(name);
                    //    cube.name = name;

                    //    j++;
                    //}

                    var x = cutter.Cut(victim, _textureMat, tempList, LM.Obstacles, out GameObject cuttedPiece);

                    if (x)
                    {
                        polygon = cbm.ToArray();
                        LM.AddPieceToList(ref cuttedPiece);
                        LM.UpdateScore();

                        if (lastIntersectionPoint != IntersectionPoint.zero)
                        {
                            CorrectLastIntersectionPoint();
                            CorrectIntersectionPointsLeft(i + 1);
                        }
                        lastCutTime = Time.unscaledTime;
                        //lastPointIntersection = IntersectionPoint.zero;
                    }
                }
            }
        }
    }

    public void UpdateMats(Material textureMat)
    {
        _textureMat = textureMat;
    }

    private bool hasAtMostOneCommonEdge(IntersectionPoint point1, IntersectionPoint point2)
    {
        bool diffNextdiffPrev1 = point1._nextBoundaryPoint == point2._nextBoundaryPoint && point1._previousBoundaryPoint == point2._previousBoundaryPoint;
        bool diffNextdiffPrev2 = point1._nextBoundaryPoint == point2._previousBoundaryPoint && point1._previousBoundaryPoint == point2._nextBoundaryPoint;

        return !diffNextdiffPrev1 && !diffNextdiffPrev2;
    }

    public void ReEnable()
    {
        hasStarted = false;
        hasEnded = false;
        _startPos = cbm.PolygonCenter;
        _endPos = cbm.PolygonCenter;
        hasStartedOutside = false;
        trailrenderer.forceRenderingOff = true;
        _intersectionPoints.Clear();
        lastIntersectionPoint = IntersectionPoint.zero;
        isEnabled = true;
        polygon = cbm.ToArray();
    }
}



