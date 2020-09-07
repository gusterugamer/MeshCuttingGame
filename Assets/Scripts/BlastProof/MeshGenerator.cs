using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshProperties CreateMesh(List<BoundaryPoint> genPoly, Transform objTrans,float spriteSquareSize)
    {
        const int FRONTOFFSET = 3;
        const float BACKFACE_OFFSET = 1.2f;
        const float SCALE_FACTOR = 1.3f;


        MeshProperties generatedMesh;

        Vector2[] genPolyArrFront = new Vector2[genPoly.Count];

        List<VertexProperties> verts = new List<VertexProperties>();

        List<int> indicies = new List<int>();

        //ConvertingToArray
        for (int i = 0; i < genPoly.Count; i++)
        {
            genPolyArrFront[i] = objTrans.TransformPoint(genPoly[i].m_pos);
        }

        Vector3 vecSum = Vector3.zero;

        //VerticiesFront
        for (int i = 0; i < genPoly.Count; i++)
        {
            Vector3 _position = new Vector3(genPolyArrFront[i].x, genPolyArrFront[i].y, 0.0f);

            vecSum += _position;

            verts.Add(new VertexProperties { position = _position });
            verts.Add(new VertexProperties { position = _position });
            verts.Add(new VertexProperties { position = _position });
        }

        //Calculating the center of the unscaled polygon
        Vector3 polygonCenter = vecSum / genPoly.Count;
        polygonCenter.z = objTrans.position.z;

        Matrix4x4 scaleMatrix = BlastProof.Mathematics.ScaleMatrix(SCALE_FACTOR);   

        Vector3 vecSum2 = Vector3.zero;    

        //VerticiesBack
        for (int i = 0; i < genPoly.Count; i++)
        {
            Vector3 _position = scaleMatrix.MultiplyPoint(new Vector3(genPolyArrFront[i].x, genPolyArrFront[i].y, BACKFACE_OFFSET));
            vecSum2 += _position;

            verts.Add(new VertexProperties { position = _position});
            verts.Add(new VertexProperties { position = _position});
            verts.Add(new VertexProperties { position = _position});
        }

        Vector3 scaledPolyCenter = vecSum2 / genPoly.Count;
        scaledPolyCenter.z = objTrans.position.z;

        //Caching how much should the polygon move on axis so it matches the original scale polygon
        Vector3 translVec = polygonCenter - scaledPolyCenter;

        Matrix4x4 transMatrix = BlastProof.Mathematics.TranslateMatrix(translVec);    

        //Multiplying each backface polygon position with the translation matrix so the center of backface polygon and frontface polygon matches
        for (int i = verts.Count/2; i<verts.Count;i++)
        {
            verts[i].position = transMatrix.MultiplyPoint(verts[i].position);
        }


        Triangulator tri = new Triangulator(genPolyArrFront);
        int[] triangledPoly = tri.Triangulate();

        //FrontFaceIndicies
        for (int i = 0; i < triangledPoly.Length; i++)
        {
            indicies.Add(triangledPoly[i] * FRONTOFFSET);
        }

        //BackFaceIndicies
        for (int i = triangledPoly.Length - 1; i >= 0; i--)
        {
            indicies.Add(triangledPoly[i] * FRONTOFFSET + (verts.Count / 2));
        }

        //Front-Back Faces normals
        for (int i = 0; i < indicies.Count / 2; i += 3)
        {
            int[] v1 = { indicies[i], indicies[(i + 1) % (indicies.Count / 2)], indicies[(i + 2) % (indicies.Count / 2)] };
            int[] v2 = { indicies[i + (indicies.Count / 2)], indicies[(i + 1) % (indicies.Count / 2) + (indicies.Count / 2)], indicies[(i + 2) % (indicies.Count / 2) + (indicies.Count / 2)] };

            GetNormalsForVerts(verts, v1);
            GetNormalsForVerts(verts, v2);
            GetUVsWithSize(verts, v1, Faces.forward, spriteSquareSize);
            GetUVsWithSize(verts, v2, Faces.forward, spriteSquareSize);
        }

        //Generating Side Triangles
        for (int i = 1; i < verts.Count / 2; i += 6)
        {
            int[] frontFaceVerts = { i, (i + 3) % (verts.Count / 2) };
            int[] backFaceVerts = { (i + (verts.Count / 2)), (i + 3) % (verts.Count / 2) + (verts.Count / 2) };

            //verts pos are used as uvs
            int[] uvCoord = { i, (i + 3) % (verts.Count / 2), (i + (verts.Count / 2)), (i + 3) % (verts.Count / 2) + (verts.Count / 2) };

            GetQuadIndicies(frontFaceVerts, backFaceVerts, indicies, verts);

            GetUVsWithSize(verts, uvCoord, Faces.left, spriteSquareSize);
        }

        //Generate Up-Down Verts
        for (int i = 5; i < verts.Count / 2; i += 6)
        {
            int[] frontFaceVerts = { i % (verts.Count / 2), (i + 3) % (verts.Count / 2) };
            int[] backFaceVerts = { (i % (verts.Count/2) + (verts.Count / 2)),
                                    (i + 3) % (verts.Count / 2) + (verts.Count / 2) };

            //verts pos are used as uvs
            int[] uvCoord = { i % (verts.Count / 2), (i + 3) % (verts.Count / 2), (i % (verts.Count / 2) + (verts.Count / 2)), (i + 3) % (verts.Count / 2) + (verts.Count / 2) };

            GetQuadIndicies(frontFaceVerts, backFaceVerts, indicies, verts);
            GetUVsWithSize(verts, uvCoord, Faces.up, spriteSquareSize);

        }

        generatedMesh = new MeshProperties(verts);
        generatedMesh.SetIndicies(indicies.ToArray());

        return generatedMesh;
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
