using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    List<Vector2> points;

    public Path(Vector2 start, Vector2 end, Vector2 dir, List<Vector2> midPoints)
    {
        points = new List<Vector2>
        {
            start,
            start + (Vector2.right * dir * 0.5f),
            end - (Vector2.right * dir * 0.5f),
            end,
        };
        for (int i = 0; i < midPoints.Count; i++)
        {
            InsertPoint(midPoints[i], dir);
        }
        Debug.DrawLine(points[points.Count - 4], points[points.Count - 1], Color.red, 100f);
    }

    private void InsertPoint(Vector2 newAnchor, Vector2 dir)
    {
        Vector2 lastToCurr = newAnchor - points[points.Count - 4];

        float controlPointDistance = lastToCurr.magnitude / 2;

        float angle = Vector2.SignedAngle(lastToCurr, dir);
        float ranAngle = UnityEngine.Random.Range(90 - angle, 180 - angle + 90);
        float radians = ranAngle * Mathf.Deg2Rad;

        Vector2 controlP1 = newAnchor + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * controlPointDistance * dir.x;
        Vector2 controlP2 = newAnchor - new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * controlPointDistance * dir.x;

        Debug.DrawLine(points[points.Count - 4], newAnchor, Color.red, 100f);
        Debug.DrawLine(newAnchor, controlP1, Color.blue, 100f);
        Debug.DrawLine(newAnchor, controlP2, Color.green, 100f);

        points.Insert(points.Count - 2, controlP1);
        points.Insert(points.Count - 2, newAnchor);
        points.Insert(points.Count - 2, controlP2);
    }

    public Vector2[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector2> evenlySpacedPoints = new List<Vector2>();
        evenlySpacedPoints.Add(points[0]);
        Vector2 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector2[] p = GetPointsInSegment(segmentIndex);
            float controlNetLength = Vector2.Distance(p[0], p[1]) + Vector2.Distance(p[1], p[2]) + Vector2.Distance(p[2], p[3]);
            float estimatedCurveLength = Vector2.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector2 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector2.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector2 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();
    }

    public Vector2 this[int i]
    {
        get
        {
            return points[i];
        }
    }

    public int NumPoints
    {
        get
        {
            return points.Count;
        }
    }

    public int NumSegments
    {
        get
        {
            return points.Count / 3;
        }
    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };
    }
}
