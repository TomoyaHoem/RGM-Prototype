using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RGMEA : MonoBehaviour
{
    //Evolutionary Algorithm

    //population and generation number
    GameObject populationHolder;
    [SerializeField]
    List<GameObject> population, bestParents, emptyMachines;
    public int Generation { get; private set; }

    //script references
    MachineRater mR;
    MachineSelector mS;
    MachineBreeder mB;
    MachineMutator mT;


    //count of generated machines
    private int count = 0;
    //Current Task
    Task cur;

    public IEnumerator EvolveMachines()
    {
        //Initialize Population
        InitializePopulation();

        cur = new Task(WaitForMachineGeneration());
        while (cur.Running) yield return null;

        //EA-Setup -> Population, Scripts
        EASetup();

        Physics2D.autoSimulation = false;

        //Debug.Log("Press Space to start evolution.");

        //while (!Input.GetKeyDown(KeyCode.Space))
        //{
        //    yield return null;
        //}

        //Loop
        for (int i = 0; i < SettingsReader.Instance.EASettings.Iterations; i++)
        {
            //Rate and Sort Population
            cur = new Task(mR.RateMachines(population));
            while (cur.Running) yield return null;
            //sort population
            population = population.OrderByDescending(x => x.GetComponent<Machine>().Fitness).ToList();

            //Selection
            cur = new Task(mS.SelectMachines(population, bestParents, emptyMachines));
            while (cur.Running) yield return null;

            //Crossover & Breeding
            cur = new Task(mB.BreedMachines(bestParents, emptyMachines));
            while (cur.Running) yield return null;

            //Mutation
            cur = new Task(mT.MutateMachines(population));
            while (cur.Running) yield return null;

            Debug.Log("iteration: " + i);
            //while (!Input.GetKeyDown(KeyCode.Space))
            //{
            //    yield return null;
            //}
        }

        cur = new Task(mR.RateMachines(population));
        while (cur.Running) yield return null;

        Physics2D.autoSimulation = true;
    }

    private void InitializePopulation()
    {
        //Parent GameObject of all Individuals
        populationHolder = new GameObject("Population");
        //Generate Individuals (Machines)
        populationHolder.AddComponent<MachineSpawner>().GenerateMachineGrid();
    }

    private IEnumerator WaitForMachineGeneration()
    {
        foreach(Transform child in populationHolder.transform)
        {
            child.GetComponent<MachineGenerator>().machineCompleteEvent += OnMachineGenerated;
        }
        while(count < SettingsReader.Instance.EASettings.PopulationSize)
        {
            yield return null;
        }
        foreach (Transform child in populationHolder.transform)
        {
            child.GetComponent<MachineGenerator>().machineCompleteEvent -= OnMachineGenerated;
        }
        count = 0;
    }

    private void OnMachineGenerated()
    {
        count++;
    }

    private void EASetup()
    {
        population = new List<GameObject>();
        foreach(Transform child in populationHolder.transform)
        {
            population.Add(child.gameObject);
        }
        bestParents = new List<GameObject>();
        emptyMachines = new List<GameObject>();
        mR = gameObject.AddComponent<MachineRater>();
        mS = gameObject.AddComponent<MachineSelector>();
        mB = gameObject.AddComponent<MachineBreeder>();
        mT = gameObject.AddComponent<MachineMutator>();
        gameObject.AddComponent<MachineTestManager>();
    }
}