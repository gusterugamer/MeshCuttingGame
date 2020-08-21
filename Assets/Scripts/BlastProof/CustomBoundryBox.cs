using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private GameObject m_toCutObject;
    
    public List<BoundaryPoint> m_CustomBox = new List<BoundaryPoint>();    

    private Transform trans;

    private PolygonCollider2D polyCol;

    // Start is called before the first frame update
    void Start()
    {
        m_toCutObject = gameObject;
        trans = m_toCutObject.GetComponent<Transform>();
        polyCol = GetComponent<PolygonCollider2D>();
    }
   
    public void CreateCustomBoundary()
    {
        Vector2[] coliderPoints = new Vector2[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            m_CustomBox.Add(new BoundaryPoint(child.localPosition));
            coliderPoints[i] = child.localPosition;
            i++;
        }
        polyCol.pathCount = 1;
        polyCol.points = coliderPoints;
    }

    public void UpdateCustomBoundary()
    {
        Vector2[] coliderPoints = new Vector2[m_CustomBox.Count];
        int i = 0;
        foreach (var pct in m_CustomBox)
        {
            coliderPoints[i] = pct.m_pos;
            i++;
        }
        polyCol.pathCount = 1;
        polyCol.points = coliderPoints;
    }

    private void OnDrawGizmosSelected()
    {
        List<BoundaryPoint> m_CustomBox2 = new List<BoundaryPoint>();

        foreach (Transform child in transform)
        {
            m_CustomBox2.Add(new BoundaryPoint(child.localPosition));
        }

        int length = m_CustomBox2.Count;
        for (int i = 0; i < length; i++)
        {
            Debug.DrawLine((transform.position + m_CustomBox2[i].m_pos) * transform.localScale.x, (transform.position + m_CustomBox2[(i + 1) % length].m_pos) * transform.localScale.x, Color.red);
        }
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

            if (Mathematics.LineSegmentsIntersection(tempStartPos, tempEndPos, currentBP.m_pos, nextBP.m_pos, out Vector2 intersPoint))
            {                
                pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x,intersPoint.y, -36.659f), i, (i + 1)));                
            }
        }      
        return pointsList;
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
