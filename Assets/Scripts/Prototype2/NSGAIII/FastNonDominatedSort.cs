using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FastNonDominatedSort
{
    //fast non dominated sort based on original NSGA-II paper & jMetal implementation
    public static List<List<GameObject>> CalculateFronts(List<GameObject> population)
    {
        //fronts[i] contains list of individuals belonging to front i+1 -> 1, 2, 3, ...
        List<List<GameObject>> fronts = new List<List<GameObject>>();
        //initialize first front
        if(population.Count > 0)
        {
            fronts.Add(new List<GameObject>());
        }

        //dominateMeCount[i] -> contains the count of number of individuals that dominate i
        int[] dominateMeCount = new int[population.Count];
        //iDominateList[i] -> contains list of individuals i dominates
        List<List<GameObject>> iDominateList = new List<List<GameObject>>();

        //pairwise compare each individual and calculate np and Sp
        for(int p = 0; p < population.Count; p++)
        {
            //initialize iDominateList for each individual
            iDominateList.Add(new List<GameObject>());
            for (int q = 0; q < population.Count; q++)
            {
                //if p dominates q -> add q to iDominateList
                if(Dominates(population[p], population[q]))
                {
                    iDominateList[p].Add(population[q]);
                //else if q dominates p -> increase dominateMeCount for p
                } else if(Dominates(population[q], population[p]))
                {
                    dominateMeCount[p]++;
                }
            }
            //if dominateMeCount[p] = 0 -> p not dominated -> belongs to first front
            if(dominateMeCount[p] == 0)
            {
                fronts[0].Add(population[p]);
            }
        }

        //determine remaining fronts

        //next front index, currentFront init as first front
        int frontIndex = 1; List<GameObject> currentFront = fronts[0];

        //for each individual of currentFront
        for(int p = 0; p < currentFront.Count; p++)
        {
            //get index of that individual in population
            int indexP = population.IndexOf(currentFront[p]);
            //decrease dominateMeCount of each individual that is dominated by p
            for (int q = 0; q < iDominateList[indexP].Count; q++)
            {
                //dominateMeCount at index of qth individual in population in iDominateList at p
                dominateMeCount[population.IndexOf(iDominateList[indexP][q])]--;
                //if the dominateMeCount is 0 after -> belongs to next front
                if (dominateMeCount[population.IndexOf(iDominateList[indexP][q])] == 0)
                {
                    //create new front if necessary
                    if(fronts.Count <= frontIndex)
                    {
                        fronts.Add(new List<GameObject>());
                    }
                    fronts[frontIndex].Add(iDominateList[indexP][q]);
                }
            }
            //if unchecked fronts left
            if(p == currentFront.Count-1 && fronts.Count > frontIndex)
            {
                //go to next front & increse counter
                currentFront = fronts[frontIndex];
                frontIndex++;
                //reset p
                p = -1;
            }
        }
        
        return fronts;
    }

    //returns true if a dominates b, else false
    //a dominates b, iff foreach fitness value x of a and fitness value y of b, x <= y & there exists one x where x < y
    private static bool Dominates(GameObject a, GameObject b)
    {
        List<float> fitnessA = a.GetComponent<Machine>().FitnessVals;
        List<float> fitnessB = b.GetComponent<Machine>().FitnessVals;

        bool dominates = false;

        //check if any fitness value of b is less than of a -> if so a does not dominate b
        //if no fitness value of b is less than of a, check if one value of a is less than one of b -> a dominates b
        for(int i = 0; i < fitnessA.Count; i++)
        {
            if (fitnessB[i] < fitnessA[i])
            {
                dominates = false;
                break;
            }
            if(fitnessA[i] < fitnessB[i])
            {
                dominates = true;
            }
        }

        return dominates;
    } 
}
