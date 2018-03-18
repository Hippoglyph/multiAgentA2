using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherSwedish : MonoBehaviour {

    ProblemSwedish problem;
    public string problemPath = "P21.json";
    public GameObject boundingObject;
    public float wallHeight = 4f;
    public float wallThickness = 0.5f;
    public Material wallMaterial;
    public GameObject actorsBoundingObject;
    public GameObject actorObject;
    float boundingMinX, boundingMinZ = float.MaxValue;
    float boundingMaxX, boundingMaxZ = float.MinValue;
    public float actorHeight = 0f;
    public float actorRadius = 0.5f;
    public float speed = 5f;
    public float time = 2f;

    List<MotionModelSwedish> actors;

    // Use this for initialization  
    void Start () {
        problem = ProblemSwedish.Import(problemPath);
        actors = new List<MotionModelSwedish>();
        spawnObjects();
    }

    public void shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rnd = UnityEngine.Random.Range(0, array.Length);
            int tempGO = array[rnd];
            array[rnd] = array[i];
            array[i] = tempGO;
        }
    }

    float getDt()
    {
        return Time.deltaTime * speed;
        //return problem.vehicle_dt * speed;
    }

    // Update is called once per frame
	void Update () {
        moveAll();
    }

    void moveAll()
    {
        int[] order = new int[actors.Count];
        for (int i = 0; i < actors.Count; i++)
            order[i] = i;
        shuffle(order);
        foreach (int i in order)
        {
            actors[i].calculateVelocity(actors, i, time, getDt());
        }


        for (int i = 0; i < actors.Count; ++i)
        {
            actors[i].moveTowards(getDt());
            actors[i].drawVelocity();
        }
    }
  
    void spawnObjects()
    {
        //Spawn bounding wall
        if(problem.boundingPolygon.Count != 0)
            spawnObject(problem.boundingPolygon, "boundingPolygon", boundingObject);
        for(int i = 0; i < problem.obstacles.Count; i++)
            spawnObject(problem.obstacles[i], "obstacle_"+i, boundingObject);

        //Spawn bois at start position
        //for (int i = 0; i<problem.startPositions.Count; i++)
        //    actors.Add(spawnActor(problem.startPositions[i], "actor_" + i, Mathf.PI, i, problem.goalPositions[i]));

        //JOUST
        actors.Add(spawnActor(problem.startPositions[0], "Arthur", Mathf.PI, 0, problem.startPositions[problem.startPositions.Count-1]));
        actors.Add(spawnActor(problem.startPositions[problem.startPositions.Count-1], "Maximillian", Mathf.PI, 1, problem.startPositions[0]));
        //actors.Add(spawnActor(problem.startPositions[(problem.startPositions.Count-1)/2], "Arestoteles", Mathf.PI, 2, problem.goalPositions[(problem.startPositions.Count - 1) / 2]));
        //actors.Add(spawnActor(problem.goalPositions[(problem.startPositions.Count - 1) / 2], "Hans", Mathf.PI, 3, problem.startPositions[(problem.startPositions.Count - 1) / 2]));



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

    MotionModelSwedish spawnActor(float[] origin, string name, float orientation, int index, float[] goalPoint)
    {
        Vector3 newOrientation = new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        Vector3 position = new Vector3(origin[0], actorHeight, origin[1]);
        Vector3 goal = new Vector3(goalPoint[0], actorHeight, goalPoint[1]);
        GameObject actor = Instantiate(actorObject, actorsBoundingObject.transform, true);
        actor.name = name;
        actor.transform.position = position;
        actor.transform.forward = newOrientation;
        MotionModelSwedish mm = actor.AddComponent<MotionModelSwedish>();
        mm.setParams(problem.vehicle_v_max, actorRadius, 3f, index, goal);
        return mm;
    }
}
