using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MotionModelSalesBoi : MonoBehaviour {

    float vMax;
    Vector3[] path;
    Transform myTransform;
    int nextPoint;

    public void setParams(float vMax)
    {
        this.vMax = vMax;

        myTransform = this.gameObject.transform;

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
