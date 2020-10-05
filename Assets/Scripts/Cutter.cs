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

    private Material maskMaterial = Resources.Load("Material/MaskMaterial") as Material;

    public List<BoundaryPoint> NewRightBoundary { get => _newRightBoundary; private set { _newRightBoundary = value; } }
    public List<BoundaryPoint> NewLeftBoundary { get => _newLeftBoundary; private set { _newLeftBoundary = value; } }
    public bool Cut(SpriteShapeController shape, Material textureMat, List<IntersectionPoint> intersectionPoints, List<GameObject> obstacles, out GameObject mask)
    {
        CustomBoundryBox _boundaryBox = shape.GetComponent<CustomBoundryBox>();
        
        //Decides which value is bigger to create the size of texture square
        float spriteSquareSize = Mathf.Max(_boundaryBox.MaxX, _boundaryBox.MaxY);

        if (intersectionPoints.Count == 2)
        {
            bool distanceBeetWeenPoints = Vector3.Distance(intersectionPoints[0]._pos, intersectionPoints[1]._pos) > 0.5f;
            if (CreateNewBoundary(_boundaryBox, intersectionPoints, obstacles) && distanceBeetWeenPoints)
            {
                //Generates a 3d mesh out of cutted polygon (generatedMesh) and uses it's frontface as mask (maskMesh)
                MeshProperties[] newMeshes = MeshGenerator.CreateMesh(NewRightBoundary, shape.transform, spriteSquareSize);
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
                maskObj.GetComponent<MeshRenderer>().material = maskMaterial;
                //testobj.AddComponent<Rigidbody>().AddForce(new Vector3(100.1f, 150f, 130f), ForceMode.Force);                

                mask = maskObj;
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

    private bool CreateNewBoundary(in CustomBoundryBox _boundaryBox, List<IntersectionPoint> intersectionPoint, List<GameObject> obstacles)
    {
        //picking first and second intersection point indicies by looking who is closest to the start of the ppolygon
        int firstPointIndex = intersectionPoint[0]._nextBoundaryPoint < intersectionPoint[1]._nextBoundaryPoint ? 0 : 1;
        int secondPointIndex = 1 - firstPointIndex;

       // Plane plane = Mathematics.SlicePlane(intersectionPoint[firstPointIndex]._pos, intersectionPoint[secondPointIndex]._pos, Camera.main.transform.forward);

        int count;

        NewLeftBoundary = new List<BoundaryPoint>();
        NewRightBoundary = new List<BoundaryPoint>();

        for (int i = 0; i < intersectionPoint[firstPointIndex]._nextBoundaryPoint; i++)
        {            
            NewLeftBoundary.Add(_boundaryBox.m_CustomBox[i]);            
        }

        if (!Mathematics.IsVectorsAproximately(NewLeftBoundary[NewLeftBoundary.Count - 1].m_pos, intersectionPoint[firstPointIndex]._pos))
        {
            NewLeftBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());           
        }

        if (!Mathematics.IsVectorsAproximately(NewLeftBoundary[NewLeftBoundary.Count - 1].m_pos, intersectionPoint[secondPointIndex]._pos))
        {
            NewLeftBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());
        }      

        for (int i = intersectionPoint[secondPointIndex]._nextBoundaryPoint; i < _boundaryBox.m_CustomBox.Count; i++)
        {           
            NewLeftBoundary.Add(_boundaryBox.m_CustomBox[i]);            
        }

        int dup = 0;
        for (int i = 1; i < NewLeftBoundary.Count; i++)
        {
            if (Mathematics.IsVectorsAproximately(NewLeftBoundary[i-1].m_pos, NewLeftBoundary[i].m_pos))
            {
                dup++;                
            }
        }

        //rightside
        int intersectionPointDistance = intersectionPoint[secondPointIndex]._previousBoundaryPoint - intersectionPoint[firstPointIndex]._previousBoundaryPoint;

        NewRightBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());
        NewRightBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());

        if (!Mathematics.IsVectorsAproximately(NewRightBoundary[NewRightBoundary.Count - 1].m_pos, _boundaryBox.m_CustomBox[intersectionPoint[firstPointIndex]._nextBoundaryPoint].m_pos))
        {
            NewRightBoundary.Add(_boundaryBox.m_CustomBox[intersectionPoint[firstPointIndex]._nextBoundaryPoint]);
        }

        for (int i = (intersectionPoint[firstPointIndex]._nextBoundaryPoint + 1); i < intersectionPoint[firstPointIndex]._nextBoundaryPoint + intersectionPointDistance; i++)
        {            
            NewRightBoundary.Add(_boundaryBox.m_CustomBox[i]);            
        }

        int dup2 = 0;
        for (int i = 1; i < NewLeftBoundary.Count; i++)
        {
            if (Mathematics.IsVectorsAproximately(NewLeftBoundary[i - 1].m_pos, NewLeftBoundary[i].m_pos))
            {
                dup2++;
            }
        }

        if (NewRightBoundary.Count < 3)
        {
            return false;
        }

        else if (IsObstaclesInSamePolygon(NewRightBoundary, obstacles,out count))
        {
            List<BoundaryPoint> tempB = NewLeftBoundary;
            NewLeftBoundary = NewRightBoundary;
            NewRightBoundary = tempB;
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
    private bool IsObjectsOnSameSide(Plane plane, List<GameObject> obstacles, out int _count)
    {
        int count = 0;
        foreach (var go in obstacles)
        {
            count += plane.GetSide(go.transform.position) ? 1 : -1;
        }
        _count = count;
        return Math.Abs(count) == obstacles.Count;
    }

    private bool isInBetweenHeads(IntersectionPoint point, List<BoundaryPoint> cb)
    {
        return !Mathematics.IsVectorsAproximately(point._pos, cb[point._previousBoundaryPoint % cb.Count].m_pos) &&
               !Mathematics.IsVectorsAproximately(point._pos, cb[point._nextBoundaryPoint % cb.Count].m_pos);
    }

    private bool IsObstaclesInSamePolygon(List<BoundaryPoint> _bp, List<GameObject> obstacles, out int _count)
    {
        Vector2[] points = new Vector2[_bp.Count];
        for (int i=0;i<_bp.Count;i++)
        {
            points[i] = _bp[i].m_pos;
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