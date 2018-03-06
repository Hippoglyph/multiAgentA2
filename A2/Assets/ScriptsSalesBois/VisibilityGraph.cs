using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibilityGraph {

    Vertice[] vertices;
    float[][] edges;

    public VisibilityGraph(List<float[]> verts)
    {
        vertices = new Vertice[verts.Count];
        for (int i = 0; i<verts.Count; i++)
            edges[i] = new float[verts.Count];

    }
}

public class Vertice
{
    public float x;
    public float y;
}
