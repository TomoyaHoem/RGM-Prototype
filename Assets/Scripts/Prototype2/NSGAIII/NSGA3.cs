using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NSGA3
{
    //input copy of original reference point because they will be modified during algorithm
    public static List<GameObject> NSGAIII(List<List<GameObject>> fronts, List<ReferencePoint> referencePoints, int populationSize)
    {
        //step 1 - 4 in algorithm, if not already done
        List<GameObject> nextPopulation = new List<GameObject>();
        List<GameObject> constructPop = new List<GameObject>();

        int currentFrontIndex = 0;

        //step 5-8
        while(constructPop.Count < populationSize)
        {
            foreach(GameObject g in fronts[currentFrontIndex])
            {
                constructPop.Add(g);
            }
            currentFrontIndex++;
        }
        int lastFrontIndex = currentFrontIndex-1;

        //remove useless fronts
        fronts.RemoveRange(lastFrontIndex+1, fronts.Count - (lastFrontIndex+1));

        //step 9-10
        if(constructPop.Count == populationSize)
        {
            nextPopulation = constructPop;
            return nextPopulation;
        } else //step 11-12
        {
            for (int i = 0; i < lastFrontIndex; i++)
            {
                foreach (GameObject g in fronts[i])
                {
                    nextPopulation.Add(g);
                }
            }
            //step 13-14
            int pointsToChoose = populationSize - nextPopulation.Count;
            List<float> idealPoint = Normalization.ComputeIdealPoint(fronts);
            List<GameObject> extremePoints = Normalization.FindExtremePoints(fronts);
            List<float> intercepts = Normalization.ConstructHyperplane(extremePoints);

            Normalization.NormalizeObjectives(fronts, intercepts, idealPoint);

            //step 15
            Niching.Associate(referencePoints, fronts);

            //step 16-17
            while (pointsToChoose > 0)
            {
                //if NichePreservation finds individual pointsToChoose will be decremented by 1 else by 0
                pointsToChoose -= Niching.NichePreservation(referencePoints, nextPopulation);
            }
        }
        
        //step 18
        return nextPopulation;
    }
}
