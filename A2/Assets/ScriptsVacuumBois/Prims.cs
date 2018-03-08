using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Prims  {
    List<Vector3> points;
    List<List<Vector3>> obstacles;
    float radius;

	public Prims(List<List<float[]>> obstacles, List<float[]> interests, float radius)
    {
        this.radius = radius;
        points = new List<Vector3>();
        this.obstacles = new List<List<Vector3>>();
        for (int i = 0; i < interests.Count; i++)
            points.Add(new Vector3(interests[i][0], 0f, interests[i][1]));

        for(int i = 0; i < obstacles.Count; i++)
        {
            this.obstacles.Add(new List<Vector3>());
            for (int j = 0; j < obstacles[i].Count; j++)
                this.obstacles[i].Add(new Vector3(obstacles[i][j][0], 0f, obstacles[i][j][1]));
        }
    }

    public List<float[]> getNewPointsOfInterest()
    {
        List<Vector3> bestPoints = new List<Vector3>();
        int current = Random.Range(0, points.Count - 1);
        float sqrRadius = radius * radius;
        float targetDistance = 4 * sqrRadius*20000;
        List<int> openSet = new List<int>();
        for (int i = 0; i < points.Count; i++)
            if (i != current)
                openSet.Add(i);
        List<int> closedSet = new List<int>() { current };
        while (openSet.Count != 0)
        {
            float distance = float.MaxValue;
            int bestI = 0;
            /*
            for (int i = 0; i < openSet.Count; i++)
            {
                for (int j = 0; j < closedSet.Count; j++)
                {     
                    float currDistance = (points[closedSet[j]] - points[openSet[i]]).sqrMagnitude;
                    if (Mathf.Abs(currDistance - targetDistance) < distance)
                    {
                        distance = currDistance;
                        bestI = i;
                    }
                }
            }*/
            /*
            int[] scores = new int[openSet.Count];
            for (int j = 0; j < closedSet.Count; j++)
            {
                float scoreDist = float.MaxValue;
                int bestPoint = 0;
                for (int i = 0; i < openSet.Count; i++)
                {
                    float currDistance = (points[closedSet[j]] - points[openSet[i]]).sqrMagnitude;
                    if (Mathf.Abs(currDistance - targetDistance) < scoreDist)
                    {
                        scoreDist = currDistance;
                        bestPoint = i;
                    }
                }
                scores[bestPoint]++;
            }
            float bestScore = float.MinValue;
            for(int i = 0; i < scores.Length; i++)
            {
                if (bestScore < scores[i])
                {
                    bestScore = scores[i];
                    bestI = i;
                }
            }*/
            bestI = Random.Range(0, openSet.Count - 1);

            int pointsIndex = openSet[bestI];
            bestPoints.Add(points[pointsIndex]);
            for (int i = openSet.Count-1; i >= 0; i--)
            {
                if ((points[pointsIndex] - points[openSet[i]]).sqrMagnitude <= sqrRadius && !collides(pointsIndex, openSet[i]))
                {
                    closedSet.Add(openSet[i]);
                    openSet.RemoveAt(i);
                }
            }
        }
        List<float[]> toReturn = new List<float[]>();
        foreach (Vector3 p in bestPoints)
            toReturn.Add(new float[] { p.x, p.z });
        return toReturn;
    }

    public bool collides(int pIndex, int toIndex)
    {
        Vector3 p = points[pIndex];
        Vector3 to = points[toIndex];
        return collides(p, to);
    }

    public bool collides(Vector3 p, Vector3 to)
    {
        Vector3 r = to - p;
        for (int i = 0; i < obstacles.Count; i++)
        {
            for (int j = 0; j < obstacles[i].Count; j++)
            {
                Vector3 goal;
                if (j == obstacles[i].Count - 1)
                    goal = obstacles[i][0];
                else
                    goal = obstacles[i][j + 1];
                if (to == obstacles[i][j] || goal == to || p == obstacles[i][j] || goal == p)
                    continue;
                Vector3 s = goal - obstacles[i][j];
                Vector3 q = obstacles[i][j];
                Vector3 nominator = q - p;
                float denominator = cross2d(r, s);
                float t = cross2d(nominator, s) / denominator;
                float u = cross2d(nominator, r) / denominator;
                if (0 <= t && t <= 1 && 0 <= u && u <= 1 && denominator != 0.0)
                    return true;
            }
        }
        return false;
    }

    float cross2d(Vector3 v, Vector3 w)
    {
        return v.x * w.z - v.z * w.x;
    }
}
