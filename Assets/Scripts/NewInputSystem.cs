using BlastProof;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

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

    private Vector3 _startPos;
    private Vector3 _endPos;

    private List<IntersectionPoint> _intersectionPoints = new List<IntersectionPoint>();

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

    void Start()
    {
        trailrenderer = GetComponent<TrailRenderer>();
        trailrenderer.forceRenderingOff = true;

        circle = new Circle(transform.position, _CIRCLE_RADIUS);

        _mainCam = Camera.main;
        polygon = cbm.ToArray();
        _startPos = Vector3.zero;
        _endPos = Vector3.zero;

        cutter = new Cutter();
        distanceFromCam = Mathf.Abs(_mainCam.transform.position.z - victim.transform.position.z);
        StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        yield return new WaitForSeconds(_LOOP_TIME);
        MoveBlade();
        StartCoroutine(Loop());
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(_startPos, _endPos);
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

        if (Input.GetMouseButton(0) && isEnabled)
        {
            trailrenderer.forceRenderingOff = false;

            if (Time.unscaledTime > lastCutTime + _COOLDOWN_TIME)
            {
                _startPos = _endPos;
                _endPos = position;
               
                List<IntersectionPoint> ip = cbm.GetIntersections(_startPos, _endPos);
                IntersectionPoint lastPoint = ip.Count>0 ? ip[ip.Count - 1] : IntersectionPoint.zero;
                foreach(var interpoint in ip)
                {
                    if (hasAtMostOneCommonEdge(lastPoint,interpoint))
                    {
                        _intersectionPoints.Add(interpoint);
                    }
                }
                Cut();   
                lastCutTime = Time.unscaledTime;
            }    
               

   
        }
        else
        {
            hasStarted = false;
            hasEnded = false;
            _startPos = position;
            _endPos = position;
            hasStartedOutside = false;
            trailrenderer.forceRenderingOff = true;
        }
    }


    private void Cut()
    {
        for (int i=1;i<_intersectionPoints.Count-1;i++)
        {
            Vector3 middlePoint = (_intersectionPoints[i - 1]._pos + _intersectionPoints[i]._pos) / 2f;
            bool isMidPointInside = Mathematics.IsPointInPolygon(middlePoint, polygon);
            if (isMidPointInside)
            {
                if (cutter.Cut(victim, _textureMat, _intersectionPoints[i - 1]._pos, _intersectionPoints[i]._pos, LM.Obstacles, out GameObject cuttedPiece))
                {
                    LM.AddPieceToList(ref cuttedPiece);
                    polygon = cbm.ToArray();
                    hasEnded = true;
                    hasStarted = false;
                    hasStartedOutside = false;
                    LM.UpdateScore();
                    lastCutTime = Time.unscaledTime;
                }
            }
        }
        _intersectionPoints.Clear();
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
        isEnabled = true;
        polygon = cbm.ToArray();
    }
}



