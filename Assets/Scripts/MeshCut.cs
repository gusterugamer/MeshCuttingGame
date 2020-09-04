using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;

public static class MeshCut
{
    private static List<BoundaryPoint> _newRightBoundary;
    private static List<BoundaryPoint> _newLeftBoundary;

    // TEST ONLY
    public static List<IntersectionPoint> intersectionPoint;

    //TEST ONLY
    public static void StartCutting (SpriteShapeController shape, Material capMaterial,Vector3 startPos, Vector3 endPos)
    {
        Cut(shape, capMaterial, startPos, endPos);
    }

    public static void Cut(SpriteShapeController shape, Material capMaterial, Vector3 startPos, Vector3 endPos)
    {
        CustomBoundryBox _boundaryBox = shape.GetComponent<CustomBoundryBox>();
        List<IntersectionPoint> intersectionPoints = _boundaryBox.GetIntersections(startPos, endPos);     

        if (intersectionPoints.Count == 2)
        {         
            CreateNewBoundary(_boundaryBox, ref intersectionPoints);

            MeshProperties generatedMesh = MeshGenerator.CreateMesh(_newRightBoundary, shape.transform, 16.0f);

            Mesh newMesh = new Mesh();
            newMesh.name = "GenObjectMesh";
            newMesh.SetVertices(generatedMesh.mesh_vertices);
            newMesh.SetTriangles(generatedMesh.mesh_indicies, 0);
            newMesh.SetNormals(generatedMesh.mesh_normals);
            newMesh.SetUVs(0, generatedMesh.mesh_uvs);

            GameObject generatedObj = new GameObject();
            generatedObj.name = "right side";
            generatedObj.transform.position = Vector3.zero;
            generatedObj.AddComponent<MeshFilter>();
            generatedObj.AddComponent<MeshRenderer>();
            generatedObj.AddComponent<Rigidbody>().angularDrag = 0.0f;
            generatedObj.name = "Generated";
            generatedObj.GetComponent<MeshFilter>().mesh = newMesh;
            generatedObj.GetComponent<MeshRenderer>().material = Resources.Load("Material/SignMaterial") as Material;

            GameObject maskObj = new GameObject();
            maskObj.AddComponent<MeshFilter>();
            maskObj.AddComponent<MeshRenderer>();
            maskObj.transform.position = new Vector3(0.0f,0.0f, 0.5f);
            maskObj.GetComponent<MeshFilter>().mesh = newMesh;
            maskObj.name = "mask";
            maskObj.GetComponent<MeshRenderer>().material = Resources.Load("Material/MaskMaterial") as Material;
            //testobj.AddComponent<Rigidbody>().AddForce(new Vector3(100.1f, 150f, 130f), ForceMode.Force);

            _boundaryBox.UpdateCustomBoundary(_newLeftBoundary);
        }     
    }

    private static void CreateNewBoundary(in CustomBoundryBox _boundaryBox, ref List<IntersectionPoint> intersectionPoint)
    {
        //picking first and second intersection point indicies by looking who is closest to the start of the ppolygon
        int firstPointIndex = intersectionPoint[0]._nextBoundaryPoint < intersectionPoint[1]._nextBoundaryPoint ? 0 : 1;
        int secondPointIndex = 1 - firstPointIndex;       

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

        _newRightBoundary.Add(intersectionPoint[secondPointIndex].toBoundaryPoint());
        _newRightBoundary.Add(intersectionPoint[firstPointIndex].toBoundaryPoint());

        for (int i = intersectionPoint[firstPointIndex]._nextBoundaryPoint; i < intersectionPoint[firstPointIndex]._nextBoundaryPoint + intersectionPointDistance; i++)
        {
            _newRightBoundary.Add(_boundaryBox.m_CustomBox[i]);
        }  
    }   
}