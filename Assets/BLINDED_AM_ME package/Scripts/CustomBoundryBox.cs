using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomBoundryBox : MonoBehaviour
{
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

        public static BoundaryPoint zero => new BoundaryPoint(0.0f,0.0f);       
    }

    [SerializeField] private GameObject m_toCutObject;
    public BoundaryPoint[] m_CustomBox;
    public BoundaryPoint[] m_newBoundaryPoints; 


    // Start is called before the first frame update
    void Start()
    {
        CreateCustomBoundary();
    }

    // Update is called once per frame
    void Update()
    {
        DrawCustomBoundary();
    }

    void CreateCustomBoundary()
    {
        Vector3 center = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.center;
        Vector3 minPrime = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.min;
        Vector3 maxPrime = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.max;
        Vector3 min = center + minPrime;
        Vector3 max = center + maxPrime;

        m_CustomBox = new BoundaryPoint[4];

        m_CustomBox[0] = new BoundaryPoint(min);       
        m_CustomBox[1] = new BoundaryPoint(new Vector3(max.x, min.y, min.z));
        m_CustomBox[2] = new BoundaryPoint(new Vector3(max.x, max.y, min.z));
        m_CustomBox[3] = new BoundaryPoint(new Vector3(min.x, max.y, min.z));
    }

    void DrawCustomBoundary()
    {
        int length = m_CustomBox.Length;
        for (int i= 0; i<length;i++)
        {
            if (i + 3 < length)
            {
                Debug.DrawLine(transform.position + m_CustomBox[i].m_pos, transform.position + m_CustomBox[(i + 3)].m_pos, Color.red);
            }
            if (i + 1 < length)
            {
                Debug.DrawLine(transform.position + m_CustomBox[i].m_pos, transform.position + m_CustomBox[(i + 1)].m_pos, Color.red);
            }
            if (i + 4 < length)
            {
                Debug.DrawLine(transform.position + m_CustomBox[i].m_pos, transform.position + m_CustomBox[(i + 4)].m_pos, Color.red);
            }
        }
    }

    public List<BoundaryPoint> GetIntersections(Vector3 startPoint, Vector3 endPoint, Transform victim)
    {
        BoundaryPoint tempIntersectionPoint = BoundaryPoint.zero;
        List<BoundaryPoint> pointsList = new List<BoundaryPoint>();
        int length = m_CustomBox.Length;
        for (int i=0;i<m_CustomBox.Length;i++)
        {
            //tempIntersectionPoint = Math.getLineLineIntersection(startPoint, endPoint,
            //                                                     m_CustomBox[i], m_CustomBox[(i + 1) % length], victim);
            Vector2 inters;
            bool sax = Math.LineSegmentsIntersection(startPoint, endPoint, m_CustomBox[i].m_pos, m_CustomBox[(i + 1) % length].m_pos,out inters);
            tempIntersectionPoint.m_pos = inters;

            pointsList.Add(tempIntersectionPoint);
        }       
        return pointsList;
    }
}
