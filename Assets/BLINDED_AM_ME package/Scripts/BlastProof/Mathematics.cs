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

    public static float DistancePointLine2D(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        return (ProjectPointLine2D(point, lineStart, lineEnd) - point).magnitude;
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

    public static float ClosestDistanceToPolygon(Vector2[] verts, Vector2 point)
    {
        int nvert = verts.Length;
        int i, j = 0;
        float minDistance = Mathf.Infinity;
        for (i = 0, j = nvert - 1; i < nvert; j = i++)
        {
            float distance = DistancePointLine2D(point, verts[i], verts[j]);
            minDistance = Mathf.Min(minDistance, distance);
        }

        return minDistance;
    }

    public static bool IsInsidePolygon(Vector2[] vertices, Vector2 checkPoint, float margin = 0.000001f)
    {
        if (ClosestDistanceToPolygon(vertices, checkPoint) < margin)
        {
            return true;
        }

        float[] vertX = new float[vertices.Length];
        float[] vertY = new float[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertX[i] = vertices[i].x;
            vertY[i] = vertices[i].y;
        }

        return IsInsidePolygon(vertices.Length, vertX, vertY, checkPoint.x, checkPoint.y);
    }

    public static bool IsInsidePolygon(int nvert, float[] vertx, float[] verty, float testx, float testy)
    {
        bool c = false;
        int i, j = 0;
        for (i = 0, j = nvert - 1; i < nvert; j = i++)
        {
            if ((((verty[i] <= testy) && (testy < verty[j])) ||

                 ((verty[j] <= testy) && (testy < verty[i]))) &&

                (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
                c = !c;
        }
        return c;
    }

    /////////////////////////////////////////////////CUSTOM OVERLOARDS//////////////////////////////////////
    public static float ClosestDistanceToPolygon(in List<BoundaryPoint> verts, Vector2 point)
    {
        int nvert = verts.Count;
        int i, j = 0;
        float minDistance = Mathf.Infinity;
        for (i = 0, j = nvert - 1; i < nvert; j = i++)
        {
            float distance = DistancePointLine2D(point, verts[i].m_pos, verts[j].m_pos);
            minDistance = Mathf.Min(minDistance, distance);
        }

        return minDistance;
    }

    public static bool IsInsidePolygon(in List<BoundaryPoint> vertices, Vector2 checkPoint, float margin = 0.000001f)
    {
        if (ClosestDistanceToPolygon(vertices, checkPoint) < margin)
        {
            return true;
        }

        float[] vertX = new float[vertices.Count];
        float[] vertY = new float[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            vertX[i] = vertices[i].m_pos.x;
            vertY[i] = vertices[i].m_pos.y;
        }

        return IsInsidePolygon(vertices.Count, vertX, vertY, checkPoint.x, checkPoint.y);
    }
}
