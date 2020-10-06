using BlastProof;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public struct BoundaryPoint
{
    public Vector3 m_pos;

    public BoundaryPoint(Vector3 pos)
    {
        m_pos = pos;
    }
    public BoundaryPoint(float x, float y)
    {
        m_pos = new Vector3(x, y, 0.0f);
    }

    public bool isZero()
    {
        return m_pos == Vector3.zero;
    }

    public static BoundaryPoint zero => new BoundaryPoint(0.0f, 0.0f);
}

public class CustomBoundryBox : MonoBehaviour
{
    [SerializeField] private SpriteShapeController m_toCutObject;

    public List<BoundaryPoint> m_CustomBox = new List<BoundaryPoint>();

    private Transform trans;

    private EdgeCollider2D polyCol;

    private Vector3 polygonCenter;

    private float _area = 0.0f;

    private float _textureSize = 0.0f;

    //Used to calculate length and height
    private float maxY = -Mathf.Infinity;
    private float maxX = -Mathf.Infinity;

    private int currentCount = 0;
    private int oldCount = 0;

    private int removedPointsCount = 0;

    //Used to calculated UVs on generated objects 

    public float Area
    {
        get { return _area; }
        private set { _area = value; }
    }

    public Vector3 PolygonCenter
    {
        get { return polygonCenter; }
    }
    public float MaxY { get => maxY; }
    public float MaxX { get => maxX; }
    public int RemovedPointsCount { get => removedPointsCount; }

    public float ratio = 0.0f;

    [SerializeField] private Material levelMaterial;

    public bool drawNew = false;

    private void Awake()
    {
        CreateCustomBoundary();
    }

    // Start is called before the first frame update
    void Start()
    {
        trans = m_toCutObject.GetComponent<Transform>();
    }

    public void CreateCustomBoundary()
    {
        Vector2 pointsSum = Vector3.zero;

        polyCol = GetComponent<EdgeCollider2D>();
        int length = m_toCutObject.spline.GetPointCount();

        Vector2[] points = new Vector2[length + 1];

        for (int i = 0; i < length; i++)
        {
            points[i] = m_toCutObject.spline.GetPosition(i);
            pointsSum += points[i];
            m_CustomBox.Add(new BoundaryPoint(points[i]));
            GetMinMaxXY(points[i]);

            //GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //cube.transform.position = points[i];
            //cube.transform.name = i.ToString(); ;
        }
        points[length] = points[0];
        polygonCenter = pointsSum / length;
        polygonCenter.z = transform.position.z;

        //polyCol.pathCount = 1;
        polyCol.points = points;
        GetArea();

        ratio = _textureSize / Mathf.Max(MaxX, MaxY);

        int pixelPerUnit = Mathf.CeilToInt(ratio);
        m_toCutObject.fillPixelsPerUnit = pixelPerUnit;

        currentCount = m_CustomBox.Count;
        oldCount = m_CustomBox.Count;
    }

    public void UpdateCustomBoundary(List<BoundaryPoint> boundary)
    {
        m_CustomBox = boundary;

        ClearUnnecessaryPoints();
        UpdateCenter();
        GetArea();

        Vector2[] points = new Vector2[m_CustomBox.Count + 1];
        for (int i = 0; i <= m_CustomBox.Count; i++)
        {
            points[i] = m_CustomBox[i % m_CustomBox.Count].m_pos;
        }

        //polyCol.pathCount = 1;
        polyCol.points = points;

        oldCount = currentCount;
        currentCount = m_CustomBox.Count;

        removedPointsCount = oldCount - currentCount;
    }

    private void OnDrawGizmosSelected()
    {
        int length = m_CustomBox.Count;
        for (int i = 0; i < length; i++)
        {
            Debug.DrawLine((transform.position + m_CustomBox[i].m_pos), (transform.position + m_CustomBox[(i + 1) % length].m_pos), Color.red);
        }
    }

    private void UpdateCenter()
    {
        int length = m_CustomBox.Count;
        Vector3 pointsSum = Vector2.zero;
        for (int i = 0; i < length; i++)
        {
            pointsSum += m_CustomBox[i].m_pos;
        }
        polygonCenter = pointsSum / length;
        polygonCenter.z = transform.position.z;
    }

    public List<IntersectionPoint> GetIntersections(Vector3 startPoint, Vector3 endPoint)
    {
        List<IntersectionPoint> pointsList = new List<IntersectionPoint>();

        int length = m_CustomBox.Count;

        for (int i = 0; i < m_CustomBox.Count; i++)
        {
            Vector3 tempStartPos = trans.InverseTransformPoint(startPoint);
            Vector3 tempEndPos = trans.InverseTransformPoint(endPoint);

            BoundaryPoint currentBP = m_CustomBox[i];
            BoundaryPoint nextBP = m_CustomBox[(i + 1) % length];

            if (BlastProof.Mathematics.LineSegmentsIntersection(tempStartPos, tempEndPos, currentBP.m_pos, nextBP.m_pos, out Vector2 intersPoint))
            {
                if (Mathematics.IsVectorsAproximately(intersPoint, currentBP.m_pos))
                {
                    pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x, intersPoint.y, 0.0f), i, i));
                }
                else if (Mathematics.IsVectorsAproximately(intersPoint, nextBP.m_pos))
                {
                    pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x, intersPoint.y, 0.0f), i + 1, i + 1));
                }
                else
                {
                    pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x, intersPoint.y, 0.0f), i, i + 1));
                }
            }


        }
        return pointsList;
    }

    public Vector2[] ToArray()
    {
        Vector2[] arr = new Vector2[m_CustomBox.Count];
        for (int i = 0; i < m_CustomBox.Count; i++)
        {
            arr[i] = m_CustomBox[i].m_pos;
        }
        return arr;
    }

    private void ClearUnnecessaryPoints()
    {
        List<BoundaryPoint> cleanList = new List<BoundaryPoint>();
        BoundaryPoint lastAdded = BoundaryPoint.zero;

        for (int i = 0; i < m_CustomBox.Count; i++)
        {
            if (lastAdded.m_pos != Vector3.zero)
            {
                int j = (i + 1) % m_CustomBox.Count;

                float distanceKJ = Vector3.Distance(lastAdded.m_pos, m_CustomBox[j].m_pos);
                float distanceKI = Vector3.Distance(lastAdded.m_pos, m_CustomBox[i].m_pos);
                float distanceIJ = Vector3.Distance(m_CustomBox[i].m_pos, m_CustomBox[j].m_pos);

                if (!Mathf.Approximately(distanceKI + distanceKJ, distanceIJ))
                {
                    cleanList.Add(m_CustomBox[i]);
                    lastAdded = m_CustomBox[i];
                }
            }
            else
            {
                int j = (i + 1) % m_CustomBox.Count;
                int k = (int)Mathematics.nfmod((i - 1), m_CustomBox.Count);
                float distanceKJ = Vector3.Distance(m_CustomBox[k].m_pos, m_CustomBox[j].m_pos);
                float distanceKI = Vector3.Distance(m_CustomBox[k].m_pos, m_CustomBox[i].m_pos);
                float distanceIJ = Vector3.Distance(m_CustomBox[i].m_pos, m_CustomBox[j].m_pos);

                if (!Mathf.Approximately(distanceKI + distanceKJ, distanceIJ))
                {
                    cleanList.Add(m_CustomBox[i]);
                    lastAdded = m_CustomBox[i];
                }
            }
        }

        Debug.Log("UNNECESSARY POINTS: " + (m_CustomBox.Count - cleanList.Count).ToString());

        m_CustomBox = cleanList;


    }

    private void GetArea()
    {
        _area = Mathematics.PolygonArea(m_CustomBox);
    }

    private void GetMinMaxXY(Vector3 vec)
    {
        maxX = maxX < Mathf.Abs(vec.x) ? Mathf.Abs(vec.x) : maxX;
        maxY = maxY < Mathf.Abs(vec.y) ? Mathf.Abs(vec.y) : maxY;
    }

    public void ResetShape()
    {
        m_CustomBox.Clear();
        CreateCustomBoundary();
    }

    public void TextureSize(float size)
    {
        _textureSize = size;
    }
}
