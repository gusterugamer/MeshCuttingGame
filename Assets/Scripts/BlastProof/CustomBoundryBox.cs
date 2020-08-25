using PrimitivesPro;
using System.Collections.Generic;
using System.Linq;
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

    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        m_toCutObject = gameObject;
        trans = m_toCutObject.GetComponent<Transform>();
        polyCol = GetComponent<PolygonCollider2D>();

        ///TEST//

        CreateBoundaryA();
        //TEST//

    }

    private void CreateBoundaryA()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        Dictionary<KeyValuePair<int, int>, int> edgeRefCount = new Dictionary<KeyValuePair<int, int>, int>();
        HashSet<Vector2> vertPos; vertPos = new HashSet<Vector2>();

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            int vertPos1 = mesh.triangles[i];
            int vertPos2 = mesh.triangles[i + 1];
            int vertPos3 = mesh.triangles[i + 2];

            KeyValuePair<int, int> pair1 = new KeyValuePair<int, int>(vertPos1, vertPos2);
            KeyValuePair<int, int> pair2 = new KeyValuePair<int, int>(vertPos2, vertPos3);
            KeyValuePair<int, int> pair3 = new KeyValuePair<int, int>(vertPos3, vertPos1);

            KeyValuePair<int, int> pair1fliped = new KeyValuePair<int, int>(pair1.Value, pair1.Key);
            KeyValuePair<int, int> pair2fliped = new KeyValuePair<int, int>(pair2.Value, pair2.Key);
            KeyValuePair<int, int> pair3fliped = new KeyValuePair<int, int>(pair3.Value, pair3.Key);

            ////////////////////
            if (edgeRefCount.ContainsKey(pair1) || edgeRefCount.ContainsKey(pair1fliped))
            {
                if (edgeRefCount.ContainsKey(pair1))
                {
                    edgeRefCount[pair1]++;
                }
                else
                {
                    edgeRefCount[pair1fliped]++;
                }
            }
            else
            {
                edgeRefCount.Add(pair1, 1);
            }

            ///////////////////
            if (edgeRefCount.ContainsKey(pair2))
            {
                if (edgeRefCount.ContainsKey(pair2))
                {
                    edgeRefCount[pair2]++;
                }
                else
                {
                    edgeRefCount[pair2fliped]++;
                }
            }
            else
            {
                edgeRefCount.Add(pair2, 1);
            }

            //////////////
            if (edgeRefCount.ContainsKey(pair3) || edgeRefCount.ContainsKey(pair3fliped))
            {
                if (edgeRefCount.ContainsKey(pair3))
                {
                    edgeRefCount[pair3]++;
                }
                else
                {
                    edgeRefCount[pair3fliped]++;
                }
            }
            else
            {
                edgeRefCount.Add(pair3, 1);
            }
        }       

        foreach (var pair in edgeRefCount)
        {
            if (pair.Value == 1)
            {
                vertPos.Add(new Vector2(mesh.vertices[pair.Key.Value].x, mesh.vertices[pair.Key.Value].y));
                vertPos.Add(new Vector2(mesh.vertices[pair.Key.Key].x, mesh.vertices[pair.Key.Key].y));                
            }
        }

        List<Vector3> testList = new List<Vector3>();      

        foreach (var pos in vertPos)
        {
            testList.Add(pos);
        }   

        var polyList = JarvisMarchConvexHull.GetConvexHull(testList);
       
        //foreach (var pair in edgeRefCount)
        //{
        //    if (pair.Value == 1)
        //    {
        //        if (vertPos.Add(mesh.vertices[pair.Key.Value]))
        //        {
        //            edges.Add(pair.Key.Value);
        //        }
        //        if (vertPos.Add(mesh.vertices[pair.Key.Key]))
        //        {
        //            edges.Add(pair.Key.Key);
        //        }
        //    }
        //}

        foreach (var pos in polyList)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = transform;
            cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            cube.transform.position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
        }

        //foreach (var pair in edges)
        //{
        //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    cube.transform.parent = transform;
        //    cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //    cube.transform.position = transform.position + mesh.vertices[pair];     
        //}      

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
        //polyCol.pathCount = 1;
        //polyCol.points = coliderPoints;
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

        for (int i = 0; i < m_CustomBox.Count; i++)
        {
            Vector3 tempStartPos = trans.transform.InverseTransformPoint(startPoint);
            Vector3 tempEndPos = trans.transform.InverseTransformPoint(endPoint);

            BoundaryPoint currentBP = m_CustomBox[i];
            BoundaryPoint nextBP = m_CustomBox[(i + 1) % length];

            if (Mathematics.LineSegmentsIntersection(tempStartPos, tempEndPos, currentBP.m_pos, nextBP.m_pos, out Vector2 intersPoint))
            {
                pointsList.Add(new IntersectionPoint(new Vector3(intersPoint.x, intersPoint.y, -36.659f), i, (i + 1)));
            }
        }
        return pointsList;
    }

    private void OnBecameInvisible()
    {
        //Destroy(gameObject);
    }
}
