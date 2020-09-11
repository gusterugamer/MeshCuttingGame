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

    private PolygonCollider2D polyCol;

    private Vector3 polygonCenter;

    private float _area = 0.0f;

    public float Area
    {
        get { return _area; } private set { _area = value; }
    }

    public Vector3 PolygonCenter
    {
        get { return polygonCenter; }
    }

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

        polyCol = GetComponent<PolygonCollider2D>();
        int length = m_toCutObject.spline.GetPointCount();

        Vector2[] points = new Vector2[length];

        for (int i=0;i<length;i++)
        {
            points[i] = m_toCutObject.spline.GetPosition(i);
            pointsSum += points[i];
        }
        polygonCenter = pointsSum / length;
        polygonCenter.z = transform.position.z;

        polyCol.pathCount = 1;
        polyCol.points = points;

        foreach (Vector2 point in points)
        {
            m_CustomBox.Add(new BoundaryPoint(point));
        }
        GetArea();
    }

    public void UpdateCustomBoundary(List<BoundaryPoint> boundary)
    {
        m_CustomBox = boundary;

        ClearUnnecessaryPoints();
        UpdateCenter();
        GetArea();

        Vector2[] points = new Vector2[m_CustomBox.Count];
        for (int i = 0; i < m_CustomBox.Count; i++)
        {           
            points[i] = m_CustomBox[i].m_pos;
        }

        polyCol.pathCount = 1;
        polyCol.points = points;        
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

        for (int i=0;i<m_CustomBox.Count;i++)
        {
            Vector3 tempStartPos = trans.transform.InverseTransformPoint(startPoint);
            Vector3 tempEndPos = trans.transform.InverseTransformPoint(endPoint);

            BoundaryPoint currentBP = m_CustomBox[i];
            BoundaryPoint nextBP = m_CustomBox[(i + 1) % length];

            if (BlastProof.Mathematics.LineSegmentsIntersection(tempStartPos, tempEndPos, currentBP.m_pos, nextBP.m_pos, out Vector2 intersPoint))
            {                
                pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x,intersPoint.y, 0.0f), i, (i + 1)));                
            }
        }      
        return pointsList;
    }
    
    public Vector2[] ToArray()
    {
        Vector2[] arr = new Vector2[m_CustomBox.Count];
        for (int i=0;i<m_CustomBox.Count;i++)
        {
            arr[i] = m_CustomBox[i].m_pos;
        }
        return arr;
    }

    private void ClearUnnecessaryPoints()
    {
        List<BoundaryPoint> list = new List<BoundaryPoint>();
        BoundaryPoint lastAdded = BoundaryPoint.zero;
        for(int i=0;i<m_CustomBox.Count;i++)
        {
            if (lastAdded.m_pos != Vector3.zero)
            {
                int j = (i + 1) % m_CustomBox.Count;
                float distanceKJ = Vector3.Distance(lastAdded.m_pos, m_CustomBox[j].m_pos);
                float distanceKI = Vector3.Distance(lastAdded.m_pos, m_CustomBox[i].m_pos);
                float distanceIJ = Vector3.Distance(m_CustomBox[i].m_pos, m_CustomBox[j].m_pos);

                if (!Mathf.Approximately(distanceKI + distanceIJ, distanceKJ))
                {
                    list.Add(m_CustomBox[i]);
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

                if (!Mathf.Approximately(distanceKI + distanceIJ, distanceKJ))
                {
                    list.Add(m_CustomBox[i]);
                    lastAdded = m_CustomBox[i];
                }
            }          
        }
        m_CustomBox = list;        
    }    

    private void GetArea()
    {
        _area = Mathematics.PolygonArea(m_CustomBox);
    }
}
