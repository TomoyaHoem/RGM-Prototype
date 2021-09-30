using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Niching
{
    public static void Associate(List<ReferencePoint> refPoints, List<List<GameObject>> fronts)
    {
        //for every individual find closest reference point
        for (int t = 0; t < fronts.Count; t++)
        {
            foreach (GameObject machine in fronts[t])
            {
                int minRefPointIndex = -1;
                float minDistance = float.MaxValue;
                for (int r = 0; r < refPoints.Count; r++)
                {
                    float d = PerpendicularDistance(refPoints[r].Position, machine.GetComponent<Machine>().TranslatedObjectives);
                    if (d < minDistance)
                    {
                        minDistance = d;
                        minRefPointIndex = r;
                    }
                }
                //if current individual is not in last front we only need to store that it is associated with the current reference point 
                //-> increase count of associated members, stored in reference point
                if (t + 1 != fronts.Count)
                {
                    refPoints[minRefPointIndex].AddMember();
                }
                else //if individual in last front -> store indivudal and distance as tuple property in reference point
                {
                    refPoints[minRefPointIndex].AddLastFrontMember(machine, minDistance);
                }
            }
        }
    }

    // PerpendicularDistance:
    // Given a direction vector (w1, w2) and a point P(x1, y1),
    // we want to find a point Q(x2, y2) on the line connecting (0, 0)-(w1, w2)
    // such that (x1-x2, y1-y2) is perpendicular to (w1, w2).
    //
    // Since Q is on the line (0, 0)-(w1, w2), it should be (w1*k, w2*k).
    // (x1-w1*k, y1-w2*k).(w1, w2) = 0. (inner product) | calculate dot product
    // => (x1-w1*k)*w1 + (y1-w2*k)*w2 = 0               | expand parentheses
    // => x1*w1 - w1^2*k + y1*w2 - w2^2*k = 0           | + w1^2*k & + w2^2*k
    // => x1*w1 + y1*w2 = w1^2*k + w2^2*k               | factorise k
    // => k(w1^2 + w2^2) = w1x1 + w2x2                  | / (w1^2+w2^2)
    // => k = (w1x1 + w2x2)/(w1^2 +w2^2).               
    //
    // After obtaining k, we have Q = (w1*k, w2*k) and the distance between P and Q.
    private static float PerpendicularDistance(List<float> direction, List<float> point)
    {
        float numerator = 0, denominator = 0;
        for (int i = 0; i < direction.Count; i++)
        {
            numerator += direction[i] * point[i];
            denominator += Mathf.Pow(direction[i], 2.0f);
        }
        float k = numerator / denominator;

        float d = 0;
        for (int i = 0; i < direction.Count; i++)
        {
            d += Mathf.Pow((k * direction[i]) - point[i], 2.0f);
        }
        return Mathf.Sqrt(d);
    }

    public static int NichePreservation(List<ReferencePoint> refPoints, List<GameObject> nextPopulation)
    {
        List<ReferencePoint> minPoints = new List<ReferencePoint>();

        //find the minimum count of associated members of each referencepoint
        int minMemberSize = int.MaxValue;
        for (int r = 0; r < refPoints.Count; r++)
        {
            minMemberSize = Mathf.Min(minMemberSize, refPoints[r].MemberCount);
        }
        //find all referencepoints with that min membercount
        foreach (ReferencePoint r in refPoints)
        {
            if (r.MemberCount == minMemberSize)
            {
                minPoints.Add(r);
            }
        }
        //choose a random one in case of multiples
        ReferencePoint current = minPoints[UnityEngine.Random.Range(0, minPoints.Count)];

        //check if there are any potential memebers from the last front
        if (current.LastFrontMembers.Count > 0)
        {
            //if there are some check if min membercount = 0
            if (minMemberSize == 0)
            {
                //choose individual with lowest distance to reference point from last front
                nextPopulation.Add(MinTuple(current.LastFrontMembers).Item1);
            }
            else
            {
                //choose random individual since an individual from a better front is already associated with current reference point
                nextPopulation.Add(current.LastFrontMembers[UnityEngine.Random.Range(0, current.LastFrontMembers.Count)].Item1);
            }
            current.AddMember();
            return 1;
        }

        //if not remove current reference point from consideration
        refPoints.Remove(current);
        return 0;
    }

    //gets individual with minimum distance to reference point
    private static Tuple<GameObject, float> MinTuple(List<Tuple<GameObject, float>> lastFrontMembers)
    {
        Tuple<GameObject, float> minIndv = lastFrontMembers[0];
        for (int i = 0; i < lastFrontMembers.Count; i++)
        {
            if (lastFrontMembers[i].Item2 < minIndv.Item2)
            {
                minIndv = lastFrontMembers[i];
            }
        }
        return minIndv;
    }
}
