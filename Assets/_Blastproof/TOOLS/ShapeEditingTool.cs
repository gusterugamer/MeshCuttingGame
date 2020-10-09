using BlastProof;
using GoogleSheetsToUnity;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UIElements;

namespace Blastproof.Tools
{
    [CreateAssetMenu(menuName = "Blastproof/TOOLS/SpriteEditingTool")]
    public class ShapeEditingTool : ScriptableObject
    {
        public SpriteShapeController _shape;
        public Camera cam;

        public JsonReader _reader;

        public Material tex;

        private LevelData ld = new LevelData();

        public float size = 0;            

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
                Debug.LogError("NO MATERIAL ATTACHED TO THE TOOL!!!");
                return;
            }
            if (_shape == null)
            {
                Debug.LogError("NO SPRITE ATTACHED TO THE TOOL!!!");
                return;
            }

            _shape.spriteShape.fillTexture = tex.mainTexture as Texture2D;
            _shape.GetComponent<SpriteShapeRenderer>().material = tex;

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

            for (int i = 0; i < points.Count; i++)
            {
                minX = Mathf.Min(minX, points[i].x);
                minY = Mathf.Min(minY, points[i].y);
            }

            for (int i = 0; i < points.Count; i++)
            {
                points[i] -= new Vector2(minX, minY);
                maxX = Mathf.Max(maxX, points[i].x);
                maxY = Mathf.Max(maxY, points[i].y);
            }           

            float scaleFactorX;
            float scaleFactorY;

            scaleFactorX = tempMaxWidth / maxX;
            scaleFactorY = tempMaxHeight / maxY;

            float scaleFactor = Mathf.Min(scaleFactorX, scaleFactorY);

            for (int i = 0; i < points.Count; i++)
            {
                points[i] *= scaleFactor;
            }

            int length = points.Count;

            _shape.spline.Clear();

            Vector2 center = Vector2.zero;

            maxX = -Mathf.Infinity;
            maxY = -Mathf.Infinity;

            for (int i = 0; i < length; i++)
            {
                _shape.spline.InsertPointAt(i, points[i]);
                center += points[i];

                maxX = Mathf.Max(maxX, points[i].x);
                maxY = Mathf.Max(maxY, points[i].y);
            }

            center /= points.Count;
            cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
            _shape.fillPixelsPerUnit = _shape.spriteShape.fillTexture.width / Mathf.Max(maxX, maxY);
        }

        [Button]
        private void SaveModifications()
        {
            ld.isClockWise = _reader.loadedLevel.isClockWise;
            ld.materialName = tex.name;
            ld.objectsNames = _reader.loadedLevel.objectsNames;
            ld.objectsPositions = _reader.loadedLevel.objectsPositions;
            ld.points = points.ToArray();

            string path = Application.persistentDataPath;
            string jsonFileName = "Modified " + _reader.loadedLevelId.ToString() + ".json";

            var json = JsonUtility.ToJson(ld, true);
            File.WriteAllText(System.IO.Path.Combine(path, jsonFileName), json);

            Debug.Log("MODIFIED FILE CAN BE FOUND " + path + " with the name " + jsonFileName);
        }
    }
}
