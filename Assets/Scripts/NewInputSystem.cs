using BlastProof;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;
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

    private TrailRenderer trailrenderer;

    private Circle circle;     

    private Vector3 _startPos;
    private Vector3 _endPos;

    private float distanceFromCam;
    private float lastCutTime = 0.0f;

    private const float _CIRCLE_RADIUS = 1.25f;  
    private const float _COOLDOWN_TIME = 0.15f;
    private const float _PREDICTION_FACTOR = 4f;
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
       
        bool isOutside = Physics2D.Linecast(position, cbm.PolygonCenter, polygonLayer) || (Mathematics.PointInPolygon(position,polygon) ? false:true);
        var intersections = circle.GetIntersections(polygon);             

        if (Input.GetMouseButton(0) && isEnabled)
        {
            trailrenderer.forceRenderingOff = false;

            if (Time.unscaledTime > lastCutTime + _COOLDOWN_TIME)
            {
                if (isOutside)
                {
                    if (!hasStarted)
                    {
                        _startPos = position;
                        Debug.Log("OUT!");
                        hasStartedOutside = true;
                    }
                }
                if (!isOutside && hasStartedOutside)
                {                   
                    hasStarted = true;
                    hasEnded = false;
                    Debug.Log("IN!");
                }
                
                if (intersections.Count > 0)
                {
                    if (!isOutside)
                    {
                        if (!hasStarted)
                        {
                            Vector3 dirToIntersection = (intersections[0]._pos - transform.position).normalized;
                            _startPos = transform.position + _PREDICTION_FACTOR * dirToIntersection;
                            hasStarted = true;
                            hasEnded = false;
                        }
                    }
                }
                if (!hasEnded && hasStarted)
                {
                    //Ending point can be found only if you are inside the polygon to avoid inconsistent cutting (getting in the polygon ->generating starting point , 
                    //then getting out through the same edge which won't allow cutting, and then getting in through another edge)

                    _endPos = position;

                    //Checking if blade is intersecting an obstacle object                       
                    if (Physics2D.Linecast(_startPos, _endPos, obstacleLayer))
                    {
                        if (hasStarted)
                        {
                            LM.CollidedWithObject();
                            isEnabled = false;
                        }
                    }

                    //Pushing the point out of the polygon on the same cutting direction to avoid intersection problems
                    Vector3 cutDirection = (_endPos - _startPos).normalized;
                    _endPos = position + cutDirection * _PREDICTION_FACTOR;
                    if (cutter.Cut(victim, capMat, _startPos, _endPos, LM.Obstacles, out GameObject cuttedPiece))
                    {
                        LM.AddPieceToList(ref cuttedPiece);
                        polygon = cbm.ToArray();
                        hasEnded = true;
                        hasStarted = false;
                        hasStartedOutside = false;
                        LM.UpdateScore();
                        _startPos = cbm.PolygonCenter;
                        _endPos = cbm.PolygonCenter;
                        lastCutTime = Time.unscaledTime;
                    }
                }
            }
        }
        else
        {
            _startPos = cbm.PolygonCenter;
            _endPos = cbm.PolygonCenter;
            hasStarted = false;
            hasEnded = false;
            hasStartedOutside = false;
            trailrenderer.forceRenderingOff = true;
        }
    }

    public void ReEnable()
    {
        isEnabled = true;
        polygon = cbm.ToArray();
    }
}



