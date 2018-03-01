using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class MotionModelSwedish : MonoBehaviour {

    float radius;
    float vMax;
    float dt;
    int myIndex;
    Transform myTransform;
    Vector3 velocity;
    Vector3 goal;

    public void setParams(float vMax, float radius, float dt, int myIndex, Vector3 goal)
    {
        this.vMax = vMax;
        this.radius = radius;
        this.dt = dt;
        this.myIndex = myIndex;
        velocity = Vector3.zero;
        myTransform = this.gameObject.transform;
        this.goal = goal;
    }

    public void moveTowards(float dt)
    {
        myTransform.position += velocity * dt;
        
       /// myTransform.forward = velocity;
    }

    public void drawVO(List<MotionModelSwedish> neighbors)
    {
        foreach(List<float[]> obstacle in getVelocityObstacles(neighbors))
        {
            Vector3 p1 = new Vector3(obstacle[0][0], 1, obstacle[0][1]);
            Vector3 p2 = new Vector3(obstacle[1][0], 1, obstacle[1][1]);
            Vector3 p3 = new Vector3(obstacle[2][0], 1, obstacle[2][1]);
            Debug.DrawLine(p1, p2, Color.red, Time.deltaTime);
            Debug.DrawLine(p2, p3, Color.red, Time.deltaTime);
            Debug.DrawLine(p3, p1, Color.red, Time.deltaTime);
        }
    }

    public void drawVelocity()
    {
        Debug.DrawLine(getPosition(), getPosition() + velocity, Color.red, Time.deltaTime);
    }

    List<List<float[]>> getVelocityObstacles(List<MotionModelSwedish> neighbors)
    {
        List<List<float[]>> VO = new List<List<float[]>>();
        for(int i = 0; i < neighbors.Count; i++)
        {
            if(i != myIndex)
                VO.Add(getVelocityObstacle(neighbors[i]));
        }

        return VO;
    }

    List<float[]> getVelocityObstacle(MotionModelSwedish buddy)
    {
        
        List<float[]> vertices = new List<float[]>();

        //Skip if buddy to far away??1

        float theta = Mathf.Acos((buddy.getRadius() + getRadius()) / ((buddy.getPosition() - getPosition()).magnitude));
        Vector3 D = getPosition() - buddy.getPosition();
        float baseAngle = Vector3.Angle(Vector3.right, D) * Mathf.Deg2Rad;
        if (D.z < 0)
            baseAngle = -baseAngle;
        float x1 = buddy.getPosition().x + (buddy.getRadius() + getRadius()) * Mathf.Cos(baseAngle + theta);
        float z1 = buddy.getPosition().z + (buddy.getRadius() + getRadius()) * Mathf.Sin(baseAngle + theta);

        float x2 = buddy.getPosition().x + (buddy.getRadius() + getRadius()) * Mathf.Cos(baseAngle - theta);
        float z2 = buddy.getPosition().z + (buddy.getRadius() + getRadius()) * Mathf.Sin(baseAngle - theta);


        Vector3 dir1 = new Vector3(x1 - getPosition().x, 0, z1 - getPosition().z).normalized;
        Vector3 dir2 = new Vector3(x2 - getPosition().x, 0, z2 - getPosition().z).normalized;
        Vector3 point1 = getPosition() + dir1 * vMax * dt;
        Vector3 point2 = getPosition() + dir2 * vMax * dt;

        point1 = point1 + buddy.getVelocity();
        point2 = point2 + buddy.getVelocity();
        Vector3 point3 = getPosition() + buddy.getVelocity();

        vertices.Add(new float[] { point1.x, point1.z });
        vertices.Add(new float[] { point2.x, point2.z });
        vertices.Add(new float[] { point3.x, point3.z });
        return vertices;
    }

    public void calculateVelocity(List<MotionModelSwedish> neighbours, int myIndex, float time)
    {
        List<OrcaLine> orcalines = new List<OrcaLine>();
        for (int i = 0; i<neighbours.Count; i++)
        {
            if (i != myIndex)
            {
                orcalines.Add(getOrcaLine(neighbours[i], time));

            }
        }
        Vector3 newVelocity = Vector3.zero;
        LPSolver.linearProgram1(orcalines, orcalines.Count-1, vMax, (goal - getPosition()).normalized*vMax, ref newVelocity);

        velocity = newVelocity;
    }

    OrcaLine getOrcaLine(MotionModelSwedish buddy, float time)
    {
        Vector3 relPos = buddy.getPosition() - getPosition();
        Vector3 relVel = getVelocity() - buddy.getVelocity();
        float combRadius = buddy.getRadius() + getRadius();

        OrcaLine orcaline = new OrcaLine();
        Vector3 u;

        Vector3 w = relVel - (relPos / time);
        float decBoundary = Vector3.Dot(w, relPos);

        if (decBoundary >= 0.0f && Mathf.Pow(decBoundary,2) <= Mathf.Pow(combRadius,2) * w.sqrMagnitude)
        {
            // Legs
            float distance = Mathf.Sqrt(relPos.sqrMagnitude - Mathf.Pow(combRadius, 2));

            if (Vector3.Cross(relPos,w).y > 0.0f)
                orcaline.direction = new Vector3(relPos.x * distance - relPos.z * combRadius, 0.0f, relPos.x * combRadius + relPos.z * distance).normalized;
            else
                orcaline.direction = -new Vector3(relPos.x * distance + relPos.z * combRadius, 0.0f, -relPos.x * combRadius + relPos.z * distance).normalized;
            float uLen = Vector3.Dot(relVel, orcaline.direction);
            u = uLen * orcaline.direction - relVel;
        }
        else
        {
            orcaline.direction = new Vector3(w.normalized.z, 0.0f, -w.normalized.x);
            u = ((combRadius/time) - w.magnitude ) *w.normalized;
        }
        orcaline.point = getVelocity() + 0.5f * u;
       
        return orcaline;
    }

    public float getRadius()
    {
        return radius;
    }

    public Vector3 getVelocity()
    {
        return velocity;
    }

    public Vector3 getPosition()
    {
        return myTransform.position;
    }

    
}

public class OrcaLine
{
    public Vector3 direction;
    public Vector3 point;
}
