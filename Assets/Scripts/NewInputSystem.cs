using BlastProof;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;
using System.Runtime.CompilerServices;

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
    IntersectionPoint lastPointIntersection = IntersectionPoint.zero;

    private float distanceFromCam;
    private float lastCutTime = 0.0f;

    private const float _CIRCLE_RADIUS = 1.25f;
    private const float _COOLDOWN_TIME = 0.15f;
    private const float _PREDICTION_FACTOR = 2f;
    private const float _LOOP_TIME = 0.008333f;

    private bool hasStarted = false;
    private bool hasEnded = false;
    private bool isEnabled = true;
    private bool hasStartedOutside = false;

    List<GameObject> testobj = new List<GameObject>();

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
        //StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        yield return new WaitForSeconds(_LOOP_TIME);
        MoveBlade();
        StartCoroutine(Loop());
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(lastPointIntersection._pos, _endPos);
    }

    private void Update()
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

        if (lastPointIntersection != IntersectionPoint.zero &&
            Physics2D.Linecast(lastPointIntersection._pos, position, obstacleLayer) && !isOutside
            && position != Vector3.zero)
        {
            LM.CollidedWithObject();
            isEnabled = false;
        }

        if (Input.GetKey(KeyCode.S))
        {
            _intersectionPoints.Clear();
            foreach (var go in testobj)
            {
                Destroy(go);
            }
        }

        if (Input.GetMouseButton(0) && isEnabled)
        {
            trailrenderer.forceRenderingOff = false;


            if (Time.unscaledTime > lastCutTime + _COOLDOWN_TIME)
            {              

                _startPos = _endPos;
                _endPos = position;
                _currentPos = position;

                if (Vector3.Distance(_startPos, _endPos) > 0.3f)
                {
                    if (_startPos != cbm.PolygonCenter && _endPos != cbm.PolygonCenter)
                    {
                        NewIntersections(_startPos, _endPos);
                        Cut();

                        if (_intersectionPoints.Count > 1)
                        {
                            hasStarted = false;
                        }

                        lastCutTime = Time.unscaledTime;
                    }
                }

                if (isOutside)
                {
                    hasStarted = false;
                    lastPointIntersection = IntersectionPoint.zero;
                }
            }
        }
        else
        {
            _startPos = cbm.PolygonCenter;
            _endPos = cbm.PolygonCenter;
            hasStarted = false;
            lastPointIntersection = IntersectionPoint.zero;
            trailrenderer.forceRenderingOff = true;            
        }
    }

    private void NewIntersections(Vector3 startPos, Vector3 endPos)
    {
        _intersectionPoints.Clear();
        List<IntersectionPoint> ip = cbm.GetIntersections(startPos, endPos);
       
        List<IntersectionPoint> ipWithLastPoint;
        if (lastPointIntersection != IntersectionPoint.zero)
        {
            _intersectionPoints.Add(lastPointIntersection);
            ipWithLastPoint = cbm.GetIntersections(lastPointIntersection._pos, _currentPos);

            if (ipWithLastPoint.Count > 0)
            {              
                for (int i=0;i<ipWithLastPoint.Count;i++)
                {
                    IntersectionPoint tempPoint = ipWithLastPoint[i];                   
                    int count = 0;

                    for (int j=0;j<ip.Count;j++)
                    {
                        if (tempPoint._previousBoundaryPoint != ip[j]._previousBoundaryPoint &&
                            tempPoint._nextBoundaryPoint != ip[j]._nextBoundaryPoint)
                        {
                            count++;
                        }
                    }
                    
                    if (tempPoint != IntersectionPoint.zero &&
                        tempPoint._previousBoundaryPoint != lastPointIntersection._previousBoundaryPoint&&
                        tempPoint._nextBoundaryPoint != lastPointIntersection._nextBoundaryPoint &&                     
                        count == ip.Count
                        )
                    {
                        _intersectionPoints.Add(tempPoint);
                    }
                }
            }
        }

        for(int i=0;i<ip.Count;i++)
        {
            _intersectionPoints.Add(ip[i]);
        }      

        if (_intersectionPoints.Count > 0)
        {
            if (!hasStarted)
            {
                hasStarted = true;
            }

            lastPointIntersection = _intersectionPoints[_intersectionPoints.Count - 1];

            _intersectionPoints.Sort();

            if (_intersectionPoints.Count > 1 && (_intersectionPoints[0]._nextBoundaryPoint < _intersectionPoints[_intersectionPoints.Count-1]._nextBoundaryPoint))
            {
                _intersectionPoints.Reverse();
            }
        }


    }

    private void Cut()
    {       
        for (int i = 1; i < _intersectionPoints.Count; i++)
        {
            Vector3 middlePoint = (_intersectionPoints[i - 1]._pos + _intersectionPoints[i]._pos) / 2f;
            bool isMidInside = Mathematics.PointInPolygon(middlePoint, polygon);

            if (isMidInside)
            {
                List<IntersectionPoint> tempList = new List<IntersectionPoint>();
                tempList.Add(_intersectionPoints[i - 1]);
                tempList.Add(_intersectionPoints[i]);

                var x = cutter.Cut(victim, _textureMat, tempList, LM.Obstacles, out GameObject cuttedPiece);

                if (x)
                {
                    polygon = cbm.ToArray();
                    LM.AddPieceToList(ref cuttedPiece);
                    LM.UpdateScore();                   
                    lastCutTime = Time.unscaledTime;
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
        lastPointIntersection = IntersectionPoint.zero;
        isEnabled = true;
        polygon = cbm.ToArray();
    }
}



