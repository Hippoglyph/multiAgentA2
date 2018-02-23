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
    public GameObject vehicleBoundingObject;
    public GameObject vehicleObject;
    float boundingMinX, boundingMinZ = float.MaxValue;
    float boundingMaxX, boundingMaxZ = float.MinValue;
    public float vehicleHeight = 0f;
    public float speed = 5f;
    VirtualConstruct VC;
    List<MotionModel> carMotions;
    // Use this for initialization
    void Start () {
        problem = Problem.Import(problemPath, trajPath);
        carMotions = new List<MotionModel>();
        spawnObjects();
        VC = new VirtualConstruct(problem.formationPositions, vehicleHeight);
        
    }

    float getDt()
    {
        return Time.deltaTime * speed;
    }
	
	// Update is called once per frame
	void Update () {
        carMotions[0].moveTowards(Vector3.zero, getDt());
    }

    void vcTest()
    {
        List<Vector3> points = VC.getPoints(new Vector3(27.5f, 0, 30), -Mathf.PI);
        foreach(Vector3 point in points)
        {
            GameObject thing = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            thing.transform.position = point+Vector3.up;
        }

    }

    void moveToStartPositions()
    {

    }

    void spawnObjects()
    {
        //Spawn bounding wall
        if(problem.boundingPolygon.Count != 0)
            spawnObject(problem.boundingPolygon, "boundingPolygon", boundingObject);
        //Spawn vehicles at start position
        //for (int i = 0; i<problem.startPositions.Count; i++)
            carMotions.Add(spawnVehicle(problem.startPositions[0], "vehicle_" + 0));

        spawnVehicle(problem.formationPositions[0], "leaderVehicle");

    }

    void spawnObject(List<float[]> poly, string name, GameObject parent)
    {
        for (int i = 0; i < poly.Count - 1; i++)
        {
            Vector3 origin = new Vector3(poly[i][0], wallHeight / 2, poly[i][1]);
            Vector3 destination = new Vector3(poly[i + 1][0], wallHeight / 2, poly[i + 1][1]);
            spawnWall(origin, destination, name + "_Side" + i, parent);
            boundingMaxX = Mathf.Max(origin.x, boundingMaxX);
            boundingMaxZ = Mathf.Max(origin.z, boundingMaxZ);
            boundingMinX = Mathf.Min(origin.x, boundingMinX);
            boundingMinZ = Mathf.Min(origin.z, boundingMinZ);
        }
        spawnWall(new Vector3(poly[0][0], wallHeight / 2, poly[0][1]), new Vector3(poly[poly.Count - 1][0], wallHeight / 2, poly[poly.Count - 1][1]), name + "_Side" + (poly.Count - 1), parent);
        boundingMaxX = Mathf.Max(poly[poly.Count - 1][0], boundingMaxX);
        boundingMaxZ = Mathf.Max(poly[poly.Count - 1][1], boundingMaxZ);
        boundingMinX = Mathf.Min(poly[poly.Count - 1][0], boundingMinX);
        boundingMinZ = Mathf.Min(poly[poly.Count - 1][1], boundingMinZ);
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

    MotionModel spawnVehicle(float[] origin, string name)
    {
        Vector3 position = new Vector3(origin[0], vehicleHeight, origin[1]);
        GameObject vehicle = Instantiate(vehicleObject, vehicleBoundingObject.transform, true);
        vehicle.name = name;
        vehicle.transform.position = position;
        vehicle.transform.LookAt(new Vector3((boundingMaxX - boundingMinX) / 2, vehicleHeight, (boundingMaxZ - boundingMinZ) / 2));
        MotionModel mm = vehicle.AddComponent<MotionModel>();
        mm.setParams(problem.vehicle_v_max, problem.vehicle_L, problem.vehicle_phi_max);
        return mm;
    }
}
