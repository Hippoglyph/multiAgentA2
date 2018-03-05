using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOM {

    List<Vector3> points;
    List<Vector3> startBois;
    List<Vector3> goalBois;
    List<Vector3[]> nodes;
    Color[] colors;
    float moveStepSize = 0.2f;
    int pullForce = 5;

    public SOM(List<float[]> points, List<float[]> startBois, List<float[]> goalBois)
    {
        this.points = new List<Vector3>();
        foreach (float[] point in points)
            this.points.Add(new Vector3(point[0], 0, point[1]));
        this.startBois = new List<Vector3>();
        foreach (float[] point in startBois)
            this.startBois.Add(new Vector3(point[0], 0, point[1]));
        this.goalBois = new List<Vector3>();
        foreach (float[] point in goalBois)
            this.goalBois.Add(new Vector3(point[0], 0, point[1]));
        colors = new Color[startBois.Count];
        init();
    }

    void init()
    {
        nodes = new List<Vector3[]>();
        int nodeCount = points.Count / startBois.Count;
        for(int i = 0; i < startBois.Count; i++)
        {
            colors[i] = Random.ColorHSV();
            Vector3[] boisNodes = new Vector3[nodeCount + 2];
            boisNodes[0] = startBois[i];
            boisNodes[nodeCount+1] = goalBois[i];
            Vector3 guideLine = boisNodes[nodeCount + 1] - boisNodes[0];
            float stepSize = guideLine.magnitude / nodeCount;
            for(int j = 1; j <= nodeCount; j++)
                boisNodes[j] = boisNodes[0] + guideLine.normalized * stepSize * j;
            nodes.Add(boisNodes);
        }
    }

    float getDistance(Vector3 point1, Vector3 point2)
    {
        return (point1 - point2).sqrMagnitude;
    }

    public void drawState(float dt)
    {
        for(int agent = 0; agent < nodes.Count; agent++)
        {
            Vector3[] boisPoints = nodes[agent];
            for (int i = 1; i < boisPoints.Length; i++)
                Debug.DrawLine(boisPoints[i - 1], boisPoints[i], colors[agent], dt);
        }
    }

    void shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(0, array.Length);
            int tempGO = array[rnd];
            array[rnd] = array[i];
            array[i] = tempGO;
        }
    }

    int[] getRandomOrder(int size)
    {
        int[] order = new int[size];
        for (int i = 0; i < size; i++)
            order[i] = i;
        shuffle(order);
        return order;
    }

    int[] getWinner(int index)
    {
        float distance = float.MaxValue;
        int agent = 0;
        int pointIndex = 0;
        Vector3 point = points[index];
        for (int j = 0; j < nodes.Count; j++)
        {
            Vector3[] boisNodes = nodes[j];
            for (int i = 1; i < boisNodes.Length-1; i++)
            {
                float tmpDist = getDistance(boisNodes[i], point);
                if (tmpDist < distance)
                {
                    distance = tmpDist;
                    agent = j;
                    pointIndex = i;
                }
            }
        }
        return new int[] { agent, pointIndex };
    }

    int[] getAllWinners(int agent, int pointIndex, int hood)
    {
        int upperBound = Mathf.Min(pointIndex + hood, nodes[agent].Length);
        int lowerBound = Mathf.Max(pointIndex - hood, 0);
        int[] indexes = new int[upperBound - lowerBound];
        for (int i = 0; i < upperBound-lowerBound; i++)
            indexes[i] = lowerBound+i;
        return indexes;
    }

    void move(int agent, int pointIndex, int hood, Vector3 interestPoint)
    {
        int[] winners = getAllWinners(agent, pointIndex, hood);
        for(int i = 0; i < winners.Length; i++)
            nodes[agent][winners[i]] += moveStepSize * (interestPoint - nodes[agent][winners[i]]).normalized; //Maybe not normalized
    }

    public void update(int hood)
    {
        int[] order = getRandomOrder(points.Count);
        foreach(int index in order)
        {
            int[] winner = getWinner(index);
            move(winner[0], winner[1], hood, points[index]);
        }

        for(int agent = 0; agent < nodes.Count; agent++)
        {
            for (int i = 0; i < pullForce; i++)
            {
                move(agent, 0, hood, startBois[agent]);
                move(agent, nodes[agent].Length - 1, hood, goalBois[agent]);
            }
        }
    }
}
