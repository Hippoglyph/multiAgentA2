using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherSales : MonoBehaviour {

    ProblemSales problem;
    public string problemPath = "P22.json";
    public GameObject boundingObject;
    public float wallHeight = 4f;
    public float wallThickness = 0.5f;
    public Material wallMaterial;
    public GameObject boisBoundingObject;
    public GameObject boisObject;
    public GameObject pointsBoundingObject;
    public GameObject pointsObject;
    float boundingMinX, boundingMinZ = float.MaxValue;
    float boundingMaxX, boundingMaxZ = float.MinValue;
    public float boisHeight = 0f;
    public float pointHeight = 0.4f;
    public float speed = 5f;
    public float drawTime = 0.1f;
    public int epochs = 20;
    List<GameObject> points;
    List<MotionModelSalesBoi> bois;
    SOM som;
    // Use this for initialization
    void Start () {
        problem = ProblemSales.Import(problemPath);
        bois = new List<MotionModelSalesBoi>();
        points = new List<GameObject>();
        spawnObjects();
        som = new SOM(problem.pointsOfInterest, problem.startPositions, problem.goalPositions);
        List<float[]> interestings = new List<float[]>();
        interestings.AddRange(problem.pointsOfInterest);
        interestings.AddRange(problem.goalPositions);
        interestings.AddRange(problem.startPositions);
        VisibilityGraph vGraph = new VisibilityGraph(problem.obstacles, interestings);
        instaTrainSom();
        List<Vector3[]> pathsForBois = som.getPathsForBois();
        
        //vGraph.drawGraph();

        for(int j = 0; j < pathsForBois.Count; j++)
        {
            Vector3[] boiPath = vGraph.getPath(pathsForBois[j]);
            for (int i = 0; i < boiPath.Length - 1; i++)
                Debug.DrawLine(boiPath[i], boiPath[i + 1], Color.red, 10000f);
        }
            
    }

    float getDt()
    {
        return Time.deltaTime * speed;
    }

    // Update is called once per frame
    bool trained = false;
    List<Vector3[]> boisPaths;
    bool havePaths = false;
    void Update ()
    {
        if (false)
        {
            if (!trained)
            {
                trained = trainSom();
                return;
            }

            if (!havePaths)
            {
                boisPaths = som.getPathsForBois();
                havePaths = true;
                return;
            }

            som.drawPaths(Time.deltaTime);
        }
        
    }

    void moveAll()
    {
       
    }


    float currentTime = 0f;
    static int startingHood = 3;
    int trainCounter = 0;
    int hood = startingHood;
    int hoodFixer = 0;
    bool trainSom()
    {
  
        currentTime += Time.deltaTime;
        if (currentTime > drawTime)
        {
            currentTime = 0f;
            if (hoodFixer > epochs / (startingHood + 1))
            {
                hoodFixer = 0;
                hood--;
            }
            hoodFixer++;

            som.update(hood);
            som.drawState(drawTime);
            trainCounter++;
        }
        if (trainCounter >= epochs)
        {
            som.calculateBoisVisitOrder();
            return true;
        }
        return false;
    }

    void instaTrainSom()
    {
        for (int i = 0; i < epochs; i++)
        {
            if (hoodFixer > epochs / (startingHood + 1))
            {
                hoodFixer = 0;
                hood--;
            }
            hoodFixer++;
            som.update(hood);
            som.drawState(drawTime);
        }
        
        som.calculateBoisVisitOrder();
        
    }

    void spawnObjects()
    {
        //Spawn bounding wall
        if(problem.boundingPolygon.Count != 0)
            spawnObject(problem.boundingPolygon, "boundingPolygon", boundingObject);
        for (int i = 0; i < problem.obstacles.Count; i++)
            spawnObject(problem.obstacles[i], "obstacle_" + i, boundingObject);
        //Spawn vehicles at start position
        for (int i = 0; i<problem.startPositions.Count; i++)
            bois.Add(spawnBoi(problem.startPositions[i], "boi_" + i, Mathf.PI));

        for (int i = 0; i < problem.pointsOfInterest.Count; i++)
            points.Add(spawnPoint(problem.pointsOfInterest[i], "point_" + i, pointsBoundingObject));
        stretchField();
    }

    void stretchField()
    {
        Transform field = this.gameObject.transform;
        field.position = new Vector3((boundingMaxX + boundingMinX) / 2, 0, (boundingMaxZ + boundingMinZ) / 2);
        field.localScale = new Vector3((boundingMaxX - boundingMinX) / 10, 1, (boundingMaxZ - boundingMinZ) / 10);
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
        wall.transform.parent = parent.transform;
        wall.GetComponent<Renderer>().material = wallMaterial;
        wall.transform.rotation = Quaternion.LookRotation(polygonSide);
        wall.transform.localScale = new Vector3(wallThickness, wallHeight, width);
    }

    GameObject spawnPoint(float [] origin, string name, GameObject parent)
    {
        Vector3 pos = new Vector3(origin[0], pointHeight, origin[1]);
        GameObject point = Instantiate(pointsObject, parent.transform, true);
        point.name = name;
        point.transform.position = pos;
        //maybe random rotation hehe

        return point;
    }

    MotionModelSalesBoi spawnBoi(float[] origin, string name, float orientation)
    {
        Vector3 newOrientation = new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        Vector3 position = new Vector3(origin[0], boisHeight, origin[1]);
        GameObject vehicle = Instantiate(boisObject, boisBoundingObject.transform, true);
        vehicle.name = name;
        vehicle.transform.position = position;
        vehicle.transform.forward = newOrientation;
        MotionModelSalesBoi mm = vehicle.AddComponent<MotionModelSalesBoi>();
        mm.setParams(problem.vehicle_v_max, problem.vehicle_L, problem.vehicle_phi_max, problem.vehicle_a_max);
        return mm;
    }
}
