using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Problem {

    public float vehicle_L, vehicle_a_max, vehicle_dt, vehicle_omega_max, vehicle_t, vehicle_phi_max, vehicle_v_max;
    public List<float[]> boundingPolygon;
    public List<float[]> formationPositions;
    public List<float[]> startPositions;
    public List<float> time;
    public List<float[]> trajectory;
    public List<float> theta;
    private enum Type { bounding, formation, startPos };
    private enum TypeTraj { t, x, y, theta };

    public static Problem Import(string filePath, string trajectoryPath) {
        StreamReader reader = new StreamReader(filePath);
        string json = reader.ReadToEnd();
        reader.Close();
        reader = new StreamReader(trajectoryPath);
        string trajJson = reader.ReadToEnd();
        reader.Close();
        Problem map = JsonUtility.FromJson<Problem>(json);
        map.read_polygons(json);
        map.readTrajectory(trajJson);
        return map;
    }

    private void readTrajectory(string jsonString)
    {
        string[] json = jsonString.Split('\n');
        time = new List<float>();
        trajectory = new List<float[]>();
        List<float> x = new List<float>();
        List<float> y = new List<float>();
        List<float> theta = new List<float>();
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i].Contains("t") || json[i].Contains("x") || json[i].Contains("y") || json[i].Contains("theta"))
            {
                TypeTraj type;
                if (json[i].Contains("theta"))
                    type = TypeTraj.theta;
                else if (json[i].Contains("x"))
                    type = TypeTraj.x;
                else if (json[i].Contains("y"))
                    type = TypeTraj.y;
                else
                    type = TypeTraj.t;

                for (int j = i + 1; j < json.Length - 1; j += 4)
                {
                    if (json[j].Contains("]"))
                    {
                        break;
                    }
                    else
                    {
                        string value = json[j].Trim(' ').Trim(',').Trim(',').Trim('\r').Trim(',');
                        double valueDouble = double.Parse(value, CultureInfo.InvariantCulture);
                        if (type == TypeTraj.theta)
                            theta.Add((float)valueDouble);
                        else if (type == TypeTraj.x)
                            x.Add((float)valueDouble);
                        else if (type == TypeTraj.y)
                            y.Add((float)valueDouble);
                        else
                            time.Add((float)valueDouble);
                    }
                }
            }
        }
        for (int i = 0; i < x.Count; i++)
            trajectory.Add(new float[] { x[i], y[i] });
    }

    private void read_polygons(string jsonString) {
        string[] json = jsonString.Split('\n');
        boundingPolygon = new List<float[]>();
        formationPositions = new List<float[]>();
        startPositions = new List<float[]>();
        for (int i = 0; i < json.Length; i++) {
            if (json[i].Contains("bounding_polygon") || json[i].Contains("formation_positions") || json[i].Contains("start_positions")) {
                Type type;
                if (json[i].Contains("bounding_polygon"))
                    type = Type.bounding;
                else if (json[i].Contains("formation_positions"))
                    type = Type.formation;
                else
                    type = Type.startPos;

                for (int j = i + 1; j < json.Length - 1; j += 4) {
                    if (json[j].Contains("]") && json[j - 1].Contains("]")) {
                        break;
                    }
                    if (json[j].Contains("[")) {
                        string x_cor = json[j + 1].Trim(' ').Trim(',').Trim(',').Trim('\r').Trim(',');
                        string y_cor = json[j + 2].Trim(' ').Trim(',');
                        double x_coordinate = double.Parse(x_cor, CultureInfo.InvariantCulture);
                        double y_coordinate = double.Parse(y_cor, CultureInfo.InvariantCulture);
                        if (type == Type.bounding)
                            boundingPolygon.Add(new float[]{ (float)x_coordinate, (float)y_coordinate});
                        else if (type == Type.formation)
                            formationPositions.Add(new float[] { (float)x_coordinate, (float)y_coordinate });
                        else
                            startPositions.Add(new float[] { (float)x_coordinate, (float)y_coordinate });
                    }
                }
            }
        }

    }
}