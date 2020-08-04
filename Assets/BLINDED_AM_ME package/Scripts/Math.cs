using System.Collections;
using UnityEngine;

public class Math
{
    public unsafe static CustomBoundryBox.BoundaryPoint getLineLineIntersection(Vector2 startPoint, Vector2 endPoint, 
        CustomBoundryBox.BoundaryPoint bp1, CustomBoundryBox.BoundaryPoint bp2, Transform victim)
    {
        //float dx1 = endPoint.x - startPoint.x;
        //float dy1 = endPoint.y - startPoint.y;
        // float m1 = dy1 / dx1;
        // float c1 = startPoint.y - m1 * startPoint.x;

        //float dx2 = bp2.m_pos.x - bp1.m_pos.x;
        //float dy2 = bp2.m_pos.y - bp1.m_pos.y;      
        // float m2 = dy2 / dx2;
        // float c2 = (bp2.m_pos.y)-m2* bp2.m_pos.x;

        // float x = (c2 - c1) / (m1 - m2);
        // float y = m1 * x + c1;

        // if ((m1 - m2) == 0)

        //   float num = 0f;
        //   float num2 = 0f;
        //   float num3 = 0f;
        //   float num5 = 0f;        
        //   Vector2 vector = startPoint;
        //   Vector2 vector2 = endPoint;
        //   Vector2 vector3 = new Vector2(bp1.m_pos.x, bp1.m_pos.y);
        //   Vector2 vector4 = new Vector2(bp2.m_pos.x, bp2.m_pos.y);

        //   Vector2* vectorPtr1 = &vector2;
        //   vectorPtr1->x -= vector.x;
        //   Vector2* vectorPtr2 = &vector2;
        //   vectorPtr2->y -= vector.y;
        //   Vector2* vectorPtr3 = &vector3;
        //   vectorPtr3->x -= vector.x;
        //   Vector2* vectorPtr4 = &vector3;
        //   vectorPtr4->y -= vector.y;
        //   Vector2* vectorPtr5 = &vector4;
        //   vectorPtr5->x -= vector.x;
        //   Vector2* vectorPtr6 = &vector4;
        //   vectorPtr6->y -= vector.y;

        //   num = Mathf.Sqrt((vector2.x * vector2.x) + (vector2.y * vector2.y));
        //   num2 = vector2.x / num;
        //   num3 = vector2.y / num;
        //   vector3.y = (vector3.y * num2) - (vector3.x * num3);
        //   vector3.x = (vector3.x * num2) + (vector3.y * num3);
        //   vector4.y = (vector4.y * num2) - (vector4.x * num3);
        //   vector4.x = (vector4.x * num2) + (vector4.y * num3);    

        //float x = vector.x + (num5 * num2);
        //   float y = vector.y + (num5 * num3);     

        return CustomBoundryBox.BoundaryPoint.zero;


    }

    public static bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection)
    {
        intersection = Vector2.zero;

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

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }
}
