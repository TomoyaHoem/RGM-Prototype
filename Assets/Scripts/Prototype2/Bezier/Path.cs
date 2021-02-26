using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path
{
    List<Vector2> points;
    Vector2 direction;

    public Path(Vector2 start, Vector2 end, Vector2 dir, List<Vector2> midPoints)
    {
        points = new List<Vector2>
        {
            start,
            start + (Vector2.right * dir * ((start-midPoints[0]).magnitude/3)),
            end - (Vector2.right * dir * ((end-midPoints[midPoints.Count-1]).magnitude/3)),
            end,
        };
        direction = dir;
        for (int i = 0; i < midPoints.Count; i++)
        {
            //if point inbetween pass position of nextAnchor, if last pass end anchor
            Vector2 nextAnchor = i + 1 < midPoints.Count ? midPoints[i + 1] : end;
            InsertPoint(midPoints[i], nextAnchor);
        }
        Debug.DrawLine(points[points.Count - 4], points[points.Count - 1], Color.red, 100f);
    }

    private void InsertPoint(Vector2 newAnchor, Vector2 nextAnchorPoint)
    {
        //get vector from new point to previous and next anchor
        Vector2 offset1 = points[points.Count - 4] - newAnchor;
        Vector2 offset2 = nextAnchorPoint - newAnchor;
        //bisect angle between vectors and place points along line perpendicular to that
        Vector2 dir = offset1.normalized - offset2.normalized;
        dir.Normalize();

        //distance half the distance from neighbouring anchor point
        Vector2 controlP1 = newAnchor + dir * offset1.magnitude * 0.5f;
        Vector2 controlP2 = newAnchor + dir * -offset2.magnitude * 0.5f;

        Debug.DrawLine(points[points.Count - 4], newAnchor, Color.red, 100f);
        Debug.DrawLine(newAnchor, controlP1, Color.blue, 100f);
        Debug.DrawLine(newAnchor, controlP2, Color.green, 100f);

        points.Insert(points.Count - 2, controlP1);
        points.Insert(points.Count - 2, newAnchor);
        points.Insert(points.Count - 2, controlP2);


        //**METHOD FOR MANUALLY CALCULATING CONTORL POINT POSITIONS VIA ANGLE**//
        ////Vector from last anchor to new anchor
        //Vector2 lastToCurr = newAnchor - points[points.Count - 4];
        //float controlPointDistance = lastToCurr.magnitude / 2;
        //////signed angle between vector and direction vector 
        ////float angle = Vector2.SignedAngle(lastToCurr, dir);
        //////get random angle between given limits
        ////float ranAngle = UnityEngine.Random.Range(90 - angle, 180 - angle + 90);
        ////float radians = ranAngle * Mathf.Deg2Rad;
        ////Vector2 controlP1 = newAnchor + new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * controlPointDistance * dir.x;
        ////Vector2 controlP2 = newAnchor - new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * controlPointDistance * dir.x;
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

        //insert and add point for beginning and end platform
        evenlySpacedPoints.Insert(0, new Vector2(evenlySpacedPoints[0].x - 0.5f * direction.x, evenlySpacedPoints[0].y));
        evenlySpacedPoints.Add(new Vector2(evenlySpacedPoints[evenlySpacedPoints.Count - 1].x + 1f * direction.x, evenlySpacedPoints[evenlySpacedPoints.Count - 1].y));

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
