using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionModel : MonoBehaviour {

    float vMax, length, phiMax;

    public void setParams(float vMax, float length, float phiMax)
    {
        this.vMax = vMax;
        this.length = length;
        this.phiMax = phiMax;
    }

    public void moveTowards(Vector3 targetPos, float dt)
    {
        Transform car = this.gameObject.transform;
        Vector3 path = targetPos - car.position;
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
    }
}
