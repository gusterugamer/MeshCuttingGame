using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using BlastProof;
using System;
using UnityEngine.Rendering;

public class Cutter
{
    private List<BoundaryPoint> _newRightBoundary;
    private List<BoundaryPoint> _newLeftBoundary;

    private static Material _maskMaterial = Resources.Load("Material/MaskMaterial") as Material;

    public List<BoundaryPoint> NewRightBoundary { get => _newRightBoundary; }
    public List<BoundaryPoint> NewLeftBoundary { get => _newLeftBoundary; }
    public bool Cut(SpriteShapeController shape, Material textureMat, List<IntersectionPoint> intersectionPoints, List<GameObject> obstacles, out GameObject mask)
    {
        CustomBoundryBox _boundaryBox = shape.GetComponent<CustomBoundryBox>();

        //Decides which value is bigger to create the size of texture square
        float spriteSquareSize = Mathf.Max(_boundaryBox.MaxX, _boundaryBox.MaxY);

        if (intersectionPoints.Count == 2)
        {
            bool distanceBeetWeenPoints = Vector3.Distance(intersectionPoints[0].Pos, intersectionPoints[1].Pos) > 0.5f;
            if (CreateNewBoundary(_boundaryBox, intersectionPoints, obstacles) && distanceBeetWeenPoints)
            {
                //Generates a 3d mesh out of cutted polygon (generatedMesh) and uses it's frontface as mask (maskMesh)
                MeshProperties[] newMeshes = MeshGenerator.CreateMesh(_newRightBoundary, shape.transform, spriteSquareSize);
                MeshProperties generatedMesh = newMeshes[0];
                MeshProperties maskMesh = newMeshes[1];

                Mesh newMaskMesh = new Mesh();
                newMaskMesh.name = "GenMaskMesh";
                newMaskMesh.SetVertices(maskMesh.mesh_vertices);
                newMaskMesh.SetTriangles(maskMesh.mesh_indicies, 0);


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
                generatedObj.GetComponent<MeshRenderer>().material = textureMat;

                generatedObjParent.AddComponent<Rigidbody>().angularDrag = 0.0f;
                generatedObjParent.GetComponent<Rigidbody>().AddForce(new Vector3(0.0f, 1000.0f, -150.0f));
                generatedObjParent.GetComponent<Rigidbody>().AddTorque(new Vector3(-100.0f, 0.0f, 0.0f));
                generatedObjParent.GetComponent<Rigidbody>().mass = 100.0f;
                generatedObjParent.AddComponent<DestroyMyself>();

                GameObject maskObj = new GameObject();
                maskObj.AddComponent<MeshFilter>();
                maskObj.AddComponent<MeshRenderer>();
                maskObj.transform.position = shape.GetComponent<Transform>().position + new Vector3(0.0f, 0.0f, -0.001f);
                maskObj.GetComponent<MeshFilter>().mesh = newMaskMesh;
                maskObj.name = "mask";
                maskObj.GetComponent<MeshRenderer>().material = _maskMaterial;
                //testobj.AddComponent<Rigidbody>().AddForce(new Vector3(100.1f, 150f, 130f), ForceMode.Force);                

                mask = maskObj;
                _boundaryBox.UpdateCustomBoundary(_newLeftBoundary);
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

    private bool CreateNewBoundary(in CustomBoundryBox _boundaryBox, List<IntersectionPoint> intersectionPoint, List<GameObject> obstacles)
    {
        //picking first and second intersection point indicies by looking who is closest to the start of the ppolygon
        int firstPointIndex = intersectionPoint[0].NextBoundaryPoint < intersectionPoint[1].NextBoundaryPoint ? 0 : 1;
        int secondPointIndex = 1 - firstPointIndex;

        // Plane plane = Mathematics.SlicePlane(intersectionPoint[firstPointIndex].Pos, intersectionPoint[secondPointIndex].Pos, Camera.main.transform.forward);

        int count;

        _newLeftBoundary = new List<BoundaryPoint>();
        _newRightBoundary = new List<BoundaryPoint>();

        for (int i = 0; i < intersectionPoint[firstPointIndex].NextBoundaryPoint; i++)
        {
            _newLeftBoundary.Add(_boundaryBox.CustomBox[i]);
        }

        if (!Mathematics.IsVectorsAproximately(_newLeftBoundary[_newLeftBoundary.Count - 1].Pos, intersectionPoint[firstPointIndex].Pos))
        {
            _newLeftBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());
        }

        if (!Mathematics.IsVectorsAproximately(_newLeftBoundary[_newLeftBoundary.Count - 1].Pos, intersectionPoint[secondPointIndex].Pos))
        {
            _newLeftBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());
        }

        for (int i = intersectionPoint[secondPointIndex].NextBoundaryPoint; i < _boundaryBox.CustomBox.Count; i++)
        {
            if (!Mathematics.IsVectorsAproximately(_newLeftBoundary[_newLeftBoundary.Count - 1].Pos, _boundaryBox.CustomBox[i].Pos))
            {
                _newLeftBoundary.Add(_boundaryBox.CustomBox[i]);
            }
        }

        int dup = 0;
        for (int i = 1; i < _newLeftBoundary.Count; i++)
        {
            if (Mathematics.IsVectorsAproximately(_newLeftBoundary[i - 1].Pos, _newLeftBoundary[i].Pos))
            {
                dup++;
            }
        }

        //rightside       

        _newRightBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());
        _newRightBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());

        if (!Mathematics.IsVectorsAproximately(_newRightBoundary[_newRightBoundary.Count - 1].Pos, _boundaryBox.CustomBox[intersectionPoint[firstPointIndex].NextBoundaryPoint].Pos))
        {
            _newRightBoundary.Add(_boundaryBox.CustomBox[intersectionPoint[firstPointIndex].NextBoundaryPoint]);
        }

        for (int i = (intersectionPoint[firstPointIndex].NextBoundaryPoint + 1); i < intersectionPoint[secondPointIndex].NextBoundaryPoint;i++)
        {
            if (!Mathematics.IsVectorsAproximately(_newRightBoundary[_newRightBoundary.Count - 1].Pos, _boundaryBox.CustomBox[i].Pos))
            {
                _newRightBoundary.Add(_boundaryBox.CustomBox[i]);
            }
        }       

        if (_newRightBoundary.Count < 3)
        {
            return false;
        }

        else if (IsObstaclesInSamePolygon(_newRightBoundary, obstacles, out count))
        {
            List<BoundaryPoint> tempB = _newLeftBoundary;
            _newLeftBoundary = _newRightBoundary;
            _newRightBoundary = tempB;
            return true;
        }

        else if (count == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }   

    private bool IsObstaclesInSamePolygon(List<BoundaryPoint> _bp, List<GameObject> obstacles, out int _count)
    {
        Vector2[] points = new Vector2[_bp.Count];
        for (int i = 0; i < _bp.Count; i++)
        {
            points[i] = _bp[i].Pos;
        }
        int count = 0;
        foreach (var go in obstacles)
        {
            count += Mathematics.IsPointInPolygon(go.transform.position, points) ? 1 : 0;
        }
        _count = count;
        return count == obstacles.Count;
    }
}