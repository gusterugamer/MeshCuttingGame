﻿using System.Collections.Generic;
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

    public GameObject objectToCheckIfInside;   

    private Transform trans;

    private PolygonCollider2D pol;

    //TEST ONLY
    private bool draw = false;

    public bool drawNew = false;
   
    int i = -1;

    //TEST ONLY

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
        {
            DrawCustomBoundary();           
        }
        if (drawNew)
        {
          //  MOVETHISFUCKERFORTEST();
            DrawNewCustomBoundary();          
            draw = false;          
        }
    }

    public void CreateCustomBoundary()
    {
        //Vector3 center = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.center;
        //Vector3 minPrime = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.min;
        //Vector3 maxPrime = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.max;
        //Vector3 min = center + minPrime;
        //Vector3 max = center + maxPrime;     

        //m_CustomBox.Add(new BoundaryPoint(min));       
        //m_CustomBox.Add(new BoundaryPoint(new Vector3(min.x, max.y, min.z)));
        //m_CustomBox.Add(new BoundaryPoint(new Vector3(max.x, max.y, min.z)));
        //m_CustomBox.Add(new BoundaryPoint(new Vector3(max.x, min.y, min.z)));

        pol = GetComponent<PolygonCollider2D>();

        Vector2[] polPoints = new Vector2[transform.childCount];
        int i = 0;
        foreach (Transform child in transform)
        {
            m_CustomBox.Add(new BoundaryPoint(child.localPosition));
            polPoints[i] = child.localPosition;
            i++;
        }
        pol.pathCount = 1;
        pol.points = polPoints;

        draw = true;
    }

    public void UpdateCustomBoundary()
    {
        //Vector3 center = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.center;
        //Vector3 minPrime = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.min;
        //Vector3 maxPrime = m_toCutObject.GetComponent<MeshFilter>().mesh.bounds.max;
        //Vector3 min = center + minPrime;
        //Vector3 max = center + maxPrime;     

        //m_CustomBox.Add(new BoundaryPoint(min));       
        //m_CustomBox.Add(new BoundaryPoint(new Vector3(min.x, max.y, min.z)));
        //m_CustomBox.Add(new BoundaryPoint(new Vector3(max.x, max.y, min.z)));
        //m_CustomBox.Add(new BoundaryPoint(new Vector3(max.x, min.y, min.z)));

        pol = GetComponent<PolygonCollider2D>();

        Vector2[] polPoints = new Vector2[m_CustomBox.Count];
        int i = 0;
        foreach (var pos in m_CustomBox)
        {
            polPoints[i] = pos.m_pos;
            i++;
        }

        draw = true;
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

    //TEST ONLY
    void DrawCustomBoundary()
    {
       
    }

    void DrawNewCustomBoundary()
    {
        int length = m_CustomBox.Count;
        for (int i=0;i<length;i++)
        {
            //Debug.DrawLine(transform.position + m_CustomBox[i].m_pos, transform.position + m_CustomBox[(i + 1) % length].m_pos, Color.red);        
            Debug.DrawLine(transform.position + m_CustomBox[i].m_pos, transform.position + m_CustomBox[(i + 1) % length].m_pos, Color.red);
        }
    }
    void MOVETHISFUCKERFORTEST()
    {
        if (objectToCheckIfInside)
        {
            objectToCheckIfInside.transform.position = Vector3.MoveTowards(objectToCheckIfInside.transform.position, transform.TransformPoint(m_CustomBox[(i + 1) % m_CustomBox.Count].m_pos), Time.deltaTime* 0.1f);
            if (objectToCheckIfInside.transform.position == transform.TransformPoint(m_CustomBox[(i + 1) % m_CustomBox.Count].m_pos))
                i++;
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
                pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x,intersPoint.y, -36.659f), i, (i + 1)));                
            }
        }      
        return pointsList;
    }  
}