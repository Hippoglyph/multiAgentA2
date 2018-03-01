using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPSolver  {
     
    // Use this for initialization


    public static bool linearProgram1(List<OrcaLine> lines, int lineNo, float radius, Vector3 optVelocity, ref Vector3 result)
    {
        float RVO_EPSILON = 0.00001f;
        float dotProduct = Vector3.Dot(lines[lineNo].point , lines[lineNo].direction);
        float discriminant = Mathf.Sqrt(dotProduct) + Mathf.Sqrt(radius) - (lines[lineNo].point.sqrMagnitude);

        if (discriminant < 0.0f)
        {
            /* Max speed circle fully invalidates line lineNo. */
            return false;
        }

        float sqrtDiscriminant = Mathf.Sqrt(discriminant);
        float tLeft = -dotProduct - sqrtDiscriminant;
        float tRight = -dotProduct + sqrtDiscriminant;

        for (int i = 0; i < lineNo; ++i)
        {
            float denominator = Vector3.Cross(lines[lineNo].direction, lines[i].direction).y;
            float numerator = Vector3.Cross(lines[i].direction, lines[lineNo].point - lines[i].point).y;

            if (Mathf.Abs(denominator) <= RVO_EPSILON)
            {
                /* Lines lineNo and i are (almost) parallel. */
                if (numerator < 0.0f)
                {
                    return false;
                }

                continue;
            }

            float t2 = numerator / denominator;

            if (denominator >= 0.0f)
            {
                /* Line i bounds line lineNo on the right. */
                tRight = Mathf.Min(tRight, t2);
            }
            else
            {
                /* Line i bounds line lineNo on the left. */
                tLeft = Mathf.Max(tLeft, t2);
            }

            if (tLeft > tRight)
            {
                return false;
            }
        }

        /* Optimize closest point. */
        float t = Vector3.Dot(lines[lineNo].direction, (optVelocity - lines[lineNo].point));

        if (t < tLeft)
        {
            result = lines[lineNo].point + tLeft * lines[lineNo].direction;
        }
        else if (t > tRight)
        {
            result = lines[lineNo].point + tRight * lines[lineNo].direction;
        }
        else
        {
            result = lines[lineNo].point + t * lines[lineNo].direction;
        }


        return true;
    }
}
