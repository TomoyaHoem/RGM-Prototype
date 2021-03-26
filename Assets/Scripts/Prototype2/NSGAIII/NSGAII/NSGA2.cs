using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class NSGA2
{
    static int NUM_OBJECTIVES = 3;//SettingsReader.Instance.EASettings.FitFunc.Count;

    public static List<GameObject> NSGAII(List<List<GameObject>> fronts, int populationSize)
    {
        List<GameObject> nextPopulation = new List<GameObject>();

        //add fronts to next population until next population is full
        int currentFrontIndex = 0;
        while((nextPopulation.Count + fronts[currentFrontIndex].Count) <= populationSize)
        {
            CalcCrowdingDistance(fronts[currentFrontIndex]);
            foreach (GameObject machine in fronts[currentFrontIndex])
            {
                nextPopulation.Add(machine);
            }
            currentFrontIndex++;
        }
        //if perfect fit return
        if(nextPopulation.Count == populationSize)
        {
            return nextPopulation;
        }
        //calculate crowding distance for last front, from which remaining individuals will be selected 
        CalcCrowdingDistance(fronts[currentFrontIndex]);
        //sort last front by crowdingdistance (descending)
        fronts[currentFrontIndex] = fronts[currentFrontIndex].OrderByDescending(x => x.GetComponent<Machine>().CrowdingDistance).ToList();
        //number of remaining individuals to select
        int remainingIndvCount = populationSize - nextPopulation.Count;
        //add |N| - |nextPop| individuals from last front to next
        nextPopulation.AddRange(fronts[currentFrontIndex].GetRange(0, remainingIndvCount));
        
        return nextPopulation;
    }

    public static void CalcCrowdingDistance(List<GameObject> front)
    {
        for (int i = 0; i < NUM_OBJECTIVES; i++)
        {
            //sort front accroding to objective value in ascending order of magnitude
            front = front.OrderBy(x => x.GetComponent<Machine>().FitnessVals[i]).ToList();
            //normalize objective values
            MinMaxNormalizeObjectives(front, i);
            //set min and max to infinite val
            front[0].GetComponent<Machine>().CrowdingDistance = float.MaxValue;
            front[front.Count-1].GetComponent<Machine>().CrowdingDistance = float.MaxValue;
            //get maximum and minimum normalized value
            float minNormObj = front[0].GetComponent<Machine>().TranslatedObjectives[i];
            float maxNormObj = front[front.Count - 1].GetComponent<Machine>().TranslatedObjectives[i];

            for (int j = 1; j < front.Count; j++)
            {
                //calculate crowding distance for every but first and last individual in sorted front
                if(front[j].GetComponent<Machine>().CrowdingDistance < float.MaxValue)
                {
                    //get objective values of neighbours
                    float prevIndvObjVal = front[j - 1].GetComponent<Machine>().FitnessVals[i];
                    float nextIndvObjVal = front[j + 1].GetComponent<Machine>().FitnessVals[i];
                    //calculate objective and minmax difference
                    float objectiveDifference = nextIndvObjVal - prevIndvObjVal;
                    float minMaxDifference = maxNormObj - minNormObj;

                    front[j].GetComponent<Machine>().CrowdingDistance += (objectiveDifference / minMaxDifference);
                }
            }

        }
    }

    //minmax normalize all objective values for objective
    public static void MinMaxNormalizeObjectives(List<GameObject> front, int objective)
    {
        float min = front[0].GetComponent<Machine>().FitnessVals[objective];
        float max = front[0].GetComponent<Machine>().FitnessVals[objective];

        foreach (GameObject machine in front)
        {
            if (objective == 0)
            {
                machine.GetComponent<Machine>().TranslatedObjectives = new List<float>();
            }
            machine.GetComponent<Machine>().TranslatedObjectives.Add(MinMaxNormalize(machine.GetComponent<Machine>().FitnessVals[objective], min, max, 0, 1));
        }
    }

    //min max normalization
    public static float MinMaxNormalize(float value, float min, float max, float normMin, float normMax)
    {
        return (((value - min) / (max - min)) * (normMax - normMin)) + normMin;
    }
}
