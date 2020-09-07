using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BlastProof
{
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

        /////////////////////////////////////////////////CUSTOM OVERLOARDS//////////////////////////////////////
        public static bool PointInPolygon(Vector2 point, in List<BoundaryPoint> points)
        {
            float f = 0f;
            Vector2 zero = Vector2.zero;
            Vector2 vector2 = Vector2.zero;
            int length = points.Count;

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
    }
}
