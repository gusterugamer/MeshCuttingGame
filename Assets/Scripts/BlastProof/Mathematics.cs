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
                    Vector2 projectedPoint = Mathematics.ProjectPointLine2D(interPos, polygon[i], polygon[j]);
                    interPoints.Add(new IntersectionPoint(projectedPoint, i, j));
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
        #region Conversion

        /// <summary>
        /// Convert degrees to radians.
        /// </summary>
        /// <returns>radians.</returns>
        /// <param name="degrees">angle</param>
        public static float DegreesToRadians(float degrees)
        {
            return degrees * 0.01745329f;
        }

        /// <summary>
        /// Convert radians to degrees.
        /// </summary>
        /// <returns>degrees.</returns>
        /// <param name="radians">angle</param>
        public static float RadiansToDegrees(float rads)
        {
            return rads * 57.2957795f;
        }

        /// <summary>
        /// Convert a value within a scope into another scope. Example 5 between 0 and 10 equals 0.5 between 0 and 1
        /// </summary>
        /// <returns>.</returns>
        /// <param name="value">The value within a scope.</param>
        /// <param name="oldMin">The minimum of the current scope.</param>
        /// <param name="oldMax">The maximum of the current scope.</param>
        /// <param name="newMin">The minimum of the new scope.</param>
        /// <param name="newMax">The maximum of the new scope.</param>
        public static float Value_from_another_Scope(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            // one scoped value to another scope
            // values need to be floats
            //  a <= x <= b 
            // 	0 <= (x-a)/(b-a) <=1
            // 	new_value = ( (old_value - old_min) / (old_max - old_min) ) * (new_max - new_min) + newmin

            return ((value - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
        }

        #endregion

        #region 2D

        /// <summary>
        /// Returns a 2D vector direction going counter clockwise from (1,0)
        /// </summary>
        /// <returns> 2D vector </returns>
        /// <param name="angleDegrees">Angle in degrees.</param>
        public static Vector2 AngleToVector2D(float angleDegrees)
        {
            return new Vector2(
                Mathf.Cos(DegreesToRadians(angleDegrees)),
                Mathf.Sin(DegreesToRadians(angleDegrees))
            );

        }

        #endregion

        #region MISC

        /// <summary>
        /// Rotate a vector point or direction around point(0,0,0) by the given axis.
        /// </summary>
        /// <returns>.</returns>
        /// <param name="vector">point or direction.</param>
        /// <param name="axis">roll, pitch, and yaw.</param>
        /// <param name="angleDegrees">Angle degrees.</param>
        public static Vector3 Rotate_Vector(Vector3 vector, Vector3 axis, float angleDegrees)
        {

            // Rodrigues' rotation formula uses radians
            // Vector3 newDir = Mathf.Cos(angle) * dir + Mathf.Sin(angle) * Vector3.Cross(axis, dir) + (1.0f - Mathf.Cos(angle)) * Vector3.Dot(axis,dir) * axis;

            // may also use
            // Quaternion.AngleAxis(angleDegrees, axis) * dir

            return Mathf.Cos(DegreesToRadians(angleDegrees)) * vector
                + Mathf.Sin(DegreesToRadians(angleDegrees)) * Vector3.Cross(axis, vector)
                + (1.0f - Mathf.Cos(DegreesToRadians(angleDegrees))) * Vector3.Dot(axis, vector) * axis;

        }


        public static Vector3 Point_In_Triangle(Vector2 uv, Vector3 point1, Vector3 point2, Vector3 point3)
        {

            //point = (1 - sqrt(u)) * A + (sqrt(u) * (1 - v)) * B + (sqrt(u) * v) * C

            Vector3 point = (1.0f - Mathf.Sqrt(uv.x)) * point1;
            point += (Mathf.Sqrt(uv.x) * (1.0f - uv.y)) * point2;
            point += (Mathf.Sqrt(uv.x) * uv.y) * point3;

            return point;

        }

        public static Vector3 ScaleIndirection(Vector3 origScale, float scaling, Vector3 normal)
        {

            return origScale + (scaling - 1.0f) * Vector3.Project(origScale, normal);

        }

        public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
        {
            // from Standard Assets
            // comments are no use here... it's the catmull-rom equation.
            // Un-magic this, lord vector!
            return 0.5f * ((2f * p1) + (-p0 + p2) * i + (2f * p0 - 5f * p1 + 4f * p2 - p3) * i * i + (-p0 + 3f * p1 - 3f * p2 + p3) * i * i * i);
        }

        #endregion

        #region Trajectory

        // https://en.wikipedia.org/wiki/Trajectory_of_a_projectile

        /// <summary>
        /// if returns false, increase initSpeed
        /// </summary>
        public static bool Trajectory_Can_Hit_Point(Vector2 point, float initSpeed, float gravity = 9.81f)
        {

            if (initSpeed * initSpeed * initSpeed * initSpeed -
                gravity * (gravity * point.x * point.x + 2.0f * point.y * initSpeed * initSpeed) >= 0)
                return true;
            else
                return false;
        }

        /// <summary>
        /// returns the higher angle
        /// </summary>
        public static float Trajectory_Find_Needed_Angle1(Vector2 point, float initSpeed = 1.0f, float gravity = 9.81f)
        {
            return RadiansToDegrees(
                Mathf.Atan(
                (Mathf.Pow(initSpeed, 2.0f) + Mathf.Sqrt(Mathf.Pow(initSpeed, 4.0f) - (gravity * ((gravity * Mathf.Pow(point.x, 2.0f)) + (2.0f * point.y * Mathf.Pow(initSpeed, 2.0f))))))
                / (gravity * point.x))
            );
        }

        /// <summary>
        /// returns the lower angle
        /// </summary>
        public static float Trajectory_Find_Needed_Angle2(Vector2 point, float initSpeed = 1.0f, float gravity = 9.81f)
        {
            return RadiansToDegrees(
                Mathf.Atan(
                (Mathf.Pow(initSpeed, 2.0f) - Mathf.Sqrt(Mathf.Pow(initSpeed, 4.0f) - (gravity * ((gravity * Mathf.Pow(point.x, 2.0f)) + (2.0f * point.y * Mathf.Pow(initSpeed, 2.0f))))))
                / (gravity * point.x))
            );
        }

        public static float Trajectory_Horizontal_Distance(float initSpeed = 1.0f, float gravity = 9.81f, float angleDegrees = 45.0f, float initHeight = 0.0f)
        {


            if (initHeight < 0)
            {
                initHeight = 0;
                Debug.LogError("initHeight needs to be greater than 0");
                return 0.0f;
            }

            return (initSpeed * Mathf.Cos(DegreesToRadians(angleDegrees))) / gravity
                * (
                    (initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) +
                    Mathf.Sqrt(
                        (initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) *
                        (initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) +
                        2.0f * gravity * initHeight
                    )
                );
        }

        public static float Trajectory_Time_of_Flight(float initSpeed = 1.0f, float angleDegrees = 45.0f, float horDistance = 1.0f)
        {

            return horDistance / (initSpeed * Mathf.Cos(DegreesToRadians(angleDegrees)));
        }

        public static float Trajectory_Time_of_Flight(float initSpeed = 1.0f, float gravity = 9.81f, float angleDegrees = 45.0f, float initHeight = 0.0f)
        {

            if (initHeight < 0)
            {
                initHeight = 0;
                Debug.LogError("initHeight needs to be greater than 0");
                return 0.0f;
            }

            return (
                (initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) +
                Mathf.Sqrt(
                    (initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) *
                    (initSpeed * Mathf.Sin(DegreesToRadians(angleDegrees))) +
                    2.0f * gravity * initHeight
                )
            ) / gravity;
        }

        /// <summary>
        /// angle needed to reach horizontal distance
        /// </summary>
        public static float Trajectory_Angle_of_Reach(float initSpeed = 1.0f, float gravity = 9.81f, float horDistance = 1.0f)
        {

            return 0.5f * Mathf.Asin(gravity * horDistance / (initSpeed * initSpeed));
        }


        public static float Trajectory_Height_at_HorDistance(float initSpeed = 1.0f, float gravity = 9.81f, float angleDegrees = 45.0f, float horDistance = 1.0f, float initHeight = 0.0f)
        {

            if (initHeight < 0)
            {
                initHeight = 0;
                Debug.LogError("initHeight needs to be greater than 0");
                return 0.0f;
            }

            return initHeight + horDistance * Mathf.Tan(DegreesToRadians(angleDegrees)) -
                ((gravity * horDistance * horDistance) / (2 * Mathf.Pow(initSpeed * Mathf.Cos(DegreesToRadians(angleDegrees)), 2)));
        }


        public static Vector3[] Trajectory_Predicted_Path(Vector3 startPoint, Vector3 initVelocity, Vector3 gravity, float initHeight = 10.0f, int numIterations = 10)
        {

            if (initHeight < 0)
            {
                initHeight = 0;
                Debug.LogError("initHeight needs to be greater than 0");
                return new Vector3[] { startPoint };
            }

            Vector3 up = -gravity.normalized;
            Vector3 right = Vector3.Cross(initVelocity.normalized, up).normalized;
            right = Vector3.Cross(up, right);

            float initSpeed = initVelocity.magnitude;
            float angleDegrees = Vector3.Angle(initVelocity, right) * (Vector3.Dot(initVelocity, up) < 0 ? -1 : 1);
            float gravityAcc = gravity.magnitude;
            float horDistance = Trajectory_Horizontal_Distance(initSpeed, gravityAcc, angleDegrees, initHeight);

            Vector3[] path = new Vector3[numIterations];

            float normX = 0.0f;

            for (int i = 1; i <= numIterations; i++)
            {

                normX = (float)i / (float)numIterations;

                path[i - 1] = startPoint + right * horDistance * normX;
                path[i - 1] += up * (Trajectory_Height_at_HorDistance(initSpeed, gravityAcc, angleDegrees, horDistance * normX, initHeight) - initHeight);
            }

            return path;
        }

        #endregion


        public static float PolygonArea(Vector2[] points)
        {
            float f = 0f;
            int length = points.Length;
            for (int i = 0; i < length; i++)
            {
                Vector2 point = points[i];
                Vector2 point2 = points[(i + 1) % length];
                f += (point.x * point2.y) - (point2.x * point.y);
            }
            return (Mathf.Abs(f) / 2f);
        }

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
        public static float PolygonArea(List<BoundaryPoint> points)
        {
            float f = 0f;
            int length = points.Count;
            for (int i = 0; i < length; i++)
            {
                Vector2 point = points[i].m_pos;
                Vector2 point2 = points[(i + 1) % length].m_pos;
                f += (point.x * point2.y) - (point2.x * point.y);
            }
            return (Mathf.Abs(f) / 2f);
        }
    }
}
