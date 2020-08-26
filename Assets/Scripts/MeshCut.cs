using UnityEngine;
using System.Collections.Generic;
using System;
using PrimitivesPro.MeshCutting;
using System.Linq;

public class MeshCut : MonoBehaviour
{
    //private  Plane _blade;
    private PrimitivesPro.Utils.Plane _blade;
    public Material capMaterial;

    private List<BoundaryPoint> _newRightBoundary;
    private List<BoundaryPoint> _newLeftBoundary;

    private Vector3 startPos;
    private Vector3 endPos;

    private Mesh mesh;

    int intersect = 0;

    private Camera mainCam;

    private void Start()
    {
        Application.targetFrameRate = 60;
        mainCam = Camera.main;
        GetComponent<CustomBoundryBox>().CreateCustomBoundary();
        //CreateBoundaryA();
    }

    public void StartCutting(Vector3 startPos, Vector3 endPos)
    {
        Cut(gameObject, capMaterial, startPos, endPos);
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

        //var polyList = JarvisMarchConvexHull.GetConvexHull(testList);

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

        //foreach (var pos in polyList)
        //{
        //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //    cube.transform.parent = transform;
        //    cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        //    cube.transform.position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
        //}

        foreach (var pair in vertPos)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.parent = transform;
            cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            cube.transform.position = transform.position + new Vector3(pair.x, pair.y, 0.0f);     
        }      

    }

    public void Cut(in GameObject victim, in Material capMaterial, Vector3 startPos, Vector3 endPos)
    {
        CustomBoundryBox _boundaryBox = victim.GetComponent<CustomBoundryBox>();
        List<IntersectionPoint> intersectionPoints = _boundaryBox.GetIntersections(startPos, endPos);

        if (intersectionPoints.Count == 2)
        {
            MeshCutterWithBoundary mc = new MeshCutterWithBoundary();

            Vector3 tangent = (endPos - startPos);
            Vector3 depth = Camera.main.transform.forward;
            Vector3 normal = (Vector3.Cross(tangent, depth)).normalized;

            normal.z = 0.0f;

            _blade = new PrimitivesPro.Utils.Plane(normal, startPos);

            //CHACHE

            // get the victims mesh
            Mesh _victim_mesh = victim.GetComponent<MeshFilter>().mesh;

            //New objects creation          

            GameObject leftSideObj = victim;

            GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer));
            rightSideObj.transform.position = victim.transform.position;
            rightSideObj.transform.rotation = victim.transform.rotation;
            rightSideObj.transform.localScale = victim.transform.localScale;
            //BOUNDARY CUT      
            CreateNewBoundary(victim.GetComponent<CustomBoundryBox>(), leftSideObj, rightSideObj, ref intersectionPoints);

            //MeshCut
            mc.Cut(_victim_mesh, transform, _blade, true, new Vector4(0.0f, 0.0f, 1.0f, 1.0f), out Mesh _leftSideMesh, out Mesh _rightSideMesh, _boundaryBox.m_CustomBox);


            // The capping Material will be at the end
            Material[] mats = victim.GetComponent<MeshRenderer>().sharedMaterials;
            if (mats[mats.Length - 1].name != capMaterial.name)
            {
                Material[] newMats = new Material[mats.Length + 1];
                mats.CopyTo(newMats, 0);
                newMats[mats.Length] = capMaterial;
                mats = newMats;
            }
            // Left Mesh

            _leftSideMesh.name = "Split Mesh Left";
            leftSideObj.name = "left side";
            leftSideObj.GetComponent<MeshFilter>().mesh = _leftSideMesh;
            leftSideObj.GetComponent<MeshRenderer>().materials = mats;

            _rightSideMesh.name = "Split Mesh Right";
            rightSideObj.name = "right side";
            rightSideObj.GetComponent<MeshFilter>().mesh = _rightSideMesh;
            rightSideObj.GetComponent<MeshRenderer>().materials = mats;
            Destroy(rightSideObj, 1.0f);

        }
    }

    private void CreateNewBoundary(in CustomBoundryBox _boundaryBox, in GameObject leftSideObj, in GameObject rightSideObj, ref List<IntersectionPoint> intersectionPoint)
    {

        //picking first and second intersection point indicies by looking who is closest to the start of the ppolygon
        int firstPointIndex = intersectionPoint[0]._nextBoundaryPoint < intersectionPoint[1]._nextBoundaryPoint ? 0 : 1;
        int secondPointIndex = 1 - firstPointIndex;

        //leftSIde     
        CustomBoundryBox _leftSideBoundary = leftSideObj.GetComponent<CustomBoundryBox>();

        //Correcting intersection points so they match perfectly the cutting plane
        // intersectionPoint[firstPointIndex] = CorrectIntersections(_boundaryBox.m_CustomBox, intersectionPoint[firstPointIndex]);
        // intersectionPoint[secondPointIndex] = CorrectIntersections(_boundaryBox.m_CustomBox, intersectionPoint[secondPointIndex]);

        _newLeftBoundary = new List<BoundaryPoint>();
        _newRightBoundary = new List<BoundaryPoint>();

        for (int i = 0; i < intersectionPoint[firstPointIndex]._nextBoundaryPoint; i++)
        {
            _newLeftBoundary.Add(_boundaryBox.m_CustomBox[i]);
        }

        _newLeftBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());
        _newLeftBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());

        for (int i = intersectionPoint[secondPointIndex]._nextBoundaryPoint; i < _boundaryBox.m_CustomBox.Count; i++)
        {
            _newLeftBoundary.Add(_boundaryBox.m_CustomBox[i]);
        }

        //rightside
        int intersectionPointDistance = intersectionPoint[secondPointIndex]._previousBoundaryPoint - intersectionPoint[firstPointIndex]._previousBoundaryPoint;

        rightSideObj.AddComponent<CustomBoundryBox>();
        rightSideObj.AddComponent<Rigidbody>();
        CustomBoundryBox rightSide = rightSideObj.GetComponent<CustomBoundryBox>();

        _newRightBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());

        for (int i = intersectionPoint[firstPointIndex]._nextBoundaryPoint; i < intersectionPoint[firstPointIndex]._nextBoundaryPoint + intersectionPointDistance; i++)
        {
            _newRightBoundary.Add(_boundaryBox.m_CustomBox[i]);
        }
        _newRightBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());

        rightSide.m_CustomBox = _newRightBoundary;
        _leftSideBoundary.m_CustomBox = _newLeftBoundary;

        //if (_blade.GetSide(_boundaryBox.m_CustomBox[intersectionPoint[firstPointIndex]._previousBoundaryPoint].m_pos))
        //{
        //    rightSide.m_CustomBox = _newRightBoundary;
        //    _leftSideBoundary.m_CustomBox = _newLeftBoundary;
        //}
        //else
        //{
        //    _leftSideBoundary.m_CustomBox = _newRightBoundary;
        //    rightSide.m_CustomBox = _newLeftBoundary;
        //}    

        _leftSideBoundary.UpdateCustomBoundary();
    }

}