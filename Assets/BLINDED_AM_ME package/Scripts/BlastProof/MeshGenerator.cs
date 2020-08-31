using PrimitivesPro.MeshCutting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshProperties CreateMesh(List<BoundaryPoint> genPoly, Transform objTransform)
    {      
        const int FRONTOFFSET = 3;
        const int UPOFFSET = 4;
        const int LEFTOFFSET = 5;

        MeshProperties generatedMesh;

        Vector2[] genPolyArrFront = new Vector2[genPoly.Count];

        List<VertexProperties> verts = new List<VertexProperties>();

        List<Vector3> normals = new List<Vector3>();

        List<int> indicies = new List<int>();
        
        //ConvertingToArray
        for (int i = 0; i < genPoly.Count; i++)
        {
            genPolyArrFront[i] = genPoly[i].m_pos;
        }

        //VerticiesFront
        for (int i=0;i< genPoly.Count;i++)
        {
            verts.Add(new VertexProperties { position = new Vector3 (genPoly[i].m_pos.x, genPoly[i].m_pos.y, 0.0f) });
            verts.Add(new VertexProperties { position = new Vector3 (genPoly[i].m_pos.x, genPoly[i].m_pos.y, 0.0f) });
            verts.Add(new VertexProperties { position = new Vector3 (genPoly[i].m_pos.x, genPoly[i].m_pos.y, 0.0f) });
        }

        //VerticiesBack
        for (int i = 0; i < genPoly.Count; i++)
        {
            verts.Add(new VertexProperties { position = new Vector3(genPoly[i].m_pos.x, genPoly[i].m_pos.y, 1.0f)});
            verts.Add(new VertexProperties { position = new Vector3(genPoly[i].m_pos.x, genPoly[i].m_pos.y, 1.0f)});
            verts.Add(new VertexProperties { position = new Vector3(genPoly[i].m_pos.x, genPoly[i].m_pos.y, 1.0f)});
        }       

        Triangulator tri = new Triangulator(genPolyArrFront);
        int[] triangledPoly = tri.Triangulate();

        //FrontFaceIndicies
        for (int i=0;i<triangledPoly.Length;i++)
        {
            indicies.Add(triangledPoly[i] * FRONTOFFSET);
        }

        //BackFaceIndicies
        for (int i = triangledPoly.Length-1; i >=0; i--)
        {
            indicies.Add(triangledPoly[i] * FRONTOFFSET + (verts.Count/2));
        }

        //Front-Back Faces normals
        for (int i=0;i<indicies.Count;i+=3)
        {
            int[] v = { indicies[i], indicies[i + 1], indicies[i + 2] };
            GetNormalsForVerts(verts, v);
        }

        //Generating Side Triangles
        for (int i=1;i<verts.Count/2;i+=6)
        {
            int[] frontFaceVerts = {i, (i + 3) % (verts.Count/2) };
            int[] backFaceVerts = { (i + (verts.Count / 2)), (i + 3) % (verts.Count / 2) + (verts.Count / 2) };

            GetQuadIndicies(frontFaceVerts, backFaceVerts, indicies, verts);
        }

        //Generate Up-Down Verts
        for (int i = 5; i < verts.Count / 2; i += 6)
        {
            int[] frontFaceVerts = { i % (verts.Count/2), (i + 3) % (verts.Count / 2) };
            int[] backFaceVerts = { (i % (verts.Count/2) + (verts.Count / 2)), 
                                    (i + 3) % (verts.Count / 2) + (verts.Count / 2) };

            GetQuadIndicies(frontFaceVerts, backFaceVerts, indicies, verts);
        }

        generatedMesh = new MeshProperties(verts);
        generatedMesh.SetIndicies(indicies.ToArray());

        return generatedMesh;
    }

    public static void GetQuadIndicies(int[] frontFaceIndicies, int[] backFaceIndicies, List<int> indicies,List<VertexProperties> verts)
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
}
