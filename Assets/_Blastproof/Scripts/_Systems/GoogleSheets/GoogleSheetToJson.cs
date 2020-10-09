using System;
using System.Collections.Generic;
using System.IO;
using GoogleSheetsToUnity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Blastproof.Systems.GoogleSheets
{
    [CreateAssetMenu(menuName = "Blastproof/Systems/GoogleSheets/GoogleSheetToJSON")]
    public class GoogleSheetToJson : ScriptableObject
    {
        [BoxGroup("Values"), SerializeField] private string jsonFileName;
        [BoxGroup("Values"), SerializeField] private string sheetId;
        [BoxGroup("Values"), SerializeField] private string worksheetName;

        [BoxGroup("Info"), ShowInInspector, ReadOnly] private string Path => Application.persistentDataPath;

        [BoxGroup("LEVEL"), SerializeField, ReadOnly] private Level level;

        [Button]
        private void Read()
        {
            SpreadsheetManager.Read(new GSTU_Search(sheetId, worksheetName), UpdateJson);
        }

        public void UpdateJson(GstuSpreadSheet ss)
        {
            level.points.Clear();
            level.objectsNames.Clear();
            level.objectsPositions.Clear();           

            var columns = ss.columns;
            var rows = ss.rows;
            var cells = ss.Cells;

            var x = columns["points.x"];

            for (var i = 1; i < x.Count; i++)
            {
                var data = x[i].value;
                level.points.Add(new Vector2(float.Parse(data), 0));
            }

            var y = columns["points.y"];
            for (var i = 1; i < y.Count; i++)
            {
                var data = y[i].value;
                level.points[i - 1] = new Vector2(level.points[i - 1].x, float.Parse(data));
            }

            var names = columns["obstacleNames"];

            for (var i = 1; i < names.Count; i++)
            {
                var data = names[i];
                level.objectsNames.Add(data.value);
            }

            var obsx = columns["obstaclePos.x"];
            for (var i = 1; i < obsx.Count; i++)
            {
                var data = obsx[i].value;
                level.objectsPositions.Add(new Vector2(float.Parse(data), 0));
            }

            var obsy = columns["obstaclePos.y"];
            for (var i = 1; i < obsy.Count; i++)
            {
                var data = obsy[i].value;
                level.objectsPositions[i - 1] = new Vector2(level.objectsPositions[i - 1].x, float.Parse(data));
            }

            var material = columns["materialName"];
            level.materialName = material[1].value;

            var isClockWise = columns["isClockWise"];
            var isClockWiseData = isClockWise[1].value;

            if (isClockWiseData == "TRUE") level.isClockWise = true;
            else level.isClockWise = false;

            WriteFile();
        }

        public void WriteFile()
        {
            var json = JsonUtility.ToJson(level, true);
            File.WriteAllText(System.IO.Path.Combine(Path, jsonFileName), json);

        }
    }

    [Serializable]
    public class Level
    {
        public List<Vector2> points;
        public List<string> objectsNames;
        public List<Vector2> objectsPositions;
        public string materialName;
        public bool isClockWise;
    }
}