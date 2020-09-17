using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using BlastProof;
using System;

public class Cutter
{
    private List<BoundaryPoint> _newRightBoundary;
    private List<BoundaryPoint> _newLeftBoundary;

    public List<BoundaryPoint> NewRightBoundary { get => _newRightBoundary; private set { _newRightBoundary = value; } }
    public List<BoundaryPoint> NewLeftBoundary { get => _newLeftBoundary; private set { _newLeftBoundary = value; } }

    public bool Cut(SpriteShapeController shape, Material capMaterial, Vector3 startPos, Vector3 endPos, List<GameObject> obstacles, out GameObject mask)
    {
        CustomBoundryBox _boundaryBox = shape.GetComponent<CustomBoundryBox>();
        List<IntersectionPoint> intersectionPoints = _boundaryBox.GetIntersections(startPos, endPos);       

        if (intersectionPoints.Count == 2)
        {
            bool distanceBeetWeenPoints = Vector3.Distance(intersectionPoints[0]._pos, intersectionPoints[1]._pos) > 0.001f;
            if (CreateNewBoundary(_boundaryBox, ref intersectionPoints, obstacles) && distanceBeetWeenPoints)
            {
                MeshProperties generatedMesh = MeshGenerator.CreateMesh(NewRightBoundary, shape.transform, 16.0f);

                Mesh newMesh = new Mesh();
                newMesh.name = "GenObjectMesh";
                newMesh.SetVertices(generatedMesh.mesh_vertices);
                newMesh.SetTriangles(generatedMesh.mesh_indicies, 0);
                newMesh.SetNormals(generatedMesh.mesh_normals);
                newMesh.SetUVs(0, generatedMesh.mesh_uvs);

                GameObject generatedObj = new GameObject();
                GameObject generatedObjParent = new GameObject();
                generatedObjParent.name = "GeneratedParent";
                generatedObjParent.transform.position = generatedMesh.mesh_center;
                generatedObj.transform.parent = generatedObjParent.transform;

                generatedObj.name = "right side";
                generatedObj.transform.position = Vector3.zero;
                generatedObj.AddComponent<MeshFilter>();
                generatedObj.AddComponent<MeshRenderer>();
                generatedObj.name = "Generated";
                generatedObj.GetComponent<MeshFilter>().mesh = newMesh;
                generatedObj.GetComponent<MeshRenderer>().material = Resources.Load("Material/SignMaterial") as Material;

                generatedObjParent.AddComponent<Rigidbody>().angularDrag = 0.0f;
                generatedObjParent.GetComponent<Rigidbody>().AddForce(new Vector3(0.0f, 1000.0f, -150.0f));
                generatedObjParent.GetComponent<Rigidbody>().AddTorque(new Vector3(-100.0f, 0.0f, 0.0f));
                generatedObjParent.GetComponent<Rigidbody>().mass = 100.0f;

                GameObject maskObj = new GameObject();
                maskObj.AddComponent<MeshFilter>();
                maskObj.AddComponent<MeshRenderer>();
                maskObj.transform.position = shape.GetComponent<Transform>().position + new Vector3(0.0f, 0.0f, -0.5f);
                maskObj.GetComponent<MeshFilter>().mesh = newMesh;
                maskObj.name = "mask";
                maskObj.GetComponent<MeshRenderer>().material = Resources.Load("Material/MaskMaterial") as Material;

                mask = maskObj;
                //testobj.AddComponent<Rigidbody>().AddForce(new Vector3(100.1f, 150f, 130f), ForceMode.Force);

                _boundaryBox.UpdateCustomBoundary(NewLeftBoundary);
                return true;
            }
            else
            {
                mask = null;
                return false;
            }
        }
        mask = null;
        return false;
    }

    private bool CreateNewBoundary(in CustomBoundryBox _boundaryBox, ref List<IntersectionPoint> intersectionPoint,List<GameObject> obstacles)
    {
        //picking first and second intersection point indicies by looking who is closest to the start of the ppolygon
        int firstPointIndex = intersectionPoint[0]._nextBoundaryPoint < intersectionPoint[1]._nextBoundaryPoint ? 0 : 1;
        int secondPointIndex = 1 - firstPointIndex;

        Plane plane = Mathematics.SlicePlane(intersectionPoint[firstPointIndex]._pos, intersectionPoint[secondPointIndex]._pos, Camera.main.transform.forward);

        int count;
        if (IsObjectsOnSameSide(plane, obstacles, out count))
        {

            NewLeftBoundary = new List<BoundaryPoint>();
            NewRightBoundary = new List<BoundaryPoint>();

            for (int i = 0; i < intersectionPoint[firstPointIndex]._nextBoundaryPoint; i++)
            {
                NewLeftBoundary.Add(_boundaryBox.m_CustomBox[i]);
            }

            NewLeftBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());
            NewLeftBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());

            for (int i = intersectionPoint[secondPointIndex]._nextBoundaryPoint; i < _boundaryBox.m_CustomBox.Count; i++)
            {
                NewLeftBoundary.Add(_boundaryBox.m_CustomBox[i]);
            }

            //rightside
            int intersectionPointDistance = intersectionPoint[secondPointIndex]._previousBoundaryPoint - intersectionPoint[firstPointIndex]._previousBoundaryPoint;

            NewRightBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());
            NewRightBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());

            for (int i = intersectionPoint[firstPointIndex]._nextBoundaryPoint; i < intersectionPoint[firstPointIndex]._nextBoundaryPoint + intersectionPointDistance; i++)
            {
                NewRightBoundary.Add(_boundaryBox.m_CustomBox[i]);
            }
            if (count < 0)
            {
                List<BoundaryPoint> tempB = NewLeftBoundary;
                NewLeftBoundary = NewRightBoundary;
                NewRightBoundary = tempB;
            }
            return true;
        }
        return false;
    }
    private bool IsObjectsOnSameSide(Plane plane, List<GameObject>obstacles, out int _count)
    {
        int count = 0;
        foreach (var go in obstacles)
        {
            count += plane.GetSide(go.transform.position) ? 1 : -1;
        }
        _count = count;
        return Math.Abs(count) == obstacles.Count;
    }
}