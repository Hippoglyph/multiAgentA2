using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MotionModelVacuum : MonoBehaviour {

    float vMax;
    Vector3[] path;
    Transform myTransform;
    int nextPoint;
    float radius;

    public void setParams(float vMax, float radius)
    {
        this.vMax = vMax;
        this.radius = radius;
        myTransform = this.gameObject.transform;
    }

    public void drawRadius(float dt)
    {
        float res = 2 * Mathf.PI / 180;
        for (float i = 0; i < 2*Mathf.PI; i += res)
        {
            Vector3 from = getPosition() + new Vector3(radius * Mathf.Cos(i), 0, radius * Mathf.Sin(i));
            Vector3 to = getPosition() + new Vector3(radius * Mathf.Cos(i + res), 0, radius * Mathf.Sin(i + res));
            Debug.DrawLine(from, to, Color.red, dt);
        }
           
    }

    public void addPath(Vector3[] path)
    {
        this.path = path;
        nextPoint = 0;
    }

    public bool moveTowards(float dt)
    {
        if (nextPoint >= path.Length)
            return false;
        if ((myTransform.position - path[nextPoint]).sqrMagnitude < vMax * dt)
        {
            myTransform.position = path[nextPoint];
            nextPoint++;
            return true;
        }
        myTransform.position += Vector3.ClampMagnitude((path[nextPoint] - myTransform.position).normalized * vMax * dt, (path[nextPoint] - myTransform.position).magnitude);
        myTransform.rotation = Quaternion.RotateTowards(myTransform.rotation, Quaternion.LookRotation((path[nextPoint] - myTransform.position)), dt * 360f);
        return false;
    }

    public Vector3 getPosition()
    {
        return myTransform.position;
    }
}
