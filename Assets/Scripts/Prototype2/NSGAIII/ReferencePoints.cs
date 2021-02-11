using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ReferencePoints
{

    //returns approx numPoints uniformly distributed points with numObjectives on the unit hyperplane
    public static List<List<float>> CalculateReferencePoints(int numPoints, int numObjectives)
    {
        List<List<float>> referencePoints = new List<List<float>>();
        List<float> currentPoint = new List<float>();

        for(int i = 0; i < numObjectives; i++)
        {
            currentPoint.Add(0);
        }

        RecursiveReferencePointCalculation(referencePoints, currentPoint, numObjectives, numPoints, numPoints, 0);

        return referencePoints;
    }

    private static void RecursiveReferencePointCalculation(List<List<float>> refPoints, List<float> curP, int numO, int left, int total, int element)
    {
        if(element == (numO - 1))
        {
            curP[element] = (float)left/total;
            refPoints.Add(new List<float>(curP));
        } else
        {
            for(int i = 0; i <= left; i+=1)
            {
                curP[element] = (float)i / total;

                RecursiveReferencePointCalculation(refPoints, curP, numO, left-i, total, element+1);
            }
        }
    }
}
