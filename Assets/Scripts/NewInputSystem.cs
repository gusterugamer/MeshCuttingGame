using BlastProof;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class NewInputSystem : MonoBehaviour
{
    public LayerMask obstacleLayer;

    private Camera _mainCam;

    private CustomBoundryBox _cb;
    [SerializeField] private SpriteShapeController _shape;
    [SerializeField] private Material _capMat;
    [SerializeField] private LevelManager _LM;   

    private Cutter _cutter;   

    private Material _textureMat;

    private TrailRenderer _trailrenderer;

    private Circle _circle;

    private Vector3 _startPos;
    private Vector3 _endPos;
    private Vector3 _currentPos;
    private Vector2[] _polygon;

    private List<IntersectionPoint> _intersectionPoints = new List<IntersectionPoint>();
    private IntersectionPoint _lastIntersectionPoint = IntersectionPoint.zero;    

    private float _distanceFromCam;

    private const float _CIRCLE_RADIUS = 1.25f;
    
    private bool _isEnabled = true;

    void Start()
    {
        _cb = _shape.GetComponent<CustomBoundryBox>();

        _trailrenderer = GetComponent<TrailRenderer>();
        _trailrenderer.forceRenderingOff = true;

        _circle = new Circle(transform.position, _CIRCLE_RADIUS);

        _mainCam = Camera.main;
        _polygon = _cb.ToArray();
        _startPos = _cb.PolygonCenter;
        _endPos = _cb.PolygonCenter;

        _cutter = new Cutter();
        _distanceFromCam = Mathf.Abs(_mainCam.transform.position.z - _shape.transform.position.z);

    }

    private void FixedUpdate()
    {
        MoveBlade();
    }
    
    private void MoveBlade()
    {

        Vector3 position = Input.mousePosition;
        position.z = _distanceFromCam - 0.5f;
        position = _mainCam.ScreenToWorldPoint(position);
        transform.position = position;

        _circle.UpdatePosition(position);

        bool isOutside = (Mathematics.PointInPolygon(position, _polygon) ? false : true);
        var intersections = _circle.GetIntersections(_polygon);

        if (_lastIntersectionPoint != IntersectionPoint.zero &&
            Physics2D.Linecast(_lastIntersectionPoint.Pos, position, obstacleLayer) && !isOutside
            && position != Vector3.zero)
        {
            _LM.CollidedWithObject();
            _isEnabled = false;
        }
        _trailrenderer.forceRenderingOff = false;

        Touch[] touch = Input.touches;

        if (_isEnabled && Input.GetMouseButton(0) && touch.Length < 2)
        {
            _startPos = _endPos;
            _endPos = position;
            _currentPos = position;

            if (_startPos != _cb.PolygonCenter && _endPos != _cb.PolygonCenter)
            {
                NewIntersections(_startPos, _endPos);
                Cut();
            }

            if (isOutside)
            {
                _lastIntersectionPoint = IntersectionPoint.zero;
            }
        }
        else
        {
            _startPos = _cb.PolygonCenter;
            _endPos = _cb.PolygonCenter;           
            _lastIntersectionPoint = IntersectionPoint.zero;
            _trailrenderer.forceRenderingOff = true;
            _trailrenderer.Clear();
        }
    }   

    private void CorrectLastIntersectionPoint()
    {
        for (int i = 1; i <= _cb.CustomBox.Count; i++)
        {
            float distanceKJ = Vector3.Distance(_lastIntersectionPoint.Pos, _cb.CustomBox[i - 1].Pos);
            float distanceKI = Vector3.Distance(_lastIntersectionPoint.Pos, _cb.CustomBox[i % _cb.CustomBox.Count].Pos);
            float distanceIJ = Vector3.Distance(_cb.CustomBox[i - 1].Pos, _cb.CustomBox[i % _cb.CustomBox.Count].Pos);            

            if (Mathf.Approximately(distanceKI + distanceKJ, distanceIJ))
            {
                _lastIntersectionPoint.ChangePrevNextPoints(i - 1, i);              
            }
        }
    }

    private void CorrectIntersectionPointsLeft(int index)
    {
        for (; index < _intersectionPoints.Count; index++)
        {
            for (int i = 1; i <= _cb.CustomBox.Count; i++)
            {
                float distanceKJ = Vector3.Distance(_intersectionPoints[index].Pos, _cb.CustomBox[i - 1].Pos);
                float distanceKI = Vector3.Distance(_intersectionPoints[index].Pos, _cb.CustomBox[i % _cb.CustomBox.Count].Pos);
                float distanceIJ = Vector3.Distance(_cb.CustomBox[i - 1].Pos, _cb.CustomBox[i % _cb.CustomBox.Count].Pos);               

                if (Mathematics.IsVectorsAproximately(_intersectionPoints[index].Pos, _cb.CustomBox[i - 1].Pos))
                {
                    _intersectionPoints[index].ChangePrevNextPoints(i - 1, i - 1);                  
                }
                else if (Mathematics.IsVectorsAproximately(_intersectionPoints[index].Pos, _cb.CustomBox[i % _cb.CustomBox.Count].Pos))
                {
                    _intersectionPoints[index].ChangePrevNextPoints(i, i);                  
                }
                else if (Mathf.Approximately(distanceKI + distanceKJ, distanceIJ))
                {
                    _intersectionPoints[index].ChangePrevNextPoints(i - 1, i);                 
                }
            }
        }
    }

    private void NewIntersections(Vector3 startPos, Vector3 endPos)
    {
        _intersectionPoints.Clear();
        List<IntersectionPoint> ip = _cb.GetIntersections(startPos, endPos);

        List<IntersectionPoint> ipWithLastPoint;

        if (_lastIntersectionPoint != IntersectionPoint.zero)
        {
            _intersectionPoints.Add(_lastIntersectionPoint);
            ipWithLastPoint = _cb.GetIntersections(_lastIntersectionPoint.Pos, _currentPos);

            if (ipWithLastPoint.Count > 0)
            {
                for (int i = 0; i < ipWithLastPoint.Count; i++)
                {
                    IntersectionPoint tempPoint = ipWithLastPoint[i];
                    int count = 0;

                    for (int j = 0; j < ip.Count; j++)
                    {
                        if (tempPoint.PreviousBoundaryPoint != ip[j].PreviousBoundaryPoint ||
                            tempPoint.NextBoundaryPoint != ip[j].NextBoundaryPoint)
                        {
                            count++;
                        }
                    }

                    if (tempPoint != IntersectionPoint.zero &&
                        (tempPoint.PreviousBoundaryPoint != _lastIntersectionPoint.PreviousBoundaryPoint &&
                        tempPoint.NextBoundaryPoint != _lastIntersectionPoint.NextBoundaryPoint) &&
                        count == ip.Count &&
                        !Mathematics.IsVectorsAproximately(tempPoint.Pos, _intersectionPoints[_intersectionPoints.Count-1].Pos)
                        )
                    {
                        _intersectionPoints.Add(tempPoint);
                    }
                }
            }
        }

        for (int i = 0; i < ip.Count; i++)
        {
            if (_intersectionPoints.Count > 0)
            {
                if (!Mathematics.IsVectorsAproximately(ip[i].Pos, _intersectionPoints[_intersectionPoints.Count - 1].Pos))
                {
                    _intersectionPoints.Add(ip[i]);
                }
            }
            else if (_intersectionPoints.Count <= 0)
            {
                _intersectionPoints.Add(ip[i]);
            }
        }

        if (_intersectionPoints.Count > 0)
        {  
            _lastIntersectionPoint = _intersectionPoints[_intersectionPoints.Count - 1];  
            
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
            if ((_intersectionPoints[i - 1].PreviousBoundaryPoint != _intersectionPoints[i].PreviousBoundaryPoint ||
                _intersectionPoints[i - 1].NextBoundaryPoint != _intersectionPoints[i].NextBoundaryPoint) &&
                (!Mathematics.IsVectorsAproximately(_intersectionPoints[i - 1].Pos, _intersectionPoints[i].Pos)))
            {              

                if (isIntersectionLineInPolygon(_intersectionPoints[i - 1], _intersectionPoints[i]))
                {
                    List<IntersectionPoint> tempList = new List<IntersectionPoint>();
                    tempList.Add(_intersectionPoints[i - 1]);
                    tempList.Add(_intersectionPoints[i]);                

                    bool cutSucceded = _cutter.Cut(_shape, _textureMat, tempList, _LM.Obstacles, out GameObject cuttedPiece);

                    if (cutSucceded)
                    {
                        _polygon = _cb.ToArray();
                        _LM.AddPieceToList(ref cuttedPiece);
                        _LM.UpdateScore();

                        if (_lastIntersectionPoint != IntersectionPoint.zero)
                        {
                            CorrectLastIntersectionPoint();                            
                        }
                        CorrectIntersectionPointsLeft(i + 1);                     
                    }
                }
            }
        }
    }

    private bool isIntersectionLineInPolygon(IntersectionPoint tempPoint1, IntersectionPoint tempPoint2)
    {
        Vector3 point1 = tempPoint1.Pos;
        Vector3 point2 = tempPoint2.Pos;

        Vector3 center = (point1 + point2) / 2f;

        Matrix4x4 scaleMatrix = Mathematics.ScaleMatrix(0.5f);

        point1 = scaleMatrix.MultiplyPoint(point1);
        point2 = scaleMatrix.MultiplyPoint(point2);

        Vector3 scaledCenter = (point1 + point2) / 2f;

        Matrix4x4 transMatrix = Mathematics.TranslateMatrix(center - scaledCenter);

        point1 = transMatrix.MultiplyPoint(point1);
        point2 = transMatrix.MultiplyPoint(point2);

        int edgesHit = Physics2D.LinecastAll(point1, point2).Length;
        RaycastHit2D[] edgesHitUnscaled = Physics2D.LinecastAll(tempPoint1.Pos, tempPoint2.Pos);  

        for(int i=0;i<edgesHitUnscaled.Length;i++)
        {
            Vector2 hittedPoint = edgesHitUnscaled[i].point;
            if (!Mathematics.IsVectorsAproximately(tempPoint1.Pos, hittedPoint) && !Mathematics.IsVectorsAproximately(tempPoint2.Pos, hittedPoint))
            {
                edgesHit++;
            }
        }

        if (Mathematics.PointInPolygon(point1, _polygon) && Mathematics.PointInPolygon(point2, _polygon) && edgesHit == 0)
        {
            return true;
        }

        return false;
    }

    public void UpdateMats(Material textureMat)
    {
        _textureMat = textureMat;
    }  

    public void ReEnable()
    {      
        _startPos = _cb.PolygonCenter;
        _endPos = _cb.PolygonCenter;    
        _trailrenderer.forceRenderingOff = true;
        _intersectionPoints.Clear();
        _lastIntersectionPoint = IntersectionPoint.zero;
        _isEnabled = true;
        _polygon = _cb.ToArray();
    }
}



