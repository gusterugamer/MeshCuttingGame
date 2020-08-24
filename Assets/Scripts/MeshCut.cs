using UnityEngine;
using System.Collections.Generic;
using System;
using PrimitivesPro.MeshCutting;

public class MeshCut : MonoBehaviour
{
    //private  Plane _blade;
    private PrimitivesPro.Utils.Plane _blade;
    public Material capMaterial;

    private List<BoundaryPoint> _newRightBoundary;
    private List<BoundaryPoint> _newLeftBoundary;

    private int cutCount = 0;

    private void Start()
    {
        Application.targetFrameRate = 60;
        GetComponent<CustomBoundryBox>().CreateCustomBoundary();
    }

    public void StartCutting(Vector3 startPos, Vector3 endPos)
    {
        Cut(gameObject, capMaterial, startPos, endPos);
    }

    public void Cut(in GameObject victim, in Material capMaterial, Vector3 startPos, Vector3 endPos)
    {
        CustomBoundryBox _boundaryBox = victim.GetComponent<CustomBoundryBox>();
        List<IntersectionPoint> intersectionPoints = _boundaryBox.GetIntersections(startPos, endPos);

        if (intersectionPoints.Count == 2)
        {

            MeshCutterWithBoundary mc = new MeshCutterWithBoundary();

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
            mc.Cut(_victim_mesh, transform, _blade, false, new Vector4(0.0f, 0.0f, 1.0f, 1.0f), out Mesh _leftSideMesh, out Mesh _rightSideMesh, _boundaryBox.m_CustomBox);


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

            Mesh left_HalfMesh = _leftSideMesh;
            left_HalfMesh.name = "Split Mesh Left" + cutCount;
            leftSideObj.name = "left side" + cutCount;
            leftSideObj.GetComponent<MeshFilter>().mesh = left_HalfMesh;
            leftSideObj.GetComponent<MeshRenderer>().materials = mats;

            Mesh right_HalfMesh = _rightSideMesh;
            right_HalfMesh.name = "Split Mesh Right" + cutCount;
            rightSideObj.name = "right side" + cutCount;
            rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;
            rightSideObj.GetComponent<MeshRenderer>().materials = mats;

            cutCount++;

        }
    }
    private void CreateNewBoundary(in CustomBoundryBox _boundaryBox, in GameObject leftSideObj, in GameObject rightSideObj, ref List<IntersectionPoint> intersectionPoint)
    {
        //picking first and second intersection point indicies by looking who is closest to the start of the ppolygon
        int firstPointIndex = intersectionPoint[0]._nextBoundaryPoint < intersectionPoint[1]._nextBoundaryPoint ? 0 : 1;
        int secondPointIndex = 1 - firstPointIndex;

        Vector2 intersectPct = transform.TransformPoint(intersectionPoint[firstPointIndex]._pos);
        Vector2 intersectPct2 = transform.TransformPoint(intersectionPoint[secondPointIndex]._pos);

        Vector3 tangent = intersectPct2 - intersectPct;

        Vector3 depth = Camera.main.transform.forward;
        Vector3 normal = (Vector3.Cross(tangent, depth)).normalized;

        normal.z = 0.0f;

        _blade = new PrimitivesPro.Utils.Plane(normal, intersectPct);

        //leftSIde     
        CustomBoundryBox _leftSideBoundary = leftSideObj.GetComponent<CustomBoundryBox>();

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

        if (_blade.GetSide(_boundaryBox.m_CustomBox[intersectionPoint[firstPointIndex]._previousBoundaryPoint].m_pos))
        {
            rightSide.m_CustomBox = _newRightBoundary;
            _leftSideBoundary.m_CustomBox = _newLeftBoundary;
        }
        else
        {
            _leftSideBoundary.m_CustomBox = _newRightBoundary;
            rightSide.m_CustomBox = _newLeftBoundary;
        }

        _leftSideBoundary.UpdateCustomBoundary();
    }
}