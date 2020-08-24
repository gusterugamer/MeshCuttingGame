using System.Collections.Generic;
using UnityEngine;

public class Mathematics
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

    public static float[] DistancePointLine2D(Vector2[] points, Vector2 lineStart, Vector2 lineEnd)
    {
        float[] results = new float[points.Length];

        for(int i=0;i<results.Length;i++)
        {
            results[i] = (ProjectPointLine2D(points[i], lineStart, lineEnd) - points[i]).magnitude;
        }
        return results;
    }
    public static Vector2 ProjectPointLine2D(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 rhs = point - lineStart;
        Vector2 vector2 = lineEnd - lineStart;
        float magnitude = vector2.magnitude;
        Vector2 lhs = vector2;
        if (magnitude > 1E-06f)
        {
            lhs = (Vector2)(lhs / magnitude);
        }
        float num2 = Mathf.Clamp(Vector2.Dot(lhs, rhs), 0f, magnitude);
        return (lineStart + ((Vector2)(lhs * num2)));
    }

    //public static float ClosestDistanceToPolygon(in Vector2[] verts, in Vector2 point)
    //{
    //    int nvert = verts.Length;
    //    int i, j = 0;
    //    float minDistance = Mathf.Infinity;
    //    for (i = 0, j = nvert - 1; i < nvert; j = i++)
    //    {
    //        float distance = DistancePointLine2D(point, verts[i], verts[j]);
    //        minDistance = Mathf.Min(minDistance, distance);
    //    }

    //    return minDistance;
    //}

    //public static bool IsInsidePolygon(in Vector2[] vertices, in Vector2 checkPoint, float margin = 0.000001f)
    //{
    //    if (ClosestDistanceToPolygon(vertices, checkPoint) < margin)
    //    {
    //        return true;
    //    }

    //    float[] vertX = new float[vertices.Length];
    //    float[] vertY = new float[vertices.Length];
    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        vertX[i] = vertices[i].x;
    //        vertY[i] = vertices[i].y;
    //    }

    //    return IsInsidePolygon(vertices.Length, vertX, vertY, checkPoint.x, checkPoint.y);
    //}

    public static void IsInsidePolygon(int nvert, in float[] vertx, in float[] verty, in Vector2[] checkPoints, ref bool[] isInMargin)
    {       
        int i, j = 0;
        for (i = 0, j = nvert - 1; i < nvert; j = i++)
        {
            for (int k = 0; k < checkPoints.Length; k++)
            {
                if (!isInMargin[k])
                {
                    if ((((verty[i] <= checkPoints[k].y) && (checkPoints[k].y < verty[j])) ||

                         ((verty[j] <= checkPoints[k].y) && (checkPoints[k].y < verty[i]))) &&

                        (checkPoints[k].x < (vertx[j] - vertx[i]) * (checkPoints[k].y - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
                    {
                        isInMargin[k] = !isInMargin[k];
                    }
                }
            }
        }       
    }

    /////////////////////////////////////////////////CUSTOM OVERLOARDS//////////////////////////////////////
    public static float[] ClosestDistanceToPolygon(in List<BoundaryPoint> verts,in Vector2[] points)
    {        
        int nvert = verts.Count;
        int i, j = 0;
        float[] minDistance = new float[points.Length];

        for(int k=0;k<minDistance.Length;k++)
        {
            minDistance[k] = Mathf.Infinity;
        }

        for (i = 0, j = nvert - 1; i < nvert; j = i++)
        {
            float[] distance = DistancePointLine2D(points, verts[i].m_pos, verts[j].m_pos);
           minDistance[0] = Mathf.Min(minDistance[0], distance[0]);
           minDistance[1] = Mathf.Min(minDistance[1], distance[1]);
           minDistance[2] = Mathf.Min(minDistance[2], distance[2]);
        }

        return minDistance;
    }

    public static bool[] IsInsidePolygon(in List<BoundaryPoint> vertices, in Vector2[] checkPoints, float margin = 0.00001f)
    {
        bool[] isInMargin = new bool[checkPoints.Length];
        float[] cdtp= ClosestDistanceToPolygon(vertices, checkPoints);

        for (int i=0;i<cdtp.Length;i++)
        {
            isInMargin[i] = cdtp[i] < margin;
        }

        float[] vertX = new float[vertices.Count];
        float[] vertY = new float[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            vertX[i] = vertices[i].m_pos.x;
            vertY[i] = vertices[i].m_pos.y;
        }

        IsInsidePolygon(vertices.Count, vertX, vertY, checkPoints, ref isInMargin);

        return isInMargin;
    }
}
