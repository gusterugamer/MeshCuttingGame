﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

public static class MeshCut
{
    private static Plane _blade;
   // private static PrimitivesPro.Utils.Plane _blade;

    private static List<BoundaryPoint> _newRightBoundary;
    private static List<BoundaryPoint> _newLeftBoundary;

    private static HashSet<Vector3> isDuplicate;

     private static Vector3 _startPos;
    private static Vector3  _endPos;
    // Caching 
    private static Mesh_Maker.Triangle _triangleCache = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);

    private static List<Vector3> _newVerticesCache = new List<Vector3>();

    private static int _capMatSub = 1;

    // TEST ONLY
    public static List<IntersectionPoint> intersectionPoint;

    //TEST ONLY
    public static GameObject[] Cut(GameObject victim, Material capMaterial, Vector3 startPos, Vector3 endPos)
    {
        CustomBoundryBox _boundaryBox = victim.GetComponent<CustomBoundryBox>();
        List<IntersectionPoint> intersectionPoints = _boundaryBox.GetIntersections(startPos, endPos);

        _startPos = startPos;
        _endPos = endPos;

        if (intersectionPoints.Count == 2)
        {

            //TEST
            intersectionPoint = intersectionPoints;

            //TEST
            
            //CHACHE

            // get the victims mesh
            Mesh _victim_mesh = victim.GetComponent<MeshFilter>().mesh;

            //New meshes created after cut
            Mesh_Maker _leftSideMesh = new Mesh_Maker();
            Mesh_Maker _rightSideMesh = new Mesh_Maker();

            //Chaching mesh properties
            MeshProperties mp = new MeshProperties();

            // two new meshes
            _leftSideMesh.Clear();
            _rightSideMesh.Clear();
            _newVerticesCache.Clear();


            mp.mesh_vertices = _victim_mesh.vertices;
            mp.mesh_normals = _victim_mesh.normals;
            mp.mesh_uvs = _victim_mesh.uv;
            mp.mesh_tangents = _victim_mesh.tangents;

            if (mp.mesh_tangents != null && mp.mesh_tangents.Length == 0)
                mp.mesh_tangents = null;

            //New objects creation          

            GameObject leftSideObj = victim;

            GameObject rightSideObj = new GameObject("right side", typeof(MeshFilter), typeof(MeshRenderer));
            rightSideObj.transform.position = victim.transform.position;
            rightSideObj.transform.rotation = victim.transform.rotation;
            rightSideObj.transform.localScale = victim.transform.localScale;

            //BOUNDARY CUT      
            CreateNewBoundary(victim, leftSideObj, rightSideObj, ref intersectionPoints);

            //FILTER WHOLE TRIANGLES           
            FilterWholeTriangles(mp, _victim_mesh, ref _leftSideMesh, ref _rightSideMesh);

            // The capping Material will be at the end
            Material[] mats = victim.GetComponent<MeshRenderer>().sharedMaterials;
            if (mats[mats.Length - 1].name != capMaterial.name)
            {
                Material[] newMats = new Material[mats.Length + 1];
                mats.CopyTo(newMats, 0);
                newMats[mats.Length] = capMaterial;
                mats = newMats;
            }
            _capMatSub = mats.Length - 1; // for later use               

            // cap the opennings
            //Cap_the_Cut(ref _leftSideMesh, ref _rightSideMesh);

            // Left Mesh
            Mesh left_HalfMesh = _leftSideMesh.GetMesh();
            left_HalfMesh.name = "Split Mesh Left";

            // Right Mesh
            Mesh right_HalfMesh = _rightSideMesh.GetMesh();
            right_HalfMesh.name = "Split Mesh Right";

            // assign the game objects			

            leftSideObj.name = "left side";
            leftSideObj.GetComponent<MeshFilter>().mesh = left_HalfMesh;
            rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;

            //assign mats
            leftSideObj.GetComponent<MeshRenderer>().materials = mats;
            rightSideObj.GetComponent<MeshRenderer>().materials = mats;

            return new GameObject[] { leftSideObj, rightSideObj };
        }
        else
        {
            return null;
        }
    }

    private static void CreateNewBoundary(in GameObject victim, in GameObject leftSideObj, in GameObject rightSideObj, ref List<IntersectionPoint> intersectionPoint)
    {
        int firstPointIndex = intersectionPoint[0]._nextBoundaryPoint < intersectionPoint[1]._nextBoundaryPoint ? 0 : 1;
        int secondPointIndex = 1 - firstPointIndex;

        //Plane that helps to create the new indicies
      // _blade = Mathematics.CreateSlicePlane(intersectionPoint[firstPointIndex]._pos, intersectionPoint[secondPointIndex]._pos);
      

        Vector3 worldIntersectionPoint1 = victim.transform.TransformPoint(intersectionPoint[firstPointIndex]._pos);
        Vector3 worldIntersectionPoint2 = victim.transform.TransformPoint(intersectionPoint[secondPointIndex]._pos);
        Vector3 depth = worldIntersectionPoint1 + victim.transform.TransformPoint(new Vector3(0.0f, 0.0f, 1.0f));

        _blade = Mathematics.CreateSlicePlane(intersectionPoint[firstPointIndex]._pos, intersectionPoint[secondPointIndex]._pos);

        //leftSIde
        CustomBoundryBox _boundaryBox = victim.GetComponent<CustomBoundryBox>();
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

        _leftSideBoundary.drawNew = true;

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

        rightSide.drawNew = true;
    }

    private static void FilterWholeTriangles(in MeshProperties mp, in Mesh _victim_mesh, ref Mesh_Maker leftSideMesh, ref Mesh_Maker rightSideMesh)
    {
        for (int submeshIterator = 0; submeshIterator < _victim_mesh.subMeshCount; submeshIterator++)
        {
            //Triangles flag
            bool[] _isLeftSideCache = new bool[3];
            bool[] _isRightSideCache = new bool[3];

            // Triangles
            int[] indices = _victim_mesh.GetTriangles(submeshIterator);

            for (int i = 0; i < indices.Length; i += 3)
            {

                int index_1 = indices[i];
                int index_2 = indices[i + 1];
                int index_3 = indices[i + 2];

                // verts
                _triangleCache.vertices[0] = mp.mesh_vertices[index_1];
                _triangleCache.vertices[1] = mp.mesh_vertices[index_2];
                _triangleCache.vertices[2] = mp.mesh_vertices[index_3];

                // normals
                _triangleCache.normals[0] = mp.mesh_normals[index_1];
                _triangleCache.normals[1] = mp.mesh_normals[index_2];
                _triangleCache.normals[2] = mp.mesh_normals[index_3];

                // uvs
                _triangleCache.uvs[0] = mp.mesh_uvs[index_1];
                _triangleCache.uvs[1] = mp.mesh_uvs[index_2];
                _triangleCache.uvs[2] = mp.mesh_uvs[index_3];

                // tangents
                if (mp.mesh_tangents != null)
                {
                    _triangleCache.tangents[0] = mp.mesh_tangents[index_1];
                    _triangleCache.tangents[1] = mp.mesh_tangents[index_2];
                    _triangleCache.tangents[2] = mp.mesh_tangents[index_3];
                }
                else
                {
                    _triangleCache.tangents[0] = Vector4.zero;
                    _triangleCache.tangents[1] = Vector4.zero;
                    _triangleCache.tangents[2] = Vector4.zero;
                }               

                // which side are the vertices on
                _isLeftSideCache[0] = Mathematics.PointInPolygon(mp.mesh_vertices[index_1], _newLeftBoundary.ToArray());
                _isLeftSideCache[1] = Mathematics.PointInPolygon(mp.mesh_vertices[index_2], _newLeftBoundary.ToArray());
                _isLeftSideCache[2] = Mathematics.PointInPolygon(mp.mesh_vertices[index_3], _newLeftBoundary.ToArray());

                _isRightSideCache[0] = Mathematics.PointInPolygon(mp.mesh_vertices[index_1], _newRightBoundary.ToArray());
                _isRightSideCache[1] = Mathematics.PointInPolygon(mp.mesh_vertices[index_2], _newRightBoundary.ToArray());
                _isRightSideCache[2] = Mathematics.PointInPolygon(mp.mesh_vertices[index_3], _newRightBoundary.ToArray());             

                // whole triangle
                if (_isLeftSideCache[0] == _isLeftSideCache[1] && _isLeftSideCache[0] == _isLeftSideCache[2])
                {
                    if (_isLeftSideCache[0])// left side    
                    {
                        leftSideMesh.AddTriangle(_triangleCache, submeshIterator);
                    }
                    else // right side
                    {
                        rightSideMesh.AddTriangle(_triangleCache, submeshIterator);
                    }
                }
                else
                { // cut the triangle

                    Cut_this_Face(ref _triangleCache, submeshIterator, ref leftSideMesh, ref rightSideMesh);
                }
            }
        }
    }

    #region Cutting
    // Caching
    private static Mesh_Maker.Triangle _leftTriangleCache = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
    private static Mesh_Maker.Triangle _rightTriangleCache = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
    private static Mesh_Maker.Triangle _newTriangleCache = new Mesh_Maker.Triangle(new Vector3[3], new Vector2[3], new Vector3[3], new Vector4[3]);
    // Functions
    private static void Cut_this_Face(ref Mesh_Maker.Triangle triangle, int submesh, ref Mesh_Maker _leftSideMesh, ref Mesh_Maker _rightSideMesh)
    {
        bool[] _isLeftSideCache = new bool[3];     

        _isLeftSideCache[0] = Mathematics.PointInPolygon(triangle.vertices[0], _newLeftBoundary.ToArray());
        _isLeftSideCache[1] = Mathematics.PointInPolygon(triangle.vertices[1], _newLeftBoundary.ToArray());
        _isLeftSideCache[2] = Mathematics.PointInPolygon(triangle.vertices[2], _newLeftBoundary.ToArray());

        int leftCount = 0;
        int rightCount = 0;

        for (int i = 0; i < 3; i++)
        {
            if (_isLeftSideCache[i])
            { // left

                _leftTriangleCache.vertices[leftCount] = triangle.vertices[i];
                _leftTriangleCache.uvs[leftCount] = triangle.uvs[i];
                _leftTriangleCache.normals[leftCount] = triangle.normals[i];
                _leftTriangleCache.tangents[leftCount] = triangle.tangents[i];

                leftCount++;
            }
            else
            { // right

                _rightTriangleCache.vertices[rightCount] = triangle.vertices[i];
                _rightTriangleCache.uvs[rightCount] = triangle.uvs[i];
                _rightTriangleCache.normals[rightCount] = triangle.normals[i];
                _rightTriangleCache.tangents[rightCount] = triangle.tangents[i];

                rightCount++;
            }
        }

        // find the new triangles X 3
        // first the new vertices

        // this will give me a triangle with the solo point as first
        if (leftCount == 1)
        {
            _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
            _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
            _triangleCache.normals[0] = _leftTriangleCache.normals[0];
            _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

            _triangleCache.vertices[1] = _rightTriangleCache.vertices[0];
            _triangleCache.uvs[1] = _rightTriangleCache.uvs[0];
            _triangleCache.normals[1] = _rightTriangleCache.normals[0];
            _triangleCache.tangents[1] = _rightTriangleCache.tangents[0];

            _triangleCache.vertices[2] = _rightTriangleCache.vertices[1];
            _triangleCache.uvs[2] = _rightTriangleCache.uvs[1];
            _triangleCache.normals[2] = _rightTriangleCache.normals[1];
            _triangleCache.tangents[2] = _rightTriangleCache.tangents[1];
        }
        else // rightCount == 1
        {
            _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
            _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
            _triangleCache.normals[0] = _rightTriangleCache.normals[0];
            _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

            _triangleCache.vertices[1] = _leftTriangleCache.vertices[0];
            _triangleCache.uvs[1] = _leftTriangleCache.uvs[0];
            _triangleCache.normals[1] = _leftTriangleCache.normals[0];
            _triangleCache.tangents[1] = _leftTriangleCache.tangents[0];

            _triangleCache.vertices[2] = _leftTriangleCache.vertices[1];
            _triangleCache.uvs[2] = _leftTriangleCache.uvs[1];
            _triangleCache.normals[2] = _leftTriangleCache.normals[1];
            _triangleCache.tangents[2] = _leftTriangleCache.tangents[1];
        }

        // now to find the intersection points between the solo point and the others        
        Vector3 edgeVector = _triangleCache.vertices[1] - _triangleCache.vertices[0];  // contains edge length and direction
        _blade.Raycast(new Ray(_triangleCache.vertices[0], edgeVector.normalized), out float distance);    

        
        //_blade.IntersectSegment(_triangleCache.vertices[1], _triangleCache.vertices[0], out t, out pos);

        float normalizedDistance = distance / edgeVector.magnitude;

        Vector3 pos = Vector3.Lerp(_triangleCache.vertices[0], _triangleCache.vertices[1], normalizedDistance);

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = pos;
        cube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        //Create new vertex
        Vector3 baryCoord = PrimitivesPro.MeshUtils.ComputeBarycentricCoordinates(_triangleCache.vertices[0], _triangleCache.vertices[1], _triangleCache.vertices[2], pos);

        Vector3 normal = (new Vector3(baryCoord.x * _triangleCache.normals[0].x + baryCoord.y * _triangleCache.normals[1].x + baryCoord.z * _triangleCache.normals[2].x,
                                    baryCoord.x * _triangleCache.normals[0].y + baryCoord.y * _triangleCache.normals[1].y + baryCoord.z * _triangleCache.normals[2].y,
                                    baryCoord.x * _triangleCache.normals[0].z + baryCoord.y * _triangleCache.normals[1].z + baryCoord.z * _triangleCache.normals[2].z));

        Vector2 uvs = (new Vector2(baryCoord.x * _triangleCache.uvs[0].x + baryCoord.y * _triangleCache.uvs[1].x + baryCoord.z * _triangleCache.uvs[2].x,
                            baryCoord.x * _triangleCache.uvs[0].y + baryCoord.y * _triangleCache.uvs[1].y + baryCoord.z * _triangleCache.uvs[2].y));

       

        _newTriangleCache.vertices[0] = pos;
        _newTriangleCache.uvs[0] = uvs;
        _newTriangleCache.normals[0] = normal;


        //_newTriangleCache.tangents[0] = Vector4.Lerp(_triangleCache.tangents[0], _triangleCache.tangents[1], normalizedDistance);

        /********************************************************************************************************************************/

        edgeVector = _triangleCache.vertices[2] - _triangleCache.vertices[0];
        _blade.Raycast(new Ray(_triangleCache.vertices[0], edgeVector.normalized), out distance);

        normalizedDistance = distance / edgeVector.magnitude;

        pos = Vector3.Lerp(_triangleCache.vertices[0], _triangleCache.vertices[2], normalizedDistance);

        baryCoord = PrimitivesPro.MeshUtils.ComputeBarycentricCoordinates(_triangleCache.vertices[0], _triangleCache.vertices[1], _triangleCache.vertices[2], pos);     

        normal = (new Vector3(baryCoord.x * _triangleCache.normals[0].x + baryCoord.y * _triangleCache.normals[1].x + baryCoord.z * _triangleCache.normals[2].x,
                            baryCoord.x * _triangleCache.normals[0].y + baryCoord.y * _triangleCache.normals[1].y + baryCoord.z * _triangleCache.normals[2].y,
                            baryCoord.x * _triangleCache.normals[0].z + baryCoord.y * _triangleCache.normals[1].z + baryCoord.z * _triangleCache.normals[2].z));

        uvs = (new Vector2(baryCoord.x * _triangleCache.uvs[0].x + baryCoord.y * _triangleCache.uvs[1].x + baryCoord.z * _triangleCache.uvs[2].x,
                    baryCoord.x * _triangleCache.uvs[0].y + baryCoord.y * _triangleCache.uvs[1].y + baryCoord.z * _triangleCache.uvs[2].y));       

        _newTriangleCache.vertices[1] = pos;
        _newTriangleCache.uvs[1] = uvs;
        _newTriangleCache.normals[1] = normal;


      //  _newTriangleCache.tangents[1] = Vector4.Lerp(_triangleCache.tangents[0], _triangleCache.tangents[2], normalizedDistance);

        //TEMP DISABLED

        //Check if vertex us duplicat

        if (_newTriangleCache.vertices[0] != _newTriangleCache.vertices[1])
        {
            //tracking newly created points
            _newVerticesCache.Add(_newTriangleCache.vertices[0]);
            _newVerticesCache.Add(_newTriangleCache.vertices[1]);
        }
        // make the new triangles
        // one side will get 1 the other will get 2

        if (leftCount == 1)
        {
            // first one on the left
            _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
            _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
            _triangleCache.normals[0] = _leftTriangleCache.normals[0];
            _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

            _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
            _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
            _triangleCache.normals[1] = _newTriangleCache.normals[0];
            _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

            _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
            _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
            _triangleCache.normals[2] = _newTriangleCache.normals[1];
            _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

            // check if it is facing the right way
            NormalCheck(ref _triangleCache);

            // add it
            _leftSideMesh.AddTriangle(_triangleCache, submesh);


            // other two on the right
            _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
            _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
            _triangleCache.normals[0] = _rightTriangleCache.normals[0];
            _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

            _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
            _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
            _triangleCache.normals[1] = _newTriangleCache.normals[0];
            _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

            _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
            _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
            _triangleCache.normals[2] = _newTriangleCache.normals[1];
            _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

            // check if it is facing the right way
            NormalCheck(ref _triangleCache);

            // add it
            _rightSideMesh.AddTriangle(_triangleCache, submesh);

            // third
            _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
            _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
            _triangleCache.normals[0] = _rightTriangleCache.normals[0];
            _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

            _triangleCache.vertices[1] = _rightTriangleCache.vertices[1];
            _triangleCache.uvs[1] = _rightTriangleCache.uvs[1];
            _triangleCache.normals[1] = _rightTriangleCache.normals[1];
            _triangleCache.tangents[1] = _rightTriangleCache.tangents[1];

            _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
            _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
            _triangleCache.normals[2] = _newTriangleCache.normals[1];
            _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

            // check if it is facing the right way
            NormalCheck(ref _triangleCache);

            // add it
            _rightSideMesh.AddTriangle(_triangleCache, submesh);          
        }
        else
        {
            // first one on the right
            _triangleCache.vertices[0] = _rightTriangleCache.vertices[0];
            _triangleCache.uvs[0] = _rightTriangleCache.uvs[0];
            _triangleCache.normals[0] = _rightTriangleCache.normals[0];
            _triangleCache.tangents[0] = _rightTriangleCache.tangents[0];

            _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
            _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
            _triangleCache.normals[1] = _newTriangleCache.normals[0];
            _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

            _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
            _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
            _triangleCache.normals[2] = _newTriangleCache.normals[1];
            _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

            // check if it is facing the right way
            NormalCheck(ref _triangleCache);

            // add it
            _rightSideMesh.AddTriangle(_triangleCache, submesh);


            // other two on the left
            _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
            _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
            _triangleCache.normals[0] = _leftTriangleCache.normals[0];
            _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

            _triangleCache.vertices[1] = _newTriangleCache.vertices[0];
            _triangleCache.uvs[1] = _newTriangleCache.uvs[0];
            _triangleCache.normals[1] = _newTriangleCache.normals[0];
            _triangleCache.tangents[1] = _newTriangleCache.tangents[0];

            _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
            _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
            _triangleCache.normals[2] = _newTriangleCache.normals[1];
            _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

            // check if it is facing the right way
            NormalCheck(ref _triangleCache);

            // add it
            _leftSideMesh.AddTriangle(_triangleCache, submesh);

            // third
            _triangleCache.vertices[0] = _leftTriangleCache.vertices[0];
            _triangleCache.uvs[0] = _leftTriangleCache.uvs[0];
            _triangleCache.normals[0] = _leftTriangleCache.normals[0];
            _triangleCache.tangents[0] = _leftTriangleCache.tangents[0];

            _triangleCache.vertices[1] = _leftTriangleCache.vertices[1];
            _triangleCache.uvs[1] = _leftTriangleCache.uvs[1];
            _triangleCache.normals[1] = _leftTriangleCache.normals[1];
            _triangleCache.tangents[1] = _leftTriangleCache.tangents[1];

            _triangleCache.vertices[2] = _newTriangleCache.vertices[1];
            _triangleCache.uvs[2] = _newTriangleCache.uvs[1];
            _triangleCache.normals[2] = _newTriangleCache.normals[1];
            _triangleCache.tangents[2] = _newTriangleCache.tangents[1];

            // check if it is facing the right way
            NormalCheck(ref _triangleCache);

            // add it
            _leftSideMesh.AddTriangle(_triangleCache, submesh);     
        }

    }
    #endregion

    #region Capping
    // Caching

    private static List<int> _capUsedIndicesCache = new List<int>();
    private static List<int> _capPolygonIndicesCache = new List<int>();
    // Functions

    private static void Cap_the_Cut(ref Mesh_Maker _leftSideMesh, ref Mesh_Maker _rightSideMesh)
    {

        _capUsedIndicesCache.Clear();
        _capPolygonIndicesCache.Clear();

        // find the needed polygons
        // the cut faces added new vertices by 2 each time to make an edge
        // if two edges contain the same Vector3 point, they are connected
        for (int i = 0; i < _newVerticesCache.Count; i += 2)
        {
            // check the edge
            if (!_capUsedIndicesCache.Contains(i)) // if it has one, it has this edge
            {
                //new polygon started with this edge
                _capPolygonIndicesCache.Clear();
                _capPolygonIndicesCache.Add(i);
                _capPolygonIndicesCache.Add(i + 1);

                _capUsedIndicesCache.Add(i);
                _capUsedIndicesCache.Add(i + 1);

                Vector3 connectionPointLeft = _newVerticesCache[i];
                Vector3 connectionPointRight = _newVerticesCache[i + 1];
                bool isDone = false;

                // look for more edges
                while (!isDone)
                {
                    isDone = true;

                    // loop through edges
                    for (int index = 0; index < _newVerticesCache.Count; index += 2)
                    {   // if it has one, it has this edge
                        if (!_capUsedIndicesCache.Contains(index))
                        {
                            Vector3 nextPoint1 = _newVerticesCache[index];
                            Vector3 nextPoint2 = _newVerticesCache[index + 1];

                            // check for next point in the chain
                            if (connectionPointLeft == nextPoint1 ||
                                connectionPointLeft == nextPoint2 ||
                                connectionPointRight == nextPoint1 ||
                                connectionPointRight == nextPoint2)
                            {
                                _capUsedIndicesCache.Add(index);
                                _capUsedIndicesCache.Add(index + 1);

                                // add the other
                                if (connectionPointLeft == nextPoint1)
                                {
                                    _capPolygonIndicesCache.Insert(0, index + 1);
                                    connectionPointLeft = _newVerticesCache[index + 1];
                                }
                                else if (connectionPointLeft == nextPoint2)
                                {
                                    _capPolygonIndicesCache.Insert(0, index);
                                    connectionPointLeft = _newVerticesCache[index];
                                }
                                else if (connectionPointRight == nextPoint1)
                                {
                                    _capPolygonIndicesCache.Add(index + 1);
                                    connectionPointRight = _newVerticesCache[index + 1];
                                }
                                else if (connectionPointRight == nextPoint2)
                                {
                                    _capPolygonIndicesCache.Add(index);
                                    connectionPointRight = _newVerticesCache[index];
                                }

                                isDone = false;
                            }
                        }
                    }
                }// while isDone = False

                // check if the link is closed
                // first == last
                if (_newVerticesCache[_capPolygonIndicesCache[0]] == _newVerticesCache[_capPolygonIndicesCache[_capPolygonIndicesCache.Count - 1]])
                    _capPolygonIndicesCache[_capPolygonIndicesCache.Count - 1] = _capPolygonIndicesCache[0];
                else
                    _capPolygonIndicesCache.Add(_capPolygonIndicesCache[0]);

                // cap
                //FillCap_Method1(_capPolygonIndicesCache, ref _leftSideMesh, ref _rightSideMesh);
            }
        }
    }

    //TEMP DISABLED

    //private static void FillCap_Method1(List<int> indices, ref Mesh_Maker _leftSideMesh, ref Mesh_Maker _rightSideMesh)
    //{
    //    // center of the cap
    //    Vector3 center = Vector3.zero;
    //    foreach (int index in indices)
    //    {
    //        center += _newVerticesCache[index];
    //    }

    //    center = center / indices.Count;

    //    // you need an axis based on the cap
    //    Vector3 upward = Vector3.zero;
    //    // 90 degree turn
    //    upward.x = _blade.normal.y;
    //    upward.y = -_blade.normal.x;
    //    upward.z = _blade.normal.z;
    //    Vector3 left = Vector3.Cross(_blade.normal, upward);

    //    Vector3 displacement = Vector3.zero;
    //    Vector2 newUV1 = Vector2.zero;
    //    Vector2 newUV2 = Vector2.zero;
    //    Vector2 newUV3 = Vector2.zero;

    //    // indices should be in order like a closed chain

    //    // go through edges and eliminate by creating triangles with connected edges
    //    // each new triangle removes 2 edges but creates 1 new edge
    //    // keep the chain in order
    //    int iterator = 0;
    //    while (indices.Count > 2)
    //    {

    //        Vector3 link1 = _newVerticesCache[indices[iterator]];
    //        Vector3 link2 = _newVerticesCache[indices[(iterator + 1) % indices.Count]];
    //        Vector3 link3 = _newVerticesCache[indices[(iterator + 2) % indices.Count]];

    //        displacement = link1 - center;
    //        newUV1 = Vector3.zero;
    //        newUV1.x = 0.5f + Vector3.Dot(displacement, left);
    //        newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
    //        //newUV1.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

    //        displacement = link2 - center;
    //        newUV2 = Vector3.zero;
    //        newUV2.x = 0.5f + Vector3.Dot(displacement, left);
    //        newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
    //        //newUV2.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

    //        displacement = link3 - center;
    //        newUV3 = Vector3.zero;
    //        newUV3.x = 0.5f + Vector3.Dot(displacement, left);
    //        newUV3.y = 0.5f + Vector3.Dot(displacement, upward);
    //        //newUV2.z = 0.5f + Vector3.Dot(displacement, _blade.normal);


    //        // add triangle
    //        _newTriangleCache.vertices[0] = link1;
    //        _newTriangleCache.uvs[0] = newUV1;
    //        _newTriangleCache.normals[0] = -_blade.normal;
    //        _newTriangleCache.tangents[0] = Vector4.zero;

    //        _newTriangleCache.vertices[1] = link2;
    //        _newTriangleCache.uvs[1] = newUV2;
    //        _newTriangleCache.normals[1] = -_blade.normal;
    //        _newTriangleCache.tangents[1] = Vector4.zero;

    //        _newTriangleCache.vertices[2] = link3;
    //        _newTriangleCache.uvs[2] = newUV3;
    //        _newTriangleCache.normals[2] = -_blade.normal;
    //        _newTriangleCache.tangents[2] = Vector4.zero;

    //        // add to left side
    //        NormalCheck(ref _newTriangleCache);

    //        _leftSideMesh.AddTriangle(_newTriangleCache, _capMatSub);

    //        // add to right side
    //        _newTriangleCache.normals[0] = _blade.normal;
    //        _newTriangleCache.normals[1] = _blade.normal;
    //        _newTriangleCache.normals[2] = _blade.normal;

    //        NormalCheck(ref _newTriangleCache);

    //        _rightSideMesh.AddTriangle(_newTriangleCache, _capMatSub);


    //        // adjust indices by removing the middle link
    //        indices.RemoveAt((iterator + 1) % indices.Count);

    //        // move on
    //        iterator = (iterator + 1) % indices.Count;
    //    }

    //}

    //private static void FillCap_Method2(List<int> indices, ref Mesh_Maker _rightSideMesh, ref Mesh_Maker _leftSideMesh)
    //{

    //    // center of the cap
    //    Vector3 center = Vector3.zero;
    //    foreach (var index in indices)
    //        center += _newVerticesCache[index];

    //    center = center / indices.Count;

    //    // you need an axis based on the cap
    //    Vector3 upward = Vector3.zero;
    //    // 90 degree turn
    //    upward.x = _blade.normal.y;
    //    upward.y = -_blade.normal.x;
    //    upward.z = _blade.normal.z;
    //    Vector3 left = Vector3.Cross(_blade.normal, upward);

    //    Vector3 displacement = Vector3.zero;
    //    Vector2 newUV1 = Vector2.zero;
    //    Vector2 newUV2 = Vector2.zero;

    //    for (int i = 0; i < indices.Count - 1; i++)
    //    {

    //        displacement = _newVerticesCache[indices[i]] - center;
    //        newUV1 = Vector3.zero;
    //        newUV1.x = 0.5f + Vector3.Dot(displacement, left);
    //        newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
    //        //newUV1.z = 0.5f + Vector3.Dot(displacement, _blade.normal);

    //        displacement = _newVerticesCache[indices[i + 1]] - center;
    //        newUV2 = Vector3.zero;
    //        newUV2.x = 0.5f + Vector3.Dot(displacement, left);
    //        newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
    //        //newUV2.z = 0.5f + Vector3.Dot(displacement, _blade.normal);



    //        _newTriangleCache.vertices[0] = _newVerticesCache[indices[i]];
    //        _newTriangleCache.uvs[0] = newUV1;
    //        _newTriangleCache.normals[0] = -_blade.normal;
    //        _newTriangleCache.tangents[0] = Vector4.zero;

    //        _newTriangleCache.vertices[1] = _newVerticesCache[indices[i + 1]];
    //        _newTriangleCache.uvs[1] = newUV2;
    //        _newTriangleCache.normals[1] = -_blade.normal;
    //        _newTriangleCache.tangents[1] = Vector4.zero;

    //        _newTriangleCache.vertices[2] = center;
    //        _newTriangleCache.uvs[2] = new Vector2(0.5f, 0.5f);
    //        _newTriangleCache.normals[2] = -_blade.normal;
    //        _newTriangleCache.tangents[2] = Vector4.zero;


    //        NormalCheck(ref _newTriangleCache);

    //        _leftSideMesh.AddTriangle(_newTriangleCache, _capMatSub);

    //        _newTriangleCache.normals[0] = _blade.normal;
    //        _newTriangleCache.normals[1] = _blade.normal;
    //        _newTriangleCache.normals[2] = _blade.normal;

    //        NormalCheck(ref _newTriangleCache);

    //        _rightSideMesh.AddTriangle(_newTriangleCache, _capMatSub);

    //    }

    //}
    #endregion

    #region Misc.
    private static void NormalCheck(ref Mesh_Maker.Triangle triangle)
    {
        Vector3 crossProduct = Vector3.Cross(triangle.vertices[1] - triangle.vertices[0], triangle.vertices[2] - triangle.vertices[0]);
        Vector3 averageNormal = (triangle.normals[0] + triangle.normals[1] + triangle.normals[2]) / 3.0f;
        float dotProduct = Vector3.Dot(averageNormal, crossProduct);
        if (dotProduct < 0)
        {
            Vector3 temp = triangle.vertices[2];
            triangle.vertices[2] = triangle.vertices[0];
            triangle.vertices[0] = temp;

            temp = triangle.normals[2];
            triangle.normals[2] = triangle.normals[0];
            triangle.normals[0] = temp;

            Vector2 temp2 = triangle.uvs[2];
            triangle.uvs[2] = triangle.uvs[0];
            triangle.uvs[0] = temp2;

            Vector4 temp3 = triangle.tangents[2];
            triangle.tangents[2] = triangle.tangents[0];
            triangle.tangents[0] = temp3;
        }

    }
    #endregion

}