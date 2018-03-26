using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherCamera : MonoBehaviour {

    ProblemCamera problem;
    public string problemPath = "P24.json";
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
    float cooldown = 1f;
    public float waitTime = 1f;
    public bool drawFinalPath = false;
    public bool simulate = false;
    float scanningRadius;
    VisibilityGraphCamera vGraph;
    List<GameObject> points;
    List<MotionModelCamera> bois;
    SOMCamera som;
    Prims prim;
    // Use this for initialization
    void Start () {
        problem = ProblemCamera.Import(problemPath);
        bois = new List<MotionModelCamera>();
        points = new List<GameObject>();
        spawnObjects();
        prim = new Prims(problem.obstacles, problem.pointsOfInterest, scanningRadius);
        List<float[]> goodpoints = prim.getNewPointsOfInterest();
        som = new SOMCamera(goodpoints, problem.startPositions, problem.goalPositions);
        List<float[]> interestings = new List<float[]>();
        interestings.AddRange(goodpoints);
        interestings.AddRange(problem.goalPositions);
        interestings.AddRange(problem.startPositions);
        cooldown = waitTime;
        vGraph = new VisibilityGraphCamera(problem.obstacles, interestings);
        drawRadius(cooldown, goodpoints);
        Debug.Break();
    }

    float getDt()
    {
        return Time.deltaTime * speed;
    }

    // Update is called once per frame
    bool trained = false;
    bool drawnNodeState = false;
    bool drawnFinalState = false;
    void Update ()
    {
        
        if (cooldown >= 0)
        {
            cooldown -= Time.deltaTime;
            return;
        }
        if (simulate)
        {
            if (!trained)
            {
                trained = trainSom();
                return;
            }

            if (!drawnNodeState)
            {
                som.drawPaths(waitTime);
                cooldown = waitTime;
                drawnNodeState = true;
                return;
            }
            if (!drawnFinalState)
            {
                getFinalPaths(drawFinalPath);
                cooldown = waitTime;
                drawnFinalState = true;
                return;
            }

        }
        else if(!trained)
        {
            instaTrainSom();
            getFinalPaths(drawFinalPath);
        }
        if (trained)
        {
            moveAll();
        }
    }

    void moveAll()
    {
        for (int i = 0; i < bois.Count; i++)
        {
            bois[i].moveTowards(getDt());
            removeInterestPoint(bois[i].getPosition());
            bois[i].drawRadius(Time.deltaTime);
        }
    }

    void getFinalPaths(bool draw)
    {
        List<Vector3[]> pathsForBois = som.getPathsForBois();
        for (int j = 0; j < pathsForBois.Count; j++)
        {
            Vector3[] boiPath = vGraph.getPath(pathsForBois[j]);
            bois[j].addPath(boiPath);
            if (draw)
                for (int i = 0; i < boiPath.Length - 1; i++)
                  Debug.DrawLine(boiPath[i], boiPath[i + 1], som.getBoiColor(j), 10000f);
        }
    }

    void removeInterestPoint(Vector3 pos)
    {
        foreach(Transform child in pointsBoundingObject.transform)
        {
            if ((child.position - pos).sqrMagnitude <= (scanningRadius * scanningRadius) +0.2 && !prim.collides(child.position,pos))
            {
                GameObject.Destroy(child.gameObject);
            } 
        }
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
        }
        
        som.calculateBoisVisitOrder();
        trained = true;  
    }

    void spawnObjects()
    {
        //Spawn bounding wall
        if(problem.boundingPolygon.Count != 0)
            spawnObject(problem.boundingPolygon, "boundingPolygon", boundingObject);
        for (int i = 0; i < problem.obstacles.Count; i++)
            spawnObject(problem.obstacles[i], "obstacle_" + i, boundingObject);

        scanningRadius = (boundingMaxX - boundingMinX) * (boundingMaxZ - boundingMinZ);
        //Spawn vehicles at start position
        for (int i = 0; i<problem.startPositions.Count; i++)
            bois.Add(spawnBoi(problem.startPositions[i], "boi_" + i, Mathf.PI, scanningRadius));

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

    MotionModelCamera spawnBoi(float[] origin, string name, float orientation, float radius)
    {
        Vector3 newOrientation = new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        Vector3 position = new Vector3(origin[0], boisHeight, origin[1]);
        GameObject vehicle = Instantiate(boisObject, boisBoundingObject.transform, true);
        vehicle.name = name;
        vehicle.transform.position = position;
        vehicle.transform.forward = newOrientation;
        MotionModelCamera mm = vehicle.AddComponent<MotionModelCamera>();
        mm.setParams(problem.vehicle_v_max, radius);
        return mm;
    }

    public void drawRadius(float dt, List<float[]> points)
    {
        float res = 2 * Mathf.PI / 180;
        for (int j = 0; j < points.Count; j++)
        {
            Vector3 pos = new Vector3(points[j][0], 0f, points[j][1]);
            for (float i = 0; i < 2 * Mathf.PI; i += res)
            {
                Vector3 from = pos + new Vector3(scanningRadius * Mathf.Cos(i), 0, scanningRadius * Mathf.Sin(i));
                Vector3 to = pos + new Vector3(scanningRadius * Mathf.Cos(i + res), 0, scanningRadius * Mathf.Sin(i + res));
                Debug.DrawLine(from, to, Color.red, dt);
            }
        }

    }
}
