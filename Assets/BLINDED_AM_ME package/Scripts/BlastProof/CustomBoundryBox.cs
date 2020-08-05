using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    public BoundaryPoint[] m_CustomBox;
    public BoundaryPoint[] m_newBoundaryPoints;

    public List<BoundaryPoint> newBoundary = new List<BoundaryPoint>();   

    private Transform trans;

    private bool draw = false;

    // Start is called before the first frame update
    void Start()
    {
        m_toCutObject = gameObject;
        trans = m_toCutObject.GetComponent<Transform>();      
    }

    // Update is called once per frame
    void Update()
    {
        if (draw)
        DrawCustomBoundary();
    }

    public void CreateCustomBoundary()
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

        draw = true;
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

    public List<IntersectionPoint> GetIntersections(Vector3 startPoint, Vector3 endPoint)
    {
        List<IntersectionPoint> pointsList = new List<IntersectionPoint>();

        int length = m_CustomBox.Length;

        for (int i=0;i<m_CustomBox.Length;i++)
        {
            Vector3 tempStartPos = trans.transform.InverseTransformPoint(startPoint);
            Vector3 tempEndPos = trans.transform.InverseTransformPoint(endPoint);

            BoundaryPoint currentBP = m_CustomBox[i];
            BoundaryPoint nextBP = m_CustomBox[(i + 1) % length];

            if (Math.LineSegmentsIntersection(tempStartPos, tempEndPos, currentBP.m_pos, nextBP.m_pos, out Vector2 intersPoint))
            {                
                pointsList.Add(new IntersectionPoint(intersPoint, i, (i + 1)));                
            }
        }      
        return pointsList;
    }  
}
