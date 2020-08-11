using UnityEngine;

public class Math
{
    //Verifies is 2 lines intersect and returns the intersection point in intersectionPoint parameter
    public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersectionPoint)
    {
        intersectionPoint = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f)
        {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
        {
            return false;
        }

        intersectionPoint.x = p1.x + u * (p2.x - p1.x);
        intersectionPoint.y = p1.y + u * (p2.y - p1.y);

        return true;
    }

    //Verifies if the point is in polygon including it's edges
    public static bool PointInPolygon(Vector2 point, in BoundaryPoint[] points)
    {
        float f = 0f;
        Vector2 zero = Vector2.zero;
        Vector2 vector2 = Vector2.zero;
        int length = points.Length;

        bool isOnEdgeLine = false;

        for (int i = 0; i < length; i++)
        {
            BoundaryPoint point2 = points[i];
            BoundaryPoint point3 = points[(i + 1) % length];
            zero.x = point2.m_pos.x - point.x;
            zero.y = point2.m_pos.y - point.y;
            vector2.x = point3.m_pos.x - point.x;
            vector2.y = point3.m_pos.y - point.y;
            f += Angle2D(zero.x, zero.y, vector2.x, vector2.y);        

            //Checks if intersects one of the boundary lines
            isOnEdgeLine = isPointOnLine(point, point2.m_pos, point3.m_pos) || isOnEdgeLine; 
        }
        return (Mathf.Abs(f) >= 3.141593f) || isOnEdgeLine;
    }

    //Verifies if a point on a line including the begin and end point of the line
    public static bool isPointOnLine(Vector2 pointToCheck, Vector2 lineBeginPoint, Vector2 lineEndPoint)
    {
        float segmentDistanceToPointToCheck = Vector2.Distance(lineBeginPoint, pointToCheck);
        float segmentDistanceFromPointToCheck = Vector2.Distance(pointToCheck, lineEndPoint);
        float totalDistance = Vector2.Distance(lineBeginPoint, lineEndPoint);

        if (Mathf.Abs(segmentDistanceToPointToCheck + segmentDistanceFromPointToCheck - totalDistance) <= Mathf.Epsilon ||
            pointToCheck == lineBeginPoint || pointToCheck == lineEndPoint
            )
            return true;

        return false;




       //Vector2 distanceBeginPointToCheck = pointToCheck - lineBeginPoint;
       //Vector2 distanceBeginEnd = lineEndPoint - lineBeginPoint;

       //Vector3 crossProdAlignedPoints = Vector3.Cross(distanceBeginPointToCheck, distanceBeginEnd);

       //bool arePointsAligned = crossProdAlignedPoints.x <= Mathf.Epsilon &&
       //                         crossProdAlignedPoints.y <= Mathf.Epsilon;                               

       //float dotProd1 = Vector2.Dot(distanceBeginPointToCheck, distanceBeginEnd);
       //float dotProd2 = Vector2.Dot(distanceBeginEnd, distanceBeginEnd);

       //if (dotProd1 < 0 || dotProd1 > dotProd2 || !arePointsAligned)
       //    return false;

       //return true;

        //float crossProd = (pointToCheck.y - lineBeginPoint.y) * (lineEndPoint.x - lineBeginPoint.x) -
        //                  (pointToCheck.x - lineBeginPoint.x) * (lineEndPoint.y - lineBeginPoint.y);

        //if (Mathf.Abs(crossProd) > Mathf.Epsilon)
        //    return false;

        //float dotProd = (pointToCheck.x - lineBeginPoint.x) * (lineEndPoint.x - lineBeginPoint.x) +
        //                (pointToCheck.y - lineBeginPoint.y) * (lineEndPoint.y - lineBeginPoint.y);

        //if (dotProd < 0.0f)
        //    return false;

        //float squaredLengthBa = (lineEndPoint.x - lineBeginPoint.x) * (lineEndPoint.x - lineBeginPoint.x) +
        //                        (lineEndPoint.y - lineBeginPoint.y) * (lineEndPoint.y - lineBeginPoint.y);
        //if (dotProd > squaredLengthBa)
        //    return false;

        //return true;
    }

    //Creates plane based on the line made by first and second intersection point and depth
    public static Plane CreateSlicePlane(Vector3 firstPoint, Vector3 secondPoint)
    {
        float distance = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 a = new Vector3(firstPoint.x, firstPoint.y, distance);
        Vector3 c = new Vector3(secondPoint.x, secondPoint.y, distance);
        Vector3 b = (a + c) / 2f;
        b.z = (a.z + c.z) * 0.75f;
        return new Plane(a, b, c);
    }

    public static float Angle2D(float x1, float y1, float x2, float y2)
    {
        float num3 = Mathf.Atan2(y2, x2) - Mathf.Atan2(y1, x1);
        while (num3 > 3.141593f)
        {
            num3 -= 6.283185f;
        }
        while (num3 < -3.141593f)
        {
            num3 += 6.283185f;
        }
        return (num3 * 57.29578f);
    }    
}
