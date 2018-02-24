using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualConstruct {

    List<Node> nodes;
    float vehicleHeight;

    public VirtualConstruct(List<float[]> formation, float vehicleHeight)
    {
        this.vehicleHeight = vehicleHeight;
        nodes = new List<Node>();
        Vector2 leaderPos = new Vector2(formation[0][0], formation[0][1]);

        for(int i = 1; i < formation.Count; i++)
        {
            Vector2 followerPos = new Vector2(formation[i][0],formation[i][1]);
            Vector2 relativePos = new Vector2(followerPos.x - leaderPos.x, followerPos.y - leaderPos.y);
            float orientation = Vector2.Angle(Vector2.up, relativePos)*Mathf.Deg2Rad;
            Vector3 cross = Vector3.Cross(Vector3.forward, relativePos);
            if (cross.y < 0)
                orientation = -orientation;
            nodes.Add(new Node(relativePos.magnitude, orientation));
        }

    }

    public List<Vector3> getPoints(Vector3 start, float theta)
    {
        List<Vector3> points = new List<Vector3>();
        foreach(Node node in nodes)
        {
            float x = start.x + node.length * Mathf.Cos(node.angle + theta);
            float z = start.z + node.length * Mathf.Sin(node.angle + theta);
            points.Add(new Vector3(x, vehicleHeight, z));
        }
        return points;
    }

    public Vector3 getPosition(int index, Vector3 start, float theta)
    {
        float x = start.x + nodes[index].length * Mathf.Cos(nodes[index].angle + theta);
        float z = start.z + nodes[index].length * Mathf.Sin(nodes[index].angle + theta);
        return new Vector3(x, vehicleHeight, z);
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
