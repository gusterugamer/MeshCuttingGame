using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditorInternal;
using UnityEngine;

public class IntersectionPoint
{
    public Vector2 _pos;

    private BoundaryPoint _previousBoundaryPoint;
    private BoundaryPoint _nextBoundaryPoint;

    public BoundaryPoint toBoundaryPoint()
    {
        return new BoundaryPoint(_pos);
    }

    public IntersectionPoint(Vector2 pos)
    {
        _pos = pos;
    }

    public static IntersectionPoint zero => new IntersectionPoint(new Vector2(0.0f, 0.0f));

    public static bool operator !=(IntersectionPoint point1, IntersectionPoint point2)
    {
        return point1._pos != point2._pos;
    }

    public static bool operator ==(IntersectionPoint point1, IntersectionPoint point2)
    {
        return point1._pos == point2._pos;
    }
    public override bool Equals(object obj) =>
           ((obj != null) ? ReferenceEquals(this, obj) : true);

    public override int GetHashCode() =>
            base.GetHashCode();
}
