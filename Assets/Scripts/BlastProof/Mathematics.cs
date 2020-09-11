using System.Collections.Generic;
using UnityEngine;

namespace BlastProof
{
    public class Circle
    {
        private Vector3 _center;
        private float _radius;

        public float Radius { get => _radius; }
        public Vector3 Center { get => _center; }

        public Circle(Vector3 center, float radius)
        {
            _center = center;
            _radius = radius;
        }

        public List<IntersectionPoint> GetIntersections(Vector2[] polygon)
        {
            List<IntersectionPoint> interPoints = new List<IntersectionPoint>();
            for (int i=0;i<polygon.Length;i++)
            {
                Vector2 interPos;
                int j = (i + 1) % polygon.Length;
                if (Mathematics.CircleIntersectsSegment(_center,_radius,polygon[i],polygon[j],out interPos))
                {
                    interPoints.Add(new IntersectionPoint(interPos, i, j));
                }
            }
            return interPoints;
        }
        public List<Vector3[]> GetIntersections2(Vector2[] polygon)
        {
            List<Vector3[]> interPoints = new List<Vector3[]>();
            Vector3[] interPos = null;
            for (int i = 0; i < polygon.Length; i++)
            {               
                int j = (i + 1) % polygon.Length;
                interPos = Mathematics.SegmentIntersectCircle(polygon[i], polygon[j], _center, _radius);
                interPoints.Add(interPos);
                // interPoints.Add(new IntersectionPoint(interPos, i, j));
                
            }
            return interPoints;
        }

        public void UpdatePosition(Vector3 position)
        {
            _center = position;
        }
    }

    public static class Mathematics
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

        public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            Vector3 rhs = linePoint2 - linePoint1;
            Vector3 lhs = point - linePoint1;
            return ((Vector3.Dot(lhs, rhs) <= 0f) ? 1 : ((lhs.magnitude > rhs.magnitude) ? 2 : 0));
        }

        public static Vector3 ProjectPointOnLine(Vector3 linePoint, Vector3 lineVec, Vector3 point)
        {
            float num = Vector3.Dot(point - linePoint, lineVec);
            return (linePoint + (lineVec * num));
        }

        public static Vector3 ProjectPointOnLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point)
        {
            Vector3 vector = linePoint2 - linePoint1;
            Vector3 vector2 = ProjectPointOnLine(linePoint1, vector.normalized, point);
            int num = PointOnWhichSideOfLineSegment(linePoint1, linePoint2, vector2);
            return ((num != 0) ? ((num != 1) ? ((num != 2) ? Vector3.zero : linePoint2) : linePoint1) : vector2);
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
                lhs = (lhs / magnitude);
            }
            float num2 = Mathf.Clamp(Vector2.Dot(lhs, rhs), 0f, magnitude);
            return (lineStart + ((lhs * num2)));
        }

        public static float ClosestDistanceToPolygon(in Vector2[] verts, in Vector2 point, ref KeyValuePair<int, int> edgeVerts)
        {
            int nvert = verts.Length;
            int i, j = 0;
            float minDistance = Mathf.Infinity;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {                
                    float tempMin = minDistance;
                    float distance = DistancePointLine2D(point, verts[i], verts[j]);
                    minDistance = Mathf.Min(minDistance, distance);
                    if (minDistance != tempMin)
                    {
                        edgeVerts = new KeyValuePair<int, int>(i, j);
                    }
                
            }

            return minDistance;
        }

        public static float ClosestDistanceToPolygon(in Vector2[] verts, in Vector2 point)
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

        public static bool IsInsidePolygon(in Vector2[] vertices, in Vector2 checkPoint, float margin = 0.000001f)
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

        public static bool IsInsidePolygon(int nvert, in float[] vertx, in float[] verty, in float testx, float testy)
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

        public static bool IsVectorsAproximately(Vector2 v1, Vector2 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y);
        }
        public static float nfmod(float a, float b)
        {
            return a - b * Mathf.Floor(a / b);
        }

        public static Vector3[] SegmentIntersectCircle(Vector3 p1, Vector3 p2, Vector3 center, float radius)
        {
            Vector3 dp = new Vector3();
            Vector3[] sect;
            float a, b, c;
            float bb4ac;
            float mu1;
            float mu2;

            //  get the distance between X and Z on the segment
            dp.x = p2.x - p1.x;
            dp.y = p2.y - p1.y;
            //   I don't get the math here
            a = dp.x * dp.x + dp.y * dp.y;
            b = 2 * (dp.x * (p1.x - center.x) + dp.y * (p1.y - center.y));
            c = center.x * center.x + center.y * center.y;
            c += p1.x * p1.x + p1.y * p1.y;
            c -= 2 * (center.x * p1.x + center.y * p1.y);
            c -= radius * radius;
            bb4ac = b * b - 4 * a * c;
            if (Mathf.Abs(a) < float.Epsilon || bb4ac < 0)
            {
                //  line does not intersect
                return null;
            }
            mu1 = (-b + Mathf.Sqrt(bb4ac)) / (2 * a);
            mu2 = (-b - Mathf.Sqrt(bb4ac)) / (2 * a);
            sect = new Vector3[2];
            sect[0] = new Vector3(p1.x + mu1 * (p2.x - p1.x),p1.y + mu1 * (p2.y - p1.y));
            sect[1] = new Vector3(p1.x + mu2 * (p2.x - p1.x),p1.y + mu2 * (p2.y - p1.y));

            return sect;
        }

        /////////////////////////////////////////////////CUSTOM OVERLOARDS//////////////////////////////////////
        public static bool PointInPolygon(Vector2 point, Vector2[] points)
        {
            float f = 0f;
            Vector2 zero = Vector2.zero;
            Vector2 vector2 = Vector2.zero;
            int length = points.Length;

            for (int i = 0; i < length; i++)
            {
                Vector2 point2 = points[i];
                Vector2 point3 = points[(i + 1) % length];
                zero.x = point2.x - point.x;
                zero.y = point2.y - point.y;
                vector2.x = point3.x - point.x;
                vector2.y = point3.y - point.y;
                f += Angle2D(zero.x, zero.y, vector2.x, vector2.y);

                //Checks if intersects one of the boundary lines

            }
            return (Mathf.Abs(f) >= 3.141593f);
        }

        public static bool IsInsidePolygon(int nvert, in List<BoundaryPoint> bp, in float testx, float testy)
        {
            bool c = false;
            int i, j = 0;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if ((((bp[i].m_pos.y <= testy) && (testy < bp[j].m_pos.y)) ||

                     ((bp[j].m_pos.y <= testy) && (testy < bp[i].m_pos.y))) &&

                    (testx < (bp[j].m_pos.x - bp[i].m_pos.x) * (testy - bp[i].m_pos.y) / (bp[j].m_pos.y - bp[i].m_pos.y) + bp[i].m_pos.x))
                    c = !c;
            }
            return c;
        }

        public static float[] ClosestDistanceToPolygon(in List<BoundaryPoint> verts, Vector2[] point, float margin = 0.000001f)
        {
            int nvert = verts.Count;
            int i, j = 0;
            float[] minDistance = new float[point.Length];

            for (int k = 0; k < minDistance.Length; k++)
            {
                minDistance[k] = Mathf.Infinity;
            }

            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                int trueCount = 0;
                for (int k = 0; k < point.Length; k++)
                {
                    if (minDistance[k] >= margin)
                    {
                        float distance = DistancePointLine2D(point[k], verts[i].m_pos, verts[j].m_pos);
                        minDistance[k] = Mathf.Min(minDistance[k], distance);
                    }
                    else
                    {
                        trueCount++;
                    }
                }

                if (trueCount == 3)
                {
                    break;
                }
            }

            return minDistance;
        }

        public static bool[] IsInsidePolygon(in List<BoundaryPoint> vertices, in Vector2[] checkPoint, float margin = 0.000001f)
        {
            bool[] isCloseEnough = new bool[checkPoint.Length];

            for (int i = 0; i < isCloseEnough.Length; i++)
            {
                isCloseEnough[i] = IsInsidePolygon(vertices.Count, vertices, checkPoint[i].x, checkPoint[i].y);
            }
            for (int i = 0; i < isCloseEnough.Length; i++)
            {
                if (isCloseEnough[i] != true)
                {
                    isCloseEnough[i] = ClosestDistanceToPolygon(vertices, checkPoint, margin)[i] < margin;
                }
            }
            return isCloseEnough;
        }

        public static Matrix4x4 ScaleMatrix (float scaleFactor)
        {
            Matrix4x4 _scaleMatrix = new Matrix4x4();
            _scaleMatrix.m00 = scaleFactor;
            _scaleMatrix.m11 = scaleFactor;
            _scaleMatrix.m22 = scaleFactor;
            _scaleMatrix.m33 = 1.0f;
            return _scaleMatrix;
        }
        public static Matrix4x4 TranslateMatrix(Vector3 translVec)
        {
            Matrix4x4 _transMatrix = new Matrix4x4();
            _transMatrix.m00 = 1.0f;
            _transMatrix.m11 = 1.0f;
            _transMatrix.m22 = 1.0f;
            _transMatrix.m33 = 1.0f;
            _transMatrix.m03 = translVec.x;
            _transMatrix.m13 = translVec.y;
            _transMatrix.m23 = translVec.z;
            return _transMatrix;
        }

        public static bool CircleIntersectsSegment(Vector2 circlePoint, float circleRadius, Vector2 segmentPoint1, Vector2 segmentPoint2, out Vector2 intersection)
        {
            intersection = NearestPoint(segmentPoint1, segmentPoint2, circlePoint);
            float num = circleRadius * circleRadius;
            if (!Between3f(intersection.x, segmentPoint1.x, segmentPoint2.x) || (!Between3f(intersection.y, segmentPoint1.y, segmentPoint2.y) || (DistanceSquared(intersection, circlePoint) > num)))
            {
                if (DistanceSquared(circlePoint, segmentPoint1) <= num)
                {
                    intersection = segmentPoint1;
                    return true;
                }
                if (DistanceSquared(circlePoint, segmentPoint2) > num)
                {
                    return false;
                }
                intersection = segmentPoint2;
            }
            return true;
        }
        public static float DistanceSquared(Vector2 point1, Vector2 point2)
        {
            float num = point2.x - point1.x;
            float num2 = point2.y - point1.y;
            return ((num * num) + (num2 * num2));
        }

        public static Vector2 NearestPoint(Vector2 point1, Vector2 point2, Vector2 otherPoint)
        {
            float f = point2.x - point1.x;
            float num2 = point2.y - point1.y;
            if (Mathf.Abs(f) < 1E-05f)
            {
                return new Vector2(point1.x, otherPoint.y);
            }
            if (Mathf.Abs(num2) < 1E-05f)
            {
                return new Vector2(otherPoint.x, point1.y);
            }
            float num3 = num2 / f;
            float x = (((-point1.y + (num3 * point1.x)) + otherPoint.y) + (otherPoint.x / num3)) / (num3 + (1f / num3));
            return new Vector2(x, ((num3 * x) + point1.y) - (num3 * point1.x));
        }
        public static bool Between3f(float f1, float f2, float f3) =>
           (((f2 > f1) || (f1 > f3)) ? ((f3 <= f1) && (f1 <= f2)) : true);

    }
}
