using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VisibilityGraph {

    Vector3[] vertices;
    float[][] edges;
    List<List<Vector3>> obstacles;
    int[] obstacleIds;

    public VisibilityGraph(List<List<float[]>> obstacles, List<float[]> interestPoints)
    {
        this.obstacles = new List<List<Vector3>>();
        int verticesInObtacles = 0;
        for (int i = 0; i < obstacles.Count; i++)
        {
            this.obstacles.Add(new List<Vector3>());
            verticesInObtacles += obstacles[i].Count;
            for (int j = 0; j < obstacles[i].Count; j++)
                this.obstacles[i].Add(getVector(obstacles[i][j]));
        }
        vertices = new Vector3[verticesInObtacles + interestPoints.Count];
        edges = new float[vertices.Length][];
        obstacleIds = new int[vertices.Length];
        for (int i = 0; i< vertices.Length; i++)
            edges[i] = new float[vertices.Length];

        for(int i = 0; i < interestPoints.Count; i++)
        {
            vertices[i] = getVector(interestPoints[i]);
            obstacleIds[i] = -1;
        }
        int counter = 0;
        for (int j = 0; j < obstacles.Count; j++)
        {
            for (int i = 0; i < obstacles[j].Count; i++)
            {
                obstacleIds[counter + interestPoints.Count] = j;
                vertices[counter + interestPoints.Count] = getVector(obstacles[j][i]);
                counter++;
            }
        }
        createEdges();
    }


    void createEdges()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            for (int j = i; j < vertices.Length; j++)
            {
                if (i == j)
                    edges[i][j] = -1;
                else
                {
                    if ((!(obstacleIds[i] == obstacleIds[j]) || (obstacleIds[i] == -1 || obstacleIds[j] == -1)) && !collides(i, j))
                        edges[i][j] = (vertices[i] - vertices[j]).magnitude;
                    else
                        edges[i][j] = -1;
                    edges[j][i] = edges[i][j];
                }
            }
        }
        int wallCounter = 0;
        for (int i = 0; i < vertices.Length - 1; i++)
        {
            if (obstacleIds[i] != -1)
            {
                if (obstacleIds[i] == obstacleIds[i + 1])
                {
                    if (!collides(i, i+1))
                    {
                        edges[i][i + 1] = (vertices[i] - vertices[i + 1]).magnitude;
                        edges[i + 1][i] = edges[i][i + 1];
                        wallCounter++;
                    }
                }
                else
                {
                    if (!collides(i, i-wallCounter))
                    {
                        edges[i][i - wallCounter] = (vertices[i] - vertices[i - wallCounter]).magnitude;
                        edges[i - wallCounter][i] = edges[i][i - wallCounter];
                    }
                    wallCounter = 0;
                }
            }
            else
                wallCounter = 0;
        }
        if (!collides(vertices.Length-1, vertices.Length - 1 - wallCounter))
        {
            edges[vertices.Length - 1][vertices.Length - 1 - wallCounter] = (vertices[vertices.Length - 1] - vertices[vertices.Length - 1 - wallCounter]).magnitude;
            edges[vertices.Length - 1 - wallCounter][vertices.Length - 1] = edges[vertices.Length - 1][vertices.Length - 1 - wallCounter];
        }
    }

    bool collides(int pIndex, int toIndex)
    {
        Vector3 p = vertices[pIndex];
        Vector3 to = vertices[toIndex];
        Vector3 r = to - p;
        if (isInObstacle(p, pIndex) || isInObstacle(to, toIndex))
            return true;
        for (int i = 0; i < obstacles.Count; i++)
        {
            for (int j = 0; j < obstacles[i].Count; j++)
            {
                Vector3 goal;
                if (j == obstacles[i].Count - 1)
                    goal = obstacles[i][0];
                else
                    goal = obstacles[i][j+1];
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

    bool isInObstacle(Vector3 p, int pid )
    {
        bool inside = false;
        for (int obs = 0; obs < obstacles.Count; obs++)
        {
            if (obs == obstacleIds[pid])
                continue;
            for (int i = 0, j = obstacles[obs].Count - 1; i < obstacles[obs].Count; j = i++)
            {
                
                if ((obstacles[obs][i].z > p.z) != (obstacles[obs][j].z > p.z) &&
                     p.x < (obstacles[obs][j].x - obstacles[obs][i].x) * (p.z - obstacles[obs][i].z) / (obstacles[obs][j].z - obstacles[obs][i].z) + obstacles[obs][i].x)
                {
                    inside = !inside;
                }
            }
            if (inside == true)
                return inside;
        }
        return inside;
    }

    public void drawGraph()
    {
        for (int i = 0; i < edges.Length; i++)
            for (int j = 0; j < edges[i].Length; j++)
                if (edges[i][j] != -1)
                    Debug.DrawLine(vertices[i], vertices[j], Color.blue, 100000f);
    }

    float cross2d(Vector3 v, Vector3 w)
    {
        return v.x * w.z - v.z * w.x;
    }

    Vector3 getVector(float[] point)
    {
        return new Vector3(point[0], 0f, point[1]);
    }

    int getIndex(Vector3 pos)
    {
        float distance = (vertices[0] - pos).sqrMagnitude;
        int bestI = 0;
        for (int i = 1; i < vertices.Length; i++)
        {
            float newDistance = (vertices[i] - pos).sqrMagnitude;
            if (newDistance < distance)
            {
                distance = newDistance;
                bestI = i;
            }          
        }
        return bestI;
    }

    public Vector3[] getPath(Vector3[] interestPoints)
    {
        List<Vector3> path = new List<Vector3>();
        for (int i = 0; i < interestPoints.Length - 1; i++)
            path.AddRange(AStar(getIndex(interestPoints[i]), getIndex(interestPoints[i + 1])));
        return path.ToArray();
    }

    List<Vector3> AStar(int from, int to)
    {
        HashSet<int> closedSet = new HashSet<int>();
        List<int> openSet = new List<int>() { from };
        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        Dictionary<int, float> gScore = new Dictionary<int, float>();
        Dictionary<int, float> fScore = new Dictionary<int, float>();
        gScore.Add(from, 0f);
        fScore.Add(from, Vector3.Distance(vertices[from], vertices[to]));
        int count = 0;
        while(openSet.Count != 0 && count < 1000)
        {
            count++;
            float minFScore = float.MaxValue;
            int current = openSet[0];
            int currentI = 0;
            for (int i = 0; i < openSet.Count; i++)
            {
                if (!fScore.ContainsKey(openSet[i]))
                    fScore[openSet[i]] = float.MaxValue;
                if (fScore[openSet[i]] < minFScore)
                {
                    minFScore = fScore[openSet[i]];
                    current = openSet[i];
                    currentI = i;
                }
            }
            if (current == to)
            {
                return reconstructPath(cameFrom, current);
            }
            openSet.RemoveAt(currentI);
            closedSet.Add(current);
            foreach(int nejbor in getNeighbours(current))
            {
                if (closedSet.Contains(nejbor))
                    continue;
                float tentGscore = gScore[current] + edges[current][nejbor];
                if (!gScore.ContainsKey(nejbor))
                    gScore.Add(nejbor, float.MaxValue);
                if (tentGscore >= gScore[nejbor])
                    continue;
                if (!openSet.Contains(nejbor))
                    openSet.Add(nejbor);
                cameFrom[nejbor] = current;
                gScore[nejbor] = tentGscore;
                fScore[nejbor] = gScore[nejbor] + Vector3.Distance(vertices[nejbor], vertices[to]); 
            }
            
        }
        return new List<Vector3>();

    }

    List<Vector3> reconstructPath(Dictionary<int,int> cameFrom, int current)
    {
        Stack<Vector3> path = new Stack<Vector3>();
        path.Push(vertices[current]);
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Push(vertices[current]);
        }
        return path.ToList();
    }

    List<int> getNeighbours(int me)
    {
        List<int> neighbours = new List<int>();
        for (int i = 0; i < vertices.Length; i++)
        {
            if (edges[me][i] != -1)
                neighbours.Add(i);
        }
        return neighbours;
    }
    

}


