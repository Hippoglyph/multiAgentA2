using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mother : MonoBehaviour {

    Problem problem;
    public string problemPath = "P25.json";
    public string trajPath = "P25_25_traj.json";
    public GameObject boundingObject;
    public float wallHeight = 4f;
    public float wallThickness = 0.5f;
    public Material wallMaterial;
    // Use this for initialization
    void Start () {
        problem = Problem.Import(problemPath, trajPath);
        spawnObjects();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void spawnObjects()
    {
        if(problem.boundingPolygon.Count != 0)
            spawnObject(problem.boundingPolygon, "boundingPolygon", boundingObject);

    }

    void spawnObject(List<float[]> poly, string name, GameObject parent)
    {
        for (int i = 0; i < poly.Count - 1; i++)
        {
            Vector3 origin = new Vector3(poly[i][0], wallHeight / 2, poly[i][1]);
            Vector3 destination = new Vector3(poly[i + 1][0], wallHeight / 2, poly[i + 1][1]);
            spawnWall(origin, destination, name + "_Side" + i, parent);
        }
        spawnWall(new Vector3(poly[0][0], wallHeight / 2, poly[0][1]), new Vector3(poly[poly.Count - 1][0], wallHeight / 2, poly[poly.Count - 1][1]), name + "_Side" + (poly.Count - 1), parent);
    }

    void spawnWall(Vector3 origin, Vector3 destination, string name, GameObject parent)
    {
        float width = Vector3.Distance(origin, destination);
        Vector3 polygonSide = destination - origin;
        Vector3 middlePoint = origin + polygonSide * 0.5f;
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = middlePoint;
        wall.GetComponent<Renderer>().material = wallMaterial;
        wall.transform.rotation = Quaternion.LookRotation(polygonSide);
        wall.transform.localScale = new Vector3(wallThickness, wallHeight, width);
    }
}
