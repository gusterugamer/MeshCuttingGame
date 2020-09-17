using BlastProof;
using UnityEngine;
using UnityEngine.U2D;

public class NewInputSystem : MonoBehaviour
{
    private Camera _mainCam;

    [SerializeField] private CustomBoundryBox cbm;
    [SerializeField] private SpriteShapeController victim;
    [SerializeField] private Material capMat;
    [SerializeField] private LevelManager LM;

    public LayerMask layer;

    private Cutter cutter;
    private Vector2[] polygon;

    private TrailRenderer trailrenderer;

    private Circle circleBig;
    private Circle circleSmall;
    private Circle currentCircle;

    public delegate void CutDelegate();
    public delegate void ObjectCutDelegate();

    public event CutDelegate OnCutDone;
    public event ObjectCutDelegate OnObjectCut;    

    //public LayerMask layer;

    private Vector3 _startPos;
    private Vector3 _endPos;

    private float distanceFromCam;

    private bool hasStarted = false;
    private bool hasEnded = false;

    void Start()
    {
        trailrenderer = GetComponent<TrailRenderer>();
        trailrenderer.forceRenderingOff = true;

        circleBig = new Circle(transform.position, 1.0f);
        circleSmall = new Circle(transform.position, 0.2f);

        _mainCam = Camera.main;
        _startPos = cbm.PolygonCenter;
        _endPos = cbm.PolygonCenter;
        polygon = cbm.ToArray();

        cutter = new Cutter();
        distanceFromCam = Vector3.Distance(_mainCam.transform.position, cbm.PolygonCenter);
    }  

    private void Update()
    {
        MoveBlade();
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
            trailrenderer.forceRenderingOff = false;

            var intersections = currentCircle.GetIntersections(polygon);
            if (intersections.Count > 0)
            {
                //Check weather the cutting started from inside or from outside
                if (!Mathematics.PointInPolygon(position, polygon))
                {
                    if (!hasStarted)
                    {                       
                        _startPos = position;
                        hasStarted = true;
                        hasEnded = false;
                    }
                }
                //In case it starts inside the polygon the starting point has to be pushed outside the polygon to avoid intersection problems (cutting line - polygon)
                else if (Mathematics.PointInPolygon(position, polygon))
                {
                    if (!hasStarted)
                    {                        
                        Vector3 dirToIntersection = (intersections[0]._pos - transform.position).normalized;
                        _startPos = transform.position + 2 * currentCircle.Radius * dirToIntersection;
                        hasStarted = true;
                        hasEnded = false;
                    }
                }
            }
            if (!hasEnded)
            {
                //Ending point can be found only if you are inside the polygon to avoid inconsistent cutting (getting in the polygon ->generating starting point , 
                //then getting out through the same edge which won't allow cutting, and then getting in through another edge)
                if (Mathematics.IsPointInPolygon(position, polygon))
                {
                    _endPos = position;

                    //Checking if blade is intersecting an obstacle object
                    RaycastHit2D hit;
                    if (hit = Physics2D.Linecast(_startPos, _endPos,layer))
                    {                       
                        if (hasStarted)
                        {
                            OnObjectCut?.Invoke();
                            //polygon = cbm.ToArray();
                        }
                    }

                    //Pushing the point out of the polygon on the same cutting direction to avoid intersection problems
                    Vector3 cutDirection = (_endPos - _startPos).normalized;
                    _endPos = position + cutDirection * 2 * currentCircle.Radius;
                    if (cutter.Cut(victim, capMat, _startPos, _endPos, LM.Obstacles, out GameObject cuttedPiece))
                    {
                        LM.AddPieceToList(ref cuttedPiece);
                        polygon = cbm.ToArray();                      
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
            trailrenderer.forceRenderingOff = true;
        }
    }    
}



