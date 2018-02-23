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
    int currentPosition = 0;
    VirtualConstruct VC;
    List<MotionModel> cars;
    GameObject leader;
    float velocityGoal = 0f;
    // Use this for initialization
    void Start () {
        problem = Problem.Import(problemPath, trajPath);
        cars = new List<MotionModel>();
        spawnObjects();
        VC = new VirtualConstruct(problem.formationPositions, vehicleHeight);
    }

    float getDt()
    {
        return Time.deltaTime * speed;
        //return problem.vehicle_dt * speed;
    }

    // Update is called once per frame
    bool started = false;
	void Update () {
  
        if(currentPosition < problem.trajectory.Count)
        {
            moveAll();
            if (started)
            {
                moveConstruct();
                
            }
            else
                started = checkStartPositions();
        }
        else
        {
            Vector3 position = new Vector3(problem.trajectory[problem.trajectory.Count-1][0], vehicleHeight, problem.trajectory[problem.trajectory.Count-1][1]);
            for (int i = 0; i < cars.Count; i++)
            {
                cars[i].moveTowards(VC.getPosition(i, position, problem.theta[problem.trajectory.Count-1]), getDt(), velocityGoal);
            }
        }
        
        
    }

    void moveAll()
    {
        Vector3 position = new Vector3(problem.trajectory[currentPosition][0], vehicleHeight, problem.trajectory[currentPosition][1]);
        for (int i = 0; i<cars.Count;i++)
        {
            cars[i].moveTowards(VC.getPosition(i, position, problem.theta[currentPosition]), getDt(), velocityGoal);
        }
    }

    bool checkStartPositions()
    {
        for (int i = 0; i < cars.Count; i++)
        {
            if (!cars[i].inFormation)
                return false;
        }
        return true;
    }

    float currentTime = 0f;
    void moveConstruct()
    {
        currentTime += getDt();
        if (currentTime >= problem.vehicle_dt)
        {
            int timeSpent = (int)(currentTime / problem.vehicle_dt);
            currentTime = currentTime % (problem.vehicle_dt);
            currentPosition+=timeSpent;
            currentPosition = Mathf.Min(currentPosition, problem.trajectory.Count-1);
            Vector3 oldPos = leader.transform.position;
            leader.transform.position = new Vector3(problem.trajectory[currentPosition][0], vehicleHeight, problem.trajectory[currentPosition][1]);
            leader.transform.forward =  new Vector3(Mathf.Cos(problem.theta[currentPosition]), 0, Mathf.Sin(problem.theta[currentPosition]));
            velocityGoal = (oldPos - leader.transform.position).magnitude/getDt();
        }
    }
  
    void spawnObjects()
    {
        //Spawn bounding wall
        if(problem.boundingPolygon.Count != 0)
            spawnObject(problem.boundingPolygon, "boundingPolygon", boundingObject);
        //Spawn vehicles at start position
        for (int i = 0; i<problem.startPositions.Count; i++)
            cars.Add(spawnVehicle(problem.startPositions[i], "vehicle_" + i, Mathf.PI));
        leader = spawnVehicle(problem.trajectory[0], "leaderVehicle", problem.theta[0]).gameObject;

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

    MotionModel spawnVehicle(float[] origin, string name, float orientation)
    {
        Vector3 newOrientation = new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        Vector3 position = new Vector3(origin[0], vehicleHeight, origin[1]);
        GameObject vehicle = Instantiate(vehicleObject, vehicleBoundingObject.transform, true);
        vehicle.name = name;
        vehicle.transform.position = position;
        vehicle.transform.forward = newOrientation;
        MotionModel mm = vehicle.AddComponent<MotionModel>();
        mm.setParams(problem.vehicle_v_max, problem.vehicle_L, problem.vehicle_phi_max, problem.vehicle_a_max);
        return mm;
    }
}
