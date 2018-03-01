using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LPSolver  {

    // Use this for initialization
    public static void linearProgram3(List<OrcaLine> lines, int numObstLines, int beginLine, float radius, ref Vector3 result)
    {
        float RVO_EPSILON = 0.00001f;
        float distance = 0.0f;

        for (int i = beginLine; i < lines.Count; ++i)
        {
            if (Vector3.Cross(lines[i].direction, lines[i].point - result).y < distance)
            {
                /* Result does not satisfy constraint of line i. */
                List<OrcaLine> projLines = new List<OrcaLine>();
                for (int ii = 0; ii < numObstLines; ++ii)
                {
                    projLines.Add(lines[ii]);
                }

                for (int j = numObstLines; j < i; ++j)
                {
                    OrcaLine line = new OrcaLine();

                    float determinant = -Vector3.Cross(lines[i].direction, lines[j].direction).y;

                    if (Mathf.Abs(determinant) <= RVO_EPSILON)
                    {
                        /* Line i and line j are parallel. */
                        if (Vector3.Dot(lines[i].direction, lines[j].direction) > 0.0f)
                        {
                            /* Line i and line j point in the same direction. */
                            continue;
                        }
                        else
                        {
                            /* Line i and line j point in opposite direction. */
                            line.point = 0.5f * (lines[i].point + lines[j].point);
                        }
                    }
                    else
                    {
                        line.point = lines[i].point + (-Vector3.Cross(lines[j].direction, lines[i].point - lines[j].point).y / determinant) * lines[i].direction;
                    }

                    line.direction = (lines[j].direction - lines[i].direction).normalized;
                    projLines.Add(line);
                }

                Vector2 tempResult = result;
                if (false)//linearProgram2(projLines, radius, new Vector2(-lines[i].direction.y(), lines[i].direction.x()), true, ref result) < projLines.Count)
                {
                    /*
                     * This should in principle not happen. The result is by
                     * definition already in the feasible region of this
                     * linear program. If it fails, it is due to small
                     * floating point error, and the current result is kept.
                     */
                    result = tempResult;
                }

                distance = -Vector3.Cross(lines[i].direction, lines[i].point - result).y;
            }
        }
    }


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
            float denominator = -Vector3.Cross(lines[lineNo].direction, lines[i].direction).y;
            float numerator = -Vector3.Cross(lines[i].direction, lines[lineNo].point - lines[i].point).y;

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
