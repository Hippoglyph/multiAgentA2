using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualConstructSoccer {

    List<Node> nodes;
    float playerHeight;
    float xMin, xMax, zMin, zMax;
    

    public VirtualConstructSoccer(List<float[]> formation, float playerHeight, float boundingMinX, float boundingMaxX, float boundingMinZ, float boundingMaxZ)
    {
        
        this.playerHeight = playerHeight;
        nodes = new List<Node>();
        Vector3 leaderPos = calculateBounding(formation, boundingMinX, boundingMaxX, boundingMinZ, boundingMaxZ);

        for (int i = 0; i < formation.Count; i++)
        {
            Vector3 followerPos = new Vector3(formation[i][0], playerHeight, formation[i][1]);
            Vector3 relativePos = new Vector3(followerPos.x - leaderPos.x, 0, followerPos.z - leaderPos.z);
            float orientation = Vector3.Angle(Vector3.right, relativePos)*Mathf.Deg2Rad;
            Vector3 cross = Vector3.Cross(Vector3.right, relativePos);
            if (cross.y > 0)
                orientation = -orientation;
            nodes.Add(new Node(relativePos.magnitude, orientation));
        }
    }
    Vector3 calculateBounding(List<float[]> formation, float boundingMinX, float boundingMaxX, float boundingMinZ, float boundingMaxZ)
    {
        float minX = float.MaxValue;
        float minZ = float.MaxValue;
        float maxX = float.MinValue;
        float maxZ = float.MinValue;
        for (int i = 0; i < formation.Count; i++)
        {
            maxX = Mathf.Max(formation[i][0], maxX);
            maxZ = Mathf.Max(formation[i][1], maxZ);
            minX = Mathf.Min(formation[i][0], minX);
            minZ = Mathf.Min(formation[i][1], minZ);
        }
        float precision = 1.1f;
        float halfWidth = (maxX - minX) / 2;
        float halfHeight = (maxZ - minZ) / 2;
        xMax = boundingMaxX - (halfWidth* precision);
        xMin = boundingMinX + (halfWidth * precision);
        zMax = boundingMaxZ - (halfHeight * precision);
        zMin = boundingMinZ + (halfHeight * precision);

        Vector3 center = new Vector3(minX + halfWidth, playerHeight, minZ + halfHeight);
        return center;

    }
    public List<Vector3> getPoints(Vector3 start)
    {
        float startX = Mathf.Clamp(start.x, xMin, xMax);
        float startZ = Mathf.Clamp(start.z, zMin, zMax);
        List<Vector3> points = new List<Vector3>();
        foreach(Node node in nodes)
        {
            float x = startX + node.length * Mathf.Cos(node.angle);
            float z = startZ + node.length * Mathf.Sin(node.angle);
            points.Add(new Vector3(x, playerHeight, z));
        }
        return points;
    }

    public Vector3 getPosition(int index, Vector3 start)
    {
        float startX = Mathf.Clamp(start.x, xMin, xMax);
        float startZ = Mathf.Clamp(start.z, zMin, zMax);
        float x = startX + nodes[index].length * Mathf.Cos(nodes[index].angle);
        float z = startZ + nodes[index].length * Mathf.Sin(nodes[index].angle);
        return new Vector3(x, playerHeight, z);
    }

    class Node
    {
        public float length;
        public float angle;
        public Node(float length, float angle)
        {
            this.length = length;
            this.angle = angle;
        }
    }
}
