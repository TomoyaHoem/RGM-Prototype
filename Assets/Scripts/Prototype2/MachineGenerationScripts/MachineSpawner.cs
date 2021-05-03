using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineSpawner : MonoBehaviour
{
    //spawns Machine GameObjects in a gridlike pattern
    //also responsible for arranging feasible and infeasible population as well as children

    //N units as base distance between machines
    private float maxArea = 50;
    private int machineGridSize;

    public void GenerateMachineGrid()
    {
        int populationSize = SettingsReader.Instance.EASettings.PopulationSize;

        int count = 0;

        //calculate machine spacing
        maxArea += SettingsReader.Instance.MachineSettings.MachineArea;

        machineGridSize = (int)Mathf.Ceil(Mathf.Sqrt(populationSize));

        //fill up grid in x direction first
        for (int y = 0; y < machineGridSize; y++)
        {
            for (int x = 0; x < machineGridSize; x++)
            {
                //only fill grid until populationsize is reached
                if (count < populationSize)
                {
                    //start at 100,100
                    SpawnNewMachineObject(new Vector2(100 + maxArea * x, 100 + maxArea * y), count);
                }
                count++;
            }
        }
    }

    public void RearrangeMachines(List<GameObject> population, int quadrant)
    {
        int populationSize = population.Count;

        int count = 0;

        for (int y = 0; y < machineGridSize; y++)
        {
            for (int x = 0; x < machineGridSize; x++)
            {
                //only fill grid until populationsize is reached
                if (count < populationSize)
                {
                    //start at 100,100 in respective quadrant
                    //quadrant == 1 -> 100 + maxArea * x else == 2 -> -100 - maxArea * x
                    float xPos = quadrant == 1 ? 100 + maxArea * x : -100 - maxArea * x;
                    population[count].GetComponent<Machine>().MoveMachineTo(new Vector2(xPos, 100 + maxArea * y));
                }
                count++;
            }
        }
    }

    public GameObject SpawnNewEmptyChildMachine(int popSize, int index, int quadrant, bool mirr)
    {
        //spawn new machine object for child and append Y if mirrored and X if not
        GameObject machine = new GameObject("Machine ");
        machine.name += mirr ? "Y" : "X";
        //place in respective quadrant
        int x = index % machineGridSize;
        float xPos = quadrant == 3 ? -100 - maxArea * x : 100 + maxArea * x;
        int y = index / machineGridSize;
        
        machine.transform.position = new Vector2(xPos, -100 - maxArea * y);
        //parent object for clean hierarchy
        machine.transform.parent = gameObject.transform;

        //Generate empty Machine
        machine.AddComponent<MachineGenerator>().GenerateNewMachine(0);

        return machine;
    }

    private void SpawnNewMachineObject(Vector2 machineOriginPosition, int count)
    {
        GameObject machine = new GameObject("Machine " + count);
        machine.transform.position = machineOriginPosition;
        //parent object for clean hierarchy
        machine.transform.parent = gameObject.transform;

        //Generate Machine
        machine.AddComponent<MachineGenerator>().GenerateNewMachine(SettingsReader.Instance.MachineSettings.NumSegments);
    }
}
