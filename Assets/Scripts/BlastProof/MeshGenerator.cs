using Poly2Tri;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public static class MeshGenerator
{
    public static MeshProperties[] CreateMesh(List<BoundaryPoint> genPoly, Transform objTrans, float spriteSquareSize)
    {
        const int _FRONTOFFSET = 3;
        const float _BACKFACE_OFFSET = 0.5f;
        const float _SCALE_FACTOR = 1.1f;

        MeshProperties _generatedMesh;
        MeshProperties _frontFaceMesh;

        Vector2[] _genPolyArrFront = new Vector2[genPoly.Count];

        List<VertexProperties> _verts = new List<VertexProperties>();
        List<VertexProperties> _frontFaceVerticies = new List<VertexProperties>();

        List<int> _indicies = new List<int>();
        List<int> _frontFaceIndicies = new List<int>();

        //ConvertingToArray
        for (int i = 0; i < genPoly.Count; i++)
        {
            _genPolyArrFront[i] = objTrans.TransformPoint(genPoly[i].Pos);
        }

        Vector3 vecSum = Vector3.zero;

        //VerticiesFront
        for (int i = 0; i < genPoly.Count; i++)
        {
            Vector3 _position = new Vector3(_genPolyArrFront[i].x, _genPolyArrFront[i].y, 0.0f);

            vecSum += _position;

            _verts.Add(new VertexProperties { position = _position });
            _verts.Add(new VertexProperties { position = _position });
            _verts.Add(new VertexProperties { position = _position });

            _frontFaceVerticies.Add(new VertexProperties { position = _position });
        }

        //Calculating the center of the unscaled polygon
        Vector3 polygonCenter = vecSum / genPoly.Count;
        polygonCenter.z = objTrans.position.z;

        Matrix4x4 scaleMatrix = BlastProof.Mathematics.ScaleMatrix(_SCALE_FACTOR);

        Vector3 vecSum2 = Vector3.zero;

        //VerticiesBack
        for (int i = 0; i < genPoly.Count; i++)
        {
            Vector3 _position = scaleMatrix.MultiplyPoint(new Vector3(_genPolyArrFront[i].x, _genPolyArrFront[i].y, _BACKFACE_OFFSET));
            vecSum2 += _position;

            _verts.Add(new VertexProperties { position = _position });
            _verts.Add(new VertexProperties { position = _position });
            _verts.Add(new VertexProperties { position = _position });
        }

        Vector3 scaledPolyCenter = vecSum2 / genPoly.Count;
        scaledPolyCenter.z = objTrans.position.z;

        //Caching how much should the polygon move on axis so it matches the original scale polygon
        Vector3 translVec = polygonCenter - scaledPolyCenter;

        Matrix4x4 transMatrix = BlastProof.Mathematics.TranslateMatrix(translVec);

        //Multiplying each backface polygon position with the translation matrix so the center of backface polygon and frontface polygon matches
        for (int i = _verts.Count / 2; i < _verts.Count; i++)
        {
            _verts[i].position = transMatrix.MultiplyPoint(_verts[i].position);
        }

        var newGenPolyArrFront = new List<Poly2Tri.PolygonPoint>();

        for(int i = 0; i<_genPolyArrFront.Length;i++)
        {
            var point = new Poly2Tri.PolygonPoint(_genPolyArrFront[i].x, _genPolyArrFront[i].y);
            point.index = i;
            newGenPolyArrFront.Add(point);
        }

        Poly2Tri.Polygon poly = new Poly2Tri.Polygon(newGenPolyArrFront);

        DTSweepContext tcx = new DTSweepContext();
        tcx.PrepareTriangulation(poly);
        DTSweep.Triangulate(tcx);          

        List<int> indiciesFromTriangulator = new List<int>();

        foreach (var triangle in poly.Triangles)
        {
            foreach (var point in triangle.Points)
            {
                indiciesFromTriangulator.Add(point.index);
            }
        }

        indiciesFromTriangulator.Reverse();

        Triangulator tri = new Triangulator(_genPolyArrFront);
        int[] triangledPoly = indiciesFromTriangulator.ToArray();

        //FrontFaceIndicies
        for (int i = 0; i < triangledPoly.Length; i++)
        {
            _indicies.Add(triangledPoly[i] * _FRONTOFFSET);
            _frontFaceIndicies.Add(triangledPoly[i]);
        }

        //BackFaceIndicies
        for (int i = triangledPoly.Length - 1; i >= 0; i--)
        {
            _indicies.Add(triangledPoly[i] * _FRONTOFFSET + (_verts.Count / 2));
        }

        //Front-Back Faces normals
        for (int i = 0; i < _indicies.Count / 2; i += 3)
        {
            int[] v1 = { _indicies[i], _indicies[(i + 1) % (_indicies.Count / 2)], _indicies[(i + 2) % (_indicies.Count / 2)] };
            int[] v2 = { _indicies[i + (_indicies.Count / 2)], _indicies[(i + 1) % (_indicies.Count / 2) + (_indicies.Count / 2)], _indicies[(i + 2) % (_indicies.Count / 2) + (_indicies.Count / 2)] };

            GetNormalsForVerts(_verts, v1);
            GetNormalsForVerts(_verts, v2);
            GetUVsWithSize(_verts, v1, Faces.forward, spriteSquareSize);
            GetUVsWithSize(_verts, v2, Faces.forward, spriteSquareSize);
        }     

        //Generating Side Triangles
        for (int i = 1; i < _verts.Count / 2; i += 6)
        {
            int[] frontFaceVerts = { i, (i + 3) % (_verts.Count / 2) };
            int[] backFaceVerts = { (i + (_verts.Count / 2)), (i + 3) % (_verts.Count / 2) + (_verts.Count / 2) };

            //verts pos are used as uvs
            int[] uvCoord = { i, (i + 3) % (_verts.Count / 2), (i + (_verts.Count / 2)), (i + 3) % (_verts.Count / 2) + (_verts.Count / 2) };

            GetQuadIndicies(frontFaceVerts, backFaceVerts, _indicies, _verts);

            GetUVsWithSize(_verts, uvCoord, Faces.left, spriteSquareSize);
        }

        //Generate Up-Down Verts
        for (int i = 5; i < _verts.Count / 2; i += 6)
        {
            int[] frontFaceVerts = { i % (_verts.Count / 2), (i + 3) % (_verts.Count / 2) };
            int[] backFaceVerts = { (i % (_verts.Count/2) + (_verts.Count / 2)),
                                    (i + 3) % (_verts.Count / 2) + (_verts.Count / 2) };

            //verts pos are used as uvs
            int[] uvCoord = { i % (_verts.Count / 2), (i + 3) % (_verts.Count / 2), (i % (_verts.Count / 2) + (_verts.Count / 2)), (i + 3) % (_verts.Count / 2) + (_verts.Count / 2) };

            GetQuadIndicies(frontFaceVerts, backFaceVerts, _indicies, _verts);
            GetUVsWithSize(_verts, uvCoord, Faces.up, spriteSquareSize);

        }

        _generatedMesh = new MeshProperties(_verts);
        _generatedMesh.mesh_center = polygonCenter;
        _generatedMesh.SetIndicies(_indicies.ToArray());

        _frontFaceMesh = new MeshProperties(_frontFaceVerticies);
        _frontFaceMesh.mesh_center = polygonCenter;
        _frontFaceMesh.SetIndicies(_frontFaceIndicies.ToArray());

        return new MeshProperties[] { _generatedMesh, _frontFaceMesh };
    }
    public static void GetQuadIndicies(int[] frontFaceIndicies, int[] backFaceIndicies, List<int> indicies, List<VertexProperties> verts)
    {
        indicies.Add(backFaceIndicies[0]);
        indicies.Add(backFaceIndicies[1]);
        indicies.Add(frontFaceIndicies[1]);
        indicies.Add(frontFaceIndicies[1]);
        indicies.Add(frontFaceIndicies[0]);
        indicies.Add(backFaceIndicies[0]);

        int[] firstTiangleVerts = { backFaceIndicies[0], backFaceIndicies[1], frontFaceIndicies[1] };
        int[] secondTriangleVerts = { frontFaceIndicies[1], frontFaceIndicies[0], backFaceIndicies[0] };

        GetNormalsForVerts(verts, firstTiangleVerts);
        GetNormalsForVerts(verts, secondTriangleVerts);
    }

    public static void GetNormalsForVerts(List<VertexProperties> verts, int[] indicies)
    {
        int[] v = { indicies[0], indicies[1], indicies[2] };

        Vector3 firstDirection = (verts[v[2]].position - verts[v[1]].position).normalized;
        Vector3 secondDirection = (verts[v[0]].position - verts[v[1]].position).normalized;

        Vector3 normal = Vector3.Cross(firstDirection, secondDirection).normalized;

        verts[v[0]].normal = normal;
        verts[v[1]].normal = normal;
        verts[v[2]].normal = normal;
    }

    public static void GetUVs(List<VertexProperties> verts, int[] indicies, Faces face)
    {
        switch (face)
        {
            case Faces.forward:            
                {
                    for (int i = 0; i < indicies.Length; i++)
                    {
                        verts[indicies[i]].uv.x = verts[indicies[i]].position.x;
                        verts[indicies[i]].uv.y = verts[indicies[i]].position.y;
                    }
                }
                break;           
            case Faces.up:
                {
                    for (int i = 0; i < indicies.Length; i++)
                    {
                        verts[indicies[i]].uv.x = verts[indicies[i]].position.x;
                        verts[indicies[i]].uv.y = verts[indicies[i]].position.z;
                    }
                }
                break;
            case Faces.left:
                {
                    for (int i = 0; i < indicies.Length; i++)
                    {
                        verts[indicies[i]].uv.x = verts[indicies[i]].position.z;
                        verts[indicies[i]].uv.y = verts[indicies[i]].position.y;
                    }
                }
                break;

        }
    }

    public static void GetUVsWithSize(List<VertexProperties> verts, int[] indicies, Faces face, float spriteSquareSize)
    {
        switch (face)
        {
            case Faces.forward:
                {
                    for (int i = 0; i < indicies.Length; i++)
                    {
                        verts[indicies[i]].uv.x = verts[indicies[i]].position.x/spriteSquareSize;
                        verts[indicies[i]].uv.y = verts[indicies[i]].position.y/spriteSquareSize;
                    }
                }
                break;
            case Faces.up:
                {
                    for (int i = 0; i < indicies.Length; i++)
                    {
                        verts[indicies[i]].uv.x = verts[indicies[i]].position.x/spriteSquareSize;
                        verts[indicies[i]].uv.y = verts[indicies[i]].position.y/spriteSquareSize; 
                    }
                }
                break;
            case Faces.left:
                {
                    for (int i = 0; i < indicies.Length; i++)
                    {
                        verts[indicies[i]].uv.x = verts[indicies[i]].position.x/spriteSquareSize;
                        verts[indicies[i]].uv.y = verts[indicies[i]].position.y/spriteSquareSize;
                    }
                }
                break;

        }
    }

    public enum Faces
    {
        none = 0,
        up = 1, //Same with down
        left = 2, //Same with right
        forward = 3 // Same with backward
    }
}
