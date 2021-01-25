using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineSpawner : MonoBehaviour
{
    //spawns Machine GameObjects in a gridlike pattern

    //10 units as base distance between machines
    private float maxArea = 50;

    public void GenerateMachineGrid()
    {
        int populationSize = SettingsReader.Instance.EASettings.PopulationSize;

        int count = 0;

        //calculate machine spacing
        maxArea += SettingsReader.Instance.MachineSettings.MachineArea;

        int machineGridSize = (int)Mathf.Ceil(Mathf.Sqrt(populationSize));

        //fill up grid in x direction first
        for (int y = 0; y < machineGridSize; y++)
        {
            for (int x = 0; x < machineGridSize; x++)
            {
                //only fill grid until populationsize is reached
                if (count < populationSize)
                {
                    SpawnNewMachineObject(new Vector2(maxArea * x, maxArea * y), count);
                }
                count++;
            }
        }
    }

    private void SpawnNewMachineObject(Vector2 machineOriginPosition, int count)
    {
        GameObject machine = new GameObject("Machine " + count);
        machine.transform.position = machineOriginPosition;
        //parent object for clean hierarchy
        machine.transform.parent = gameObject.transform;

        //Generate Machine
        machine.AddComponent<MachineGenerator>().GenerateNewMachine();
    }
}
