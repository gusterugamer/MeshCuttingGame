using PrimitivesPro.MeshCutting;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshProperties CreateMesh(List<BoundaryPoint> genPoly)
    {
        MeshProperties generatedMesh = new MeshProperties();

        Vector2[] genPolyArrFront = new Vector2[genPoly.Count];

        for (int i=0;i<genPoly.Count;i++)
        {
            genPolyArrFront[i] = genPoly[i].m_pos;          
        }
        Triangulator tri = new Triangulator(genPolyArrFront);

        int[] triangledPoly = tri.Triangulate();

        Vector3[] meshVerts = new Vector3[2 * genPoly.Count];

        for(int i=0;i<genPoly.Count;i++)
        {
            meshVerts[i] = genPolyArrFront[i];
        }
        for(int i= 0;i<genPoly.Count;i++)
        {
            meshVerts[i+genPoly.Count] = new Vector3(genPolyArrFront[i].x, genPolyArrFront[i].y, 1.0f);
        }        
        List<int> indicies = new List<int>(triangledPoly);

        for(int i=0;i<triangledPoly.Length;i++)
        {
            indicies.Add(triangledPoly[i] + (genPolyArrFront.Length));
        }

        for(int i=0;i<genPoly.Count; i++)
        {
            int[] frontFace= { i , (i + 1)%(genPoly.Count)};
            int[] backFace = { (i + genPoly.Count), (i + 1) % genPoly.Count + genPoly.Count };
            GetQuadIndicies(frontFace, backFace, ref indicies);
        }

        generatedMesh.mesh_vertices = meshVerts;
        generatedMesh.mesh_indicies = indicies.ToArray();

        return generatedMesh;
    }

    public static void GetQuadIndicies(int[] frontFaceIndicies, int[] backFaceIndicies, ref List<int> indicies)
    {       
        indicies.Add(frontFaceIndicies[0]);
        indicies.Add(backFaceIndicies[0]);
        indicies.Add(frontFaceIndicies[1]);
        indicies.Add(frontFaceIndicies[1]);
        indicies.Add(backFaceIndicies[0]);
        indicies.Add(backFaceIndicies[1]);      
    }
    
}
