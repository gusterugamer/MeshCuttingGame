using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshProperties
{
    public Vector3[] mesh_vertices;
    public Vector3[] mesh_normals;
    public Vector2[] mesh_uvs;
    public Vector4[] mesh_tangents;
    public int[] mesh_indicies;

    public MeshProperties()
    {

    }

    public MeshProperties(List<VertexProperties> verts = null)
    {
        mesh_vertices = new Vector3[verts.Count];
        mesh_normals = new Vector3[verts.Count];
        mesh_uvs = new Vector2[verts.Count];

        for(int i=0;i<verts.Count;i++)
        {
            mesh_vertices[i] = verts[i].position;
            mesh_normals[i] = verts[i].normal;
            mesh_uvs[i] = verts[i].uv;
        }
    }

    public void SetIndicies(int[] indicies)
    {
        mesh_indicies = indicies;
    }
}
