using BlastProof;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public struct BoundaryPoint
{
    private Vector3 _pos;

    public Vector3 Pos { get => _pos; }

    public BoundaryPoint(Vector3 pos)
    {
        _pos = pos;
    }
    public BoundaryPoint(float x, float y)
    {
        _pos = new Vector3(x, y, 0.0f);
    }

    public bool isZero()
    {
        return _pos == Vector3.zero;        
    }

    public static BoundaryPoint zero => new BoundaryPoint(0.0f, 0.0f);

   
}

public class CustomBoundryBox : MonoBehaviour
{
    [SerializeField] private SpriteShapeController m_toCutObject;

    private List<BoundaryPoint> _CustomBox = new List<BoundaryPoint>();

    private Transform _trans;

    private EdgeCollider2D _polyCol;

    private Vector3 _polygonCenter;

    private float _area = 0.0f;

    private float _textureSize = 0.0f;

    private float _ratio = 0.0f;

    private float _maxY = -Mathf.Infinity;
    private float _maxX = -Mathf.Infinity;

    //Properties
    public List<BoundaryPoint> CustomBox { get => _CustomBox; }
    
    public float Area {get => _area; }    

    public Vector3 PolygonCenter { get => _polygonCenter; }
    
    public float MaxY { get => _maxY; }
    public float MaxX { get => _maxX; }    

    private void Awake()
    {
        CreateCustomBoundary();
    }

    // Start is called before the first frame update
    void Start()
    {
        _trans = m_toCutObject.GetComponent<Transform>();
    }

    public void CreateCustomBoundary()
    {
        Vector2 pointsSum = Vector3.zero;

        _polyCol = GetComponent<EdgeCollider2D>();
        int length = m_toCutObject.spline.GetPointCount();

        Vector2[] points = new Vector2[length + 1];

        for (int i = 0; i < length; i++)
        {
            points[i] = m_toCutObject.spline.GetPosition(i);
            pointsSum += points[i];
            _CustomBox.Add(new BoundaryPoint(points[i]));
            GetMinMaxXY(points[i]);       
        }
        points[length] = points[0];
        _polygonCenter = pointsSum / length;
        _polygonCenter.z = transform.position.z;

        //polyCol.pathCount = 1;
        _polyCol.points = points;
        GetArea();

        _ratio = _textureSize / Mathf.Max(MaxX, MaxY);

        int pixelPerUnit = Mathf.CeilToInt(_ratio);
        m_toCutObject.fillPixelsPerUnit = pixelPerUnit; 
    }

    public void UpdateCustomBoundary(List<BoundaryPoint> boundary)
    {
        _CustomBox = boundary;     
        UpdateCenter();
        GetArea();

        Vector2[] points = new Vector2[_CustomBox.Count + 1];
        for (int i = 0; i <= _CustomBox.Count; i++)
        {
            points[i] = _CustomBox[i % _CustomBox.Count].Pos;     
        }   
        _polyCol.points = points;   
    }

    private void UpdateCenter()
    {
        int length = _CustomBox.Count;
        Vector3 pointsSum = Vector2.zero;
        for (int i = 0; i < length; i++)
        {
            pointsSum += _CustomBox[i].Pos;
        }
        _polygonCenter = pointsSum / length;
        _polygonCenter.z = transform.position.z;
    }

    public List<IntersectionPoint> GetIntersections(Vector3 startPoint, Vector3 endPoint)
    {
        List<IntersectionPoint> pointsList = new List<IntersectionPoint>();

        int length = _CustomBox.Count;

        for (int i = 0; i < _CustomBox.Count; i++)
        {
            Vector3 tempStartPos = _trans.InverseTransformPoint(startPoint);
            Vector3 tempEndPos = _trans.InverseTransformPoint(endPoint);

            BoundaryPoint currentBP = _CustomBox[i];
            BoundaryPoint nextBP = _CustomBox[(i + 1) % length];

            if (BlastProof.Mathematics.LineSegmentsIntersection(tempStartPos, tempEndPos, currentBP.Pos, nextBP.Pos, out Vector2 intersPoint))
            {
                pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x, intersPoint.y, 0.0f), i, (i + 1)));
            }
        }
        return pointsList;
    }

    public Vector2[] ToArray()
    {
        Vector2[] arr = new Vector2[_CustomBox.Count];
        for (int i = 0; i < _CustomBox.Count; i++)
        {
            arr[i] = _CustomBox[i].Pos;
        }
        return arr;
    }   

    private void GetArea()
    {
        _area = Mathematics.PolygonArea(_CustomBox);
    }

    private void GetMinMaxXY(Vector3 vec)
    {
        _maxX = _maxX < Mathf.Abs(vec.x) ? Mathf.Abs(vec.x) : _maxX;
        _maxY = _maxY < Mathf.Abs(vec.y) ? Mathf.Abs(vec.y) : _maxY;
    }

    public void ResetShape()
    {
        _CustomBox.Clear();
        CreateCustomBoundary();
    }

    public void TextureSize(float size)
    {
        _textureSize = size;
    }
}
