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
        myTransform = this.gameObject.transform;
        this.goal = goal;
        this.velocity = getPrefVel();
    }

    public void moveTowards(float dt)
    {
        myTransform.position += velocity * dt;
        if (velocity != Vector3.zero)
            //myTransform.forward = velocity;
            myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, Quaternion.LookRotation(velocity), dt * 180f);
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
        for (float i = 0f; i < 2 * Mathf.PI; i += 2 * Mathf.PI / 100)
            Debug.DrawLine(getPosition() + new Vector3(getRadius() * Mathf.Cos(i- 2 * Mathf.PI / 100), 0f, getRadius() * Mathf.Sin(i- 2 * Mathf.PI / 100)), getPosition() + new Vector3(getRadius() * Mathf.Cos(i),0f, getRadius() * Mathf.Sin(i)),Color.red,Time.deltaTime) ;
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

    public void calculateVelocity(List<MotionModelSwedish> neighbours, int myIndex, float time, float dt)
    {
        float scanRadius = 10;
        List<OrcaLine> orcalines = new List<OrcaLine>();
        for (int i = 0; i < neighbours.Count; i++)
        {
            if (i != myIndex && (getPosition() - neighbours[i].getPosition()).sqrMagnitude < Mathf.Pow(getRadius()*2*scanRadius,2))
            {
                orcalines.Add(getOrcaLine(neighbours[i], time, dt));
            }
        }
        Vector3 newVelocity = getPrefVel();
        int reached = orcalines.Count;
        for (int i = 0; i < orcalines.Count; ++i)
        {
            if (Vector3.Cross(orcalines[i].direction, orcalines[i].point - newVelocity).y < 0.0f)
            {
                if (!LPSolver.linearProgram1(orcalines, i, vMax, getPrefVel(), ref newVelocity))
                {
                    newVelocity = Vector3.zero;
                    reached = i;
                    break;
                }
            }
        }

        if (reached < orcalines.Count)
        {
            LPSolver.linearProgram3(orcalines, orcalines.Count, reached, vMax, ref newVelocity);
        }

        velocity = newVelocity;
    }
  

    OrcaLine getOrcaLine(MotionModelSwedish buddy, float time, float dt)
    {
        Vector3 relPos = buddy.getPosition() - getPosition();
        Vector3 relVel = getVelocity() - buddy.getVelocity();
        float combRadius = buddy.getRadius() + getRadius();

        OrcaLine orcaline = new OrcaLine();
        Vector3 u;

        Vector3 w = relVel - (relPos / time);
        float decBoundary = Vector3.Dot(w, relPos);

        if (relPos.sqrMagnitude > Mathf.Pow(combRadius, 2))
        {
            if (!(decBoundary < 0.0f && Mathf.Pow(decBoundary, 2) > Mathf.Pow(combRadius, 2) * w.sqrMagnitude))
            {
                // Legs
                float distance = Mathf.Sqrt(relPos.sqrMagnitude - Mathf.Pow(combRadius, 2)); 

                if (Vector3.Cross(relPos, w).y < 0.0f)
                    orcaline.direction = new Vector3(relPos.x * distance - relPos.z * combRadius, 0.0f, relPos.x * combRadius + relPos.z * distance)/relPos.sqrMagnitude;
                else
                    orcaline.direction = -new Vector3(relPos.x * distance + relPos.z * combRadius, 0.0f, -relPos.x * combRadius + relPos.z * distance)/relPos.sqrMagnitude;
                float uLen = Vector3.Dot(relVel, orcaline.direction);
                u = uLen * orcaline.direction - relVel;
            }
            else
            {
                Vector3 wNorm = w.normalized;
                orcaline.direction = new Vector3(wNorm.z, 0.0f, -wNorm.x);
                u = ((combRadius / time) - w.magnitude) * wNorm;
            }
        }

        else
        {
            
            w = relVel - (relPos / dt);
            Vector3 wNorm = w.normalized;
            orcaline.direction = new Vector3(wNorm.z, 0.0f, -wNorm.x);
            u = ((combRadius / dt) - w.magnitude) * wNorm;
        }
        orcaline.point = getVelocity() + 0.5f * u;

        //Debug.DrawLine(getPosition() + orcaline.point - (Vector3.Cross(Vector3.up, orcaline.direction)).normalized * vMax, getPosition() + orcaline.point + (Vector3.Cross(Vector3.up, orcaline.direction)).normalized * vMax, Color.blue, Time.deltaTime);
        //Debug.DrawLine(getPosition() + orcaline.point, getPosition() + orcaline.point + orcaline.direction, Color.blue, Time.deltaTime);

        return orcaline;
    }

    Vector3 getPrefVel()
    {
        return Vector3.ClampMagnitude(goal - getPosition(),vMax);
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
