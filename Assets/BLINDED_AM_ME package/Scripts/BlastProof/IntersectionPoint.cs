using UnityEngine;

public class IntersectionPoint
{
    public Vector3 _pos;

    //TODO: find way to put them private
    public int _previousBoundaryPoint;
    public int _nextBoundaryPoint;

    public BoundaryPoint toBoundaryPoint()
    {
        return new BoundaryPoint(_pos);
    }

    public IntersectionPoint(Vector3 pos, int prevBp, int nextBp)
    {
        _pos = pos;
        _previousBoundaryPoint = prevBp;
        _nextBoundaryPoint = nextBp;
    }

    public static IntersectionPoint zero => new IntersectionPoint(new Vector3(0.0f, 0.0f, 0.0f), 0, 0);

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
