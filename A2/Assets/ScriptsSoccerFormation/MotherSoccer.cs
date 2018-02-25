using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotherSoccer : MonoBehaviour {

    ProblemSoccer problem;
    public string problemPath = "P26.json";
    public string trajPath = "P26_27_traj.json";
    public GameObject boundingObject;
    public float wallHeight = 4f;
    public float wallThickness = 0.5f;
    public Material wallMaterial;
    public GameObject playerBinderObject;
    public GameObject playerObject;
    public float playerHeight = 0f;
    public GameObject chaseObject;
    public float chaseHeight = 0f;
    float boundingMinX, boundingMinZ = float.MaxValue;
    float boundingMaxX, boundingMaxZ = float.MinValue;
    public float speed = 5f;
    int currentPosition = 0;
    VirtualConstructSoccer VC;
    List<MotionModelSoccer> players;
    GameObject ronaldo;
    float velocityGoal = 0f;

    int ronaldoKeeper = 0;
    int ronaldoSwitcher = 0;
    // Use this for initialization
    void Start () {
        problem = ProblemSoccer.Import(problemPath, trajPath);
        players = new List<MotionModelSoccer>();
        spawnObjects();
        VC = new VirtualConstructSoccer(problem.formationPositions, playerHeight, boundingMinX, boundingMaxX, boundingMinZ, boundingMaxZ);
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
            Vector3 position = new Vector3(problem.trajectory[problem.trajectory.Count-1][0], playerHeight, problem.trajectory[problem.trajectory.Count-1][1]);
            for (int i = 0; i < players.Count; i++)
            {
                players[i].moveTowards(VC.getPosition(i, position), getDt(), velocityGoal);
            }
        }
        

    }

    Vector3 getRonaldoPosition()
    {
        Vector3 ronaldoPosition = ronaldo.transform.position;
        ronaldoPosition.y = playerHeight;
        return ronaldoPosition;
    }

    void updateRonaldoKeeper(Vector3 position)
    {
        Vector3 ronaldoPosition = getRonaldoPosition();
        for (int i = 0; i<players.Count; i++)
        {
            if ((VC.getPosition(i, position) - ronaldoPosition).magnitude < (VC.getPosition(ronaldoKeeper, position) - ronaldoPosition).magnitude){
                ronaldoSwitcher = i;
            }
        }

        if ((players[ronaldoSwitcher].transform.position - ronaldoPosition).magnitude < chaseHeight*4)
        {
            ronaldoKeeper = ronaldoSwitcher;
        }
    }

    void moveAll()
    {
        Vector3 position = new Vector3(problem.trajectory[currentPosition][0], playerHeight, problem.trajectory[currentPosition][1]);
        for (int i = 0; i<players.Count;i++)
        {
            if (i == ronaldoKeeper || i == ronaldoSwitcher)
                players[i].moveTowards(getRonaldoPosition(), getDt(), velocityGoal);
            else
                players[i].moveTowards(VC.getPosition(i, position), getDt(), velocityGoal);
        }
        updateRonaldoKeeper(position);
    }

    bool checkStartPositions()
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].inFormation)
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
            Vector3 oldPos = ronaldo.transform.position;
            ronaldo.transform.position = new Vector3(problem.trajectory[currentPosition][0], chaseHeight, problem.trajectory[currentPosition][1]);
            ronaldo.transform.forward =  new Vector3(Mathf.Cos(problem.theta[currentPosition]), 0, Mathf.Sin(problem.theta[currentPosition]));
            velocityGoal = (oldPos - ronaldo.transform.position).magnitude/problem.vehicle_dt;
        }
        
    }

    void stretchField()
    {
        Transform field = this.gameObject.transform;
        field.position = new Vector3((boundingMaxX + boundingMinX) / 2, 0, (boundingMaxZ + boundingMinZ) / 2);
        field.localScale = new Vector3((boundingMaxZ - boundingMinZ) / 10, 1, (boundingMaxX - boundingMinX) / 10);
    }
  
    void spawnObjects()
    {
        //Spawn bounding wall
        if(problem.boundingPolygon.Count != 0)
            spawnObject(problem.boundingPolygon, "boundingPolygon", boundingObject);
        //Spawn vehicles at start position
        for (int i = 0; i<problem.startPositions.Count; i++)
            players.Add(spawnPlayers(problem.startPositions[i], "player_" + i, Mathf.PI));
        ronaldo = spawnRonaldo(problem.trajectory[0], "Ronaldo", problem.theta[0]);
        stretchField();
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

    MotionModelSoccer spawnPlayers(float[] origin, string name, float orientation)
    {
        Vector3 newOrientation = new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        Vector3 position = new Vector3(origin[0], playerHeight, origin[1]);
        GameObject player = Instantiate(playerObject, playerBinderObject.transform, true);
        player.name = name;
        player.transform.position = position;
        player.transform.forward = newOrientation;
        MotionModelSoccer mm = player.AddComponent<MotionModelSoccer>();
        mm.setParams(problem.vehicle_v_max, problem.vehicle_L, problem.vehicle_phi_max, problem.vehicle_a_max);
        return mm;
    }

    GameObject spawnRonaldo(float[] origin, string name, float orientation)
    {
        Vector3 newOrientation = new Vector3(Mathf.Cos(orientation), 0, Mathf.Sin(orientation));
        Vector3 position = new Vector3(origin[0], chaseHeight, origin[1]);
        GameObject player = Instantiate(chaseObject, playerBinderObject.transform, true);
        player.name = name;
        player.transform.position = position;
        player.transform.forward = newOrientation;
        return player;
    }
}
