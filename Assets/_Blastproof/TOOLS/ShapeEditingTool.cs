using BlastProof;
using GoogleSheetsToUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

namespace Blastproof.Tools
{
    [CreateAssetMenu(menuName = "Blastproof/TOOLS/SpriteEditingTool")]
    public class ShapeEditingTool : ScriptableObject
    {
        public SpriteShapeController _shape;

        public float size = 0;

        public Texture2D tex;

        public Camera cam;

        public float marginX = 0;
        public float marginY = 0;

        public const float _WIDTH = 35f;
        public const float _HEIGHT = 63f;


        private List<Vector2> points;

        private void ReadPoints()
        {
            points = new List<Vector2>();
            if (_shape != null)
            {
                int length = _shape.spline.GetPointCount();
                for (int i = 0; i < length; i++)
                {
                    points.Add(_shape.spline.GetPosition(i));
                }
            }
            else
            {
                Debug.LogError("NO SPRITE ATTACHED TO THE TOOL!!!");
            }
        }

        [Button]
        private void ReSize()
        {
            ReadPoints();
            if (_shape != null)
            {
                float minX = Mathf.Infinity;
                float minY = Mathf.Infinity;

                float maxX = -Mathf.Infinity;
                float maxY = -Mathf.Infinity;

                if (size != 0)
                {
                    Matrix4x4 scaleMatrix = Mathematics.ScaleMatrix(size);

                    for (int i = 0; i < points.Count; i++)
                    {
                        points[i] = scaleMatrix.MultiplyPoint(points[i]);
                        minX = Mathf.Min(minX, points[i].x);
                        minY = Mathf.Min(minY, points[i].y);
                    }

                    for (int i = 0; i < points.Count; i++)
                    {
                        points[i] -= new Vector2(minX, minY);
                    }

                    for (int i = 0; i < points.Count; i++)
                    {
                        points[i] += new Vector2(marginX, marginY);
                        maxX = Mathf.Max(maxX, points[i].x);
                        maxY = Mathf.Max(maxY, points[i].y);
                    }

                    _shape.fillPixelsPerUnit = _shape.spriteShape.fillTexture.width / Mathf.Max(maxX, maxY);


                    _shape.spline.Clear();

                    Vector2 center = Vector2.zero;

                    for (int i = 0; i < points.Count; i++)
                    {
                        _shape.spline.InsertPointAt(i, points[i]);
                        center += points[i];
                    }

                    center /= points.Count;

                    cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);

                }
            }
            else
            {
                Debug.LogError("NO SPRITE ATTACHED TO THE TOOL!!!");
            }
        }

        [Button]
        private void ChangeMaterial()
        {
            ReadPoints();
            float maxX = -Mathf.Infinity;
            float maxY = -Mathf.Infinity;

            if (tex == null)
            {
                Debug.LogError("NO TEXTURE ATTACHED TO THE TOOL!!!");
                return;
            }
            if (_shape == null)
            {
                Debug.LogError("NO SPRITE ATTACHED TO THE TOOL!!!");
                return;
            }

            _shape.spriteShape.fillTexture = tex;

            for (int i = 0; i < points.Count; i++)
            {
                points[i] += new Vector2(marginX, marginY);
                maxX = Mathf.Max(maxX, points[i].x);
                maxY = Mathf.Max(maxY, points[i].y);
            }

            _shape.fillPixelsPerUnit = _shape.spriteShape.fillTexture.width / Mathf.Max(maxX, maxY);
        }

        [Button]
        private void ReSizeToFit()
        {
            ReadPoints();

            float maxX = -Mathf.Infinity;
            float maxY = -Mathf.Infinity;

            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;

            float tempMaxWidth = _WIDTH - marginX;
            float tempMaxHeight = _HEIGHT - marginY;

            for (int i=0; i<points.Count; i++)
            {
                maxX = Mathf.Max(maxX, points[i].x);
                maxY = Mathf.Max(maxY, points[i].y);

                minX = Mathf.Min(minX, points[i].x);
                minY = Mathf.Min(minY, points[i].y);
            }

            Debug.Log(maxX);
            Debug.Log(maxY);

            maxX -= minX;
            maxY -= minY;

            float scaleFactorX;
            float scaleFactorY;

            if (maxX <= tempMaxWidth)
            {
                scaleFactorX = maxX / tempMaxWidth;
            }
            else
            {
                scaleFactorX = tempMaxWidth / maxX;
            }

            if (maxY <= tempMaxHeight)
            {
                scaleFactorY = maxY / tempMaxHeight;
            }
            else
            {
                scaleFactorY = tempMaxHeight / maxY;
            }

            for (int i=0;i<points.Count;i++)
            {
                points[i] -= new Vector2(minX, minY);
            }
            
            float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY);            
            
            for (int i=0;i<points.Count;i++)
            {
                points[i] *= scaleFactor;
            }

            int length = _shape.spline.GetPointCount();

            _shape.spline.Clear();

            Vector2 center = Vector2.zero;

            for (int i=0;i<length;i++)
            {
                _shape.spline.InsertPointAt(i, points[i]);
                center += points[i];
            }

            center /= points.Count;

            cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
        }
    }
}
