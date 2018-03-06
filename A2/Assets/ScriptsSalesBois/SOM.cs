using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOM {

    List<Vector3> points;
    List<Vector3> startBois;
    List<Vector3> goalBois;
    List<Vector3[]> nodes;
    List<Vector3[]> paths;
    Color[] colors;
    float moveStepSize = 0.2f;

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
        paths = new List<Vector3[]>(startBois.Count);
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

    public void drawPaths(float dt)
    {
        for (int agent = 0; agent < paths.Count; agent++)
        {
            Vector3[] boisPoints = paths[agent];
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
        float[] lengths = getPathLengths();
        float average = getAverageLength(lengths);

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
                tmpDist = tmpDist * (lengths[j]/average);
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
        int upperBound = Mathf.Min(pointIndex + hood+1, nodes[agent].Length);
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
            nodes[agent][winners[i]] += moveStepSize * (interestPoint - nodes[agent][winners[i]]); //Maybe not normalized
    }

    public void update(int hood)
    {
        hood = Mathf.Max(0, hood);
        int[] order = getRandomOrder(points.Count);
        foreach(int index in order)
        {
            int[] winner = getWinner(index);
            move(winner[0], winner[1], hood, points[index]);
        }
        twistBack(hood);
    }

    void twistBack(int hood)
    {
        for (int agent = 0; agent < nodes.Count; agent++)
        {
            float dist = (nodes[agent][0] - startBois[agent]).magnitude;
            for(int i = hood; i > 0; i--)
            {
                float nextDist = (nodes[agent][i - 1] - nodes[agent][i]).magnitude;
                Vector3 dir = (nodes[agent][i - 1] - nodes[agent][i]).normalized;
                nodes[agent][i] += dir * Mathf.Min(dist, nextDist);
            }
            nodes[agent][0] = startBois[agent];

            dist = (nodes[agent][nodes[agent].Length-1] - goalBois[agent]).magnitude;
            for (int i = nodes[agent].Length - 1 - hood; i < nodes[agent].Length - 1; i++)
            {
                float nextDist = (nodes[agent][i + 1] - nodes[agent][i]).magnitude;
                Vector3 dir = (nodes[agent][i + 1] - nodes[agent][i]).normalized;
                nodes[agent][i] += dir * Mathf.Min(dist, nextDist);
            }
            nodes[agent][nodes[agent].Length - 1] = goalBois[agent];
        }
    }

    float[] getPathLengths()
    {
        float[] lengths = new float[nodes.Count];
        for(int agent = 0; agent < nodes.Count; agent++)
        {
            float sum = 0.0f;
            for (int i = 0; i < nodes[agent].Length-1; i++)
                sum += (nodes[agent][i + 1] - nodes[agent][i]).sqrMagnitude;
            lengths[agent] = sum;
        }
        return lengths;
    }

    float getAverageLength(float[] lengths)
    {
        float sum = 0.0f;
        for (int i = 0; i < lengths.Length; i++)
            sum += lengths[i];
        return sum/lengths.Length;
    }

    public void calculateBoisVisitOrder()
    {
        paths.Clear();
        List<Vector3>[][] assignedPoints = assignPoints();
        for (int agent = 0; agent < assignedPoints.Length; agent++)
            paths.Add(getPath(assignedPoints[agent],agent));
    }

    public List<Vector3[]> getPathsForBois()
    {
        if (paths != null)
            return paths;
        return new List<Vector3[]>();
    }

    Vector3[] getPath(List<Vector3>[] assignedPoints, int agentNr)
    {
        List<Vector3> order = new List<Vector3>();
        order.Add(startBois[agentNr]);
        for(int node = 0; node < assignedPoints.Length; node++)
        {
            if (assignedPoints[node] == null)
                continue;
            float dist = float.MaxValue;
            List<Vector3> bestOrder = new List<Vector3>();
            foreach (List<Vector3> per in permutations(assignedPoints[node]))
            {
                float tmpDist = getDestanceInNode(per);
                if(tmpDist < dist)
                {
                    dist = tmpDist;
                    bestOrder = per;
                }
            }
            order.AddRange(bestOrder);
        }
        order.Add(goalBois[agentNr]);
        return order.ToArray();
    }

    float getDestanceInNode(List<Vector3> nodePoints)
    {
        float sum = 0.0f;
        for (int i = 1; i < nodePoints.Count; i++)
            sum += (nodePoints[i]- nodePoints[i - 1]).sqrMagnitude;
        return sum;
    }

    List<List<Vector3>> permutations(List<Vector3> nodePoints)
    {
        return Bheap.Permute(nodePoints);
    }

    List<Vector3>[][] assignPoints()
    {
        List<Vector3>[][] assignedNodes = new List<Vector3>[nodes.Count][];
        for (int i = 0; i < assignedNodes.Length; i++)
            assignedNodes[i] = new List<Vector3>[nodes[i].Length];
        for (int i = 0; i< points.Count; i++)
        {
            int bestAgent = 0;
            int bestNode = 0;
            float dist = float.MaxValue;
            for(int agent = 0; agent < nodes.Count; agent++)
            {
                for (int boisNode = 0; boisNode < nodes[agent].Length; boisNode++)
                {
                    float tempDist = getDistance(points[i], nodes[agent][boisNode]);
                    if(tempDist < dist)
                    {
                        dist = tempDist;
                        bestAgent = agent;
                        bestNode = boisNode;
                    }
                }
            }
            if (assignedNodes[bestAgent][bestNode] == null)
                assignedNodes[bestAgent][bestNode] = new List<Vector3>();
            assignedNodes[bestAgent][bestNode].Add(points[i]);
        }

        return assignedNodes;
    }
}
