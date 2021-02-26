using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ReferencePointCalculator
{
    //returns approx numPoints uniformly distributed points with numObjectives on the unit hyperplane
    public static List<ReferencePoint> CalculateReferencePoints(int numPoints, int numObjectives)
    {
        List<ReferencePoint> referencePoints = new List<ReferencePoint>();
        List<float> Position = new List<float>();

        for (int i = 0; i < numObjectives; i++)
        {
            Position.Add(0);
        }

        RecursiveReferencePointCalculation(referencePoints, Position, numObjectives, numPoints, numPoints, 0);

        return referencePoints;
    }

    private static void RecursiveReferencePointCalculation(List<ReferencePoint> refPoints, List<float> curP, int numO, int left, int total, int element)
    {
        if (element == (numO - 1))
        {
            curP[element] = (float)left / total;
            refPoints.Add(new ReferencePoint(new List<float>(curP)));
        }
        else
        {
            for (int i = 0; i <= left; i += 1)
            {
                curP[element] = (float)i / total;

                RecursiveReferencePointCalculation(refPoints, curP, numO, left - i, total, element + 1);
            }
        }
    }
}
