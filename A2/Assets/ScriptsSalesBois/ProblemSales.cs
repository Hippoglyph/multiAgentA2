using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

[System.Serializable]
public class ProblemSales {

    public float vehicle_L, vehicle_a_max, vehicle_dt, vehicle_omega_max, vehicle_t, vehicle_phi_max, vehicle_v_max;
    public List<float[]> boundingPolygon;
    public List<float[]> pointsOfInterest;
    public List<float[]> startPositions;
    public List<float[]> goalPositions;
    public List<List<float[]>> obstacles;

    private enum Type { bounding, startPos, goalPos, pointOfInterest, obstacle };

    public static ProblemSales Import(string filePath) {
        StreamReader reader = new StreamReader(filePath);
        string json = reader.ReadToEnd();
        reader.Close();
        ProblemSales map = JsonUtility.FromJson<ProblemSales>(json);
        map.read_polygons(json);
        return map;
    }


    private void read_polygons(string jsonString) {
        string[] json = jsonString.Split('\n');
        boundingPolygon = new List<float[]>();
        pointsOfInterest = new List<float[]>();
        goalPositions = new List<float[]>();
        obstacles = new List<List<float[]>>();
        startPositions = new List<float[]>();
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i].Contains("bounding_polygon") || json[i].Contains("start_positions") || json[i].Contains("goal_positions") || json[i].Contains("obstacle_") || json[i].Contains("points_of_interest"))
            {
                List<float[]> obstacle = new List<float[]>();
                Type type;
                if (json[i].Contains("bounding_polygon"))
                    type = Type.bounding;
                else if (json[i].Contains("start_positions"))
                    type = Type.startPos;
                else if (json[i].Contains("goal_positions"))
                    type = Type.goalPos;
                else if (json[i].Contains("points_of_interest"))
                    type = Type.pointOfInterest;
                else
                    type = Type.obstacle;

                for (int j = i + 1; j < json.Length - 1; j += 4)
                {
                    if (json[j].Contains("]") && json[j - 1].Contains("]"))
                    {
                        break;
                    }
                    if (json[j].Contains("["))
                    {
                        string x_cor = json[j + 1].Trim(' ').Trim(',').Trim(',').Trim('\r').Trim(',');
                        string y_cor = json[j + 2].Trim(' ').Trim(',');
                        double x_coordinate = double.Parse(x_cor, CultureInfo.InvariantCulture);
                        double y_coordinate = double.Parse(y_cor, CultureInfo.InvariantCulture);
                        if (type == Type.bounding)
                            boundingPolygon.Add(new float[] { (float)x_coordinate, (float)y_coordinate });
                        else if (type == Type.startPos)
                            startPositions.Add(new float[] { (float)x_coordinate, (float)y_coordinate });
                        else if (type == Type.goalPos)
                            goalPositions.Add(new float[] { (float)x_coordinate, (float)y_coordinate });
                        else if (type == Type.pointOfInterest)
                            pointsOfInterest.Add(new float[] { (float)x_coordinate, (float)y_coordinate });
                        else
                            obstacle.Add(new float[] { (float)x_coordinate, (float)y_coordinate });
                    }
                }
                if (type == Type.obstacle)
                    obstacles.Add(obstacle);
            }
        }

    }
}