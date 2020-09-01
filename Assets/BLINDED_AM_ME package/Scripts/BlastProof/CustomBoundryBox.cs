using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

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

    [SerializeField] private Material levelMaterial;

    public bool drawNew = false; 

    // Start is called before the first frame update
    void Start()
    {
        CreateCustomBoundary();
       trans = m_toCutObject.GetComponent<Transform>();       
    }

    public void CreateCustomBoundary()
    {
        int length = m_toCutObject.spline.GetPointCount();

        Vector2[] points = new Vector2[length];

        for (int i=0;i<length;i++)
        {
            points[i] = m_toCutObject.spline.GetPosition(i);
        }
        
        foreach (Vector2 point in points)
        {
            m_CustomBox.Add(new BoundaryPoint(point));
        }           
    }

    public void UpdateCustomBoundary(List<BoundaryPoint> boundary)
    {
        m_CustomBox = boundary;
        m_toCutObject.spline.Clear();
        for (int i = 0; i < boundary.Count; i++)
        {
            m_toCutObject.spline.InsertPointAt(i, boundary[i].m_pos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        int length = m_CustomBox.Count;
        for (int i = 0; i < length; i++)
        {
            Debug.DrawLine((transform.position + m_CustomBox[i].m_pos), (transform.position + m_CustomBox[(i + 1) % length].m_pos), Color.red);
        }
    }  

    //TEST ONLY
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
                pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x,intersPoint.y, 0.0f), i, (i + 1)));                
            }
        }      
        return pointsList;
    }  
}
