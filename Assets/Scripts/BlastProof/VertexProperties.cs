using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class VertexProperties
{
    public Vector3 position;
    public Vector3 normal;
    public Vector2 uv;
    public Vector4 tangent;

    public static bool operator ==(VertexProperties vp1, VertexProperties vp2)
    {
        return vp1.position == vp2.position &&
               vp1.normal == vp2.normal &&
               vp1.uv == vp2.uv &&
               vp1.tangent == vp2.tangent;
    }

    public static bool operator !=(VertexProperties vp1, VertexProperties vp2)
    {
        return vp1.position != vp2.position ||
               vp1.normal != vp2.normal ||
               vp1.uv != vp2.uv ||
               vp1.tangent != vp2.tangent;
    }

    public override bool Equals(object obj) =>
          ((obj != null) ? ReferenceEquals(this, obj) : true);

    public override int GetHashCode() =>
            base.GetHashCode();
}
