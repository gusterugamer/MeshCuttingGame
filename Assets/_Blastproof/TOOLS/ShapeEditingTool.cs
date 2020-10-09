using BlastProof;
using Sirenix.OdinInspector;
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

        public float size = 0;            

        public float marginX = 0;
        public float marginY = 0;

        public const float _WIDTH = 35f;
        public const float _HEIGHT = 63f;

        public List<GameObject> _go = new List<GameObject>();
        private List<string> _names = new List<string>();

        private List<Vector2> points;
        private LevelData ld = new LevelData();
        private bool materialChanged = false;

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
                    if (cam != null)
                    {
                        cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
                    }
                    else
                    {
                        Debug.LogError("NO CAMERA ATTACHED TO THE TOOL!");
                    }

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

            materialChanged = true;
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
            if (cam != null)
            {
                cam.transform.position = new Vector3(center.x, center.y, cam.transform.position.z);
            }
            else
            {
                Debug.LogError("NO CAMERA ATTACHED TO THE TOOL!!!");
            }
            _shape.fillPixelsPerUnit = _shape.spriteShape.fillTexture.width / Mathf.Max(maxX, maxY);
        }

        [Button]
        private void SaveModifications()
        {
            List<Vector2> posList = new List<Vector2>();
            List<string> obsNames = new List<string>();
            foreach (var go in _go)
            {
                posList.Add(go.transform.position);
                name = go.name;
                name = name.Replace("(Clone)", "");
                obsNames.Add(name);
            }

            foreach (var name in _names)
            {
                obsNames.Add(name);
            }

            //Material
            if (materialChanged)
            {
                ld.materialName = tex.name;
            }
            else
            {
                ld.materialName = _reader.loadedLevel.materialName;
            }          


            //ObjectPositions
            if (posList.Count > 0)
            {
                ld.objectsPositions = posList.ToArray();                
            }
            else
            {
                ld.objectsPositions = _reader.loadedLevel.objectsPositions;               
            }

            //ObjectNames
            if (obsNames.Count > 0)
            {
                ld.objectsNames = obsNames.ToArray();
            }
            else
            {
                ld.objectsNames = _reader.loadedLevel.objectsNames;
            }

            //ShapePoints
            if (points.Count > 0)
            {
                ld.points = points.ToArray();
            }
            else
            {
                ld.points = _reader.loadedLevel.points;
            }

            //isClockWise
            ld.isClockWise = _reader.loadedLevel.isClockWise;

            string path = Application.persistentDataPath;
            string jsonFileName = "Modified " + _reader.loadedLevelId.ToString() + ".json";

            var json = JsonUtility.ToJson(ld, true);
            File.WriteAllText(System.IO.Path.Combine(path, jsonFileName), json);

            Debug.Log("MODIFIED FILE CAN BE FOUND " + path + " with the name " + jsonFileName);
        }
    }
}
