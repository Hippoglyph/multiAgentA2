using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MotionModel : MonoBehaviour {

    float vMax, length, phiMax, aMax;
    public bool inFormation = false;
    Vector3 velocity;

    public void setParams(float vMax, float length, float phiMax, float aMax)
    {
        this.vMax = vMax;
        this.length = length;
        this.phiMax = phiMax;
        this.aMax = aMax;
        velocity = Vector3.zero;
    }

    public void moveTowards(Vector3 targetPos, float dt, float velocityGoal)
    {

        /*
        Transform car = this.gameObject.transform;
        Vector3 path = targetPos - car.position;
        if (path.magnitude < vMax * dt)
        {
            inFormation = true;
            return;
        }
        else
            inFormation = false;

        if (path.magnitude < vMax * dt * 35)
        {
            if (Vector3.Angle(car.forward, path) * Mathf.Deg2Rad > Mathf.PI*0.75)
                return;
        }
        float angle = Vector3.Angle(car.forward, path);
        float orientation = Vector3.Angle(Vector3.right, car.forward);
        if (car.forward.z < 0)
            orientation = -orientation;
        float phi = 0;
        if (angle > 0.00001)
        {
            Vector3 cross = Vector3.Cross(path, car.forward);
            float partTurn = (((vMax / length) * Mathf.Tan(phiMax)) * Mathf.Rad2Deg * dt) / angle;
            if (partTurn <= 1.0f)
                phi = phiMax * Mathf.Sign(cross.y);
            else
                phi = Mathf.Atan((angle * Mathf.Deg2Rad * length / vMax)) * Mathf.Sign(cross.y);
        }
        float deltaTheta = (((vMax / length) * Mathf.Tan(phi)) * dt);
        float newAngle = deltaTheta + orientation * Mathf.Deg2Rad;
        Vector3 newOrientation = new Vector3(Mathf.Cos(newAngle), 0, Mathf.Sin(newAngle));
        float xMove = vMax * Mathf.Cos(newAngle) * dt;
        float yMove = vMax * Mathf.Sin(newAngle) * dt;
        Vector3 newPosition = new Vector3(car.position.x + xMove, car.position.y, car.position.z + yMove);

        car.position = newPosition;
        car.forward = newOrientation;
        */



        Transform car = this.gameObject.transform;
        Vector3 path = targetPos - car.position;
        float accelerate = aMax;
        Vector3 direction = (targetPos - car.position + velocity * dt).normalized;

        //Debug.Log(velocityGoal);


        if (velocity.magnitude - aMax * dt < 0 && path.magnitude < vMax*dt)
        {
            velocity = Vector3.zero;
            inFormation = true;
            return;
        }
        else
            inFormation = false;

        if (path.magnitude < (velocity.sqrMagnitude - Mathf.Pow(velocityGoal * 0.9f, 2)) / (2 * aMax * dt))
        {
            direction = -velocity.normalized;
            accelerate = Mathf.Min(path.magnitude, aMax);
        }


        Vector3 deltaVel = direction * accelerate * dt;
        Vector3 newVel = Vector3.ClampMagnitude(velocity + deltaVel, vMax);
        float xMove = newVel.x * dt;
        float yMove = newVel.z * dt;
        Vector3 newPosition = new Vector3(car.position.x + xMove, car.position.y, car.position.z + yMove);
        Vector3 newOrientation = path.normalized;
        float xVel = xMove / dt;
        float zVel = yMove / dt;

        car.position = newPosition;
        velocity = new Vector3(xVel, 0, zVel);
        car.forward = newOrientation;
    }
}
