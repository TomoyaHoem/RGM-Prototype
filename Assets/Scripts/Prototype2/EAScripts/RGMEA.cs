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
    List<GameObject> population, bestInfParents, bestFParents, feasiblePop, infeasiblePop;
    public int Generation { get; private set; }

    //script references
    MachineRater mR;
    MachineSelector mS;
    MachineBreeder mB;
    MachineMutator mT;
    MachineSpawner machineSp;

    //NSGA references
    List<ReferencePoint> refPoints;
    List<List<GameObject>> fronts;
    //count of generated machines
    private int count = 0;
    //Current Task
    Task cur;

    public IEnumerator EvolveMachines()
    {
        Physics2D.autoSimulation = false;

        //Initialize Population
        InitializePopulation();

        cur = new Task(WaitForMachineGeneration());
        while (cur.Running) yield return null;

        //EA-Setup -> Population, Scripts
        EASetup();

        refPoints = ReferencePointCalculator.CalculateReferencePoints(20, SettingsReader.Instance.EASettings.FitFunc.Count);

        Debug.Log("Press Space to start evolution.");

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        int populationSize = SettingsReader.Instance.EASettings.PopulationSize;

        //Loop
        for (int i = 0; i < SettingsReader.Instance.EASettings.Iterations; i++)
        {
            //Step 1: Rate population
            cur = new Task(mR.RateMachines(population));
            while (cur.Running) yield return null;

            Debug.Log("tested machines");

            //Step 2: Split population by feasibility
            int feasIndex = SettingsReader.Instance.EASettings.FitFunc.Count - 1;
            //clear and refilter
            infeasiblePop.Clear(); feasiblePop.Clear();

            foreach (GameObject indv in population)
            {
                if (indv.GetComponent<Machine>().FitnessVals[feasIndex] < 1)
                {
                    infeasiblePop.Add(indv);
                }
                else
                {
                    feasiblePop.Add(indv);
                }
            }

            Debug.Log("split machines");

            //Step 2.1: Environment seleciton

            //Trim infeasible population
            //sort infeasible
            infeasiblePop = infeasiblePop.OrderByDescending(x => x.GetComponent<Machine>().FitnessVals[feasIndex]).ToList();
            if (infeasiblePop.Count > populationSize / 2)
            {
                //delete overflow
                int delAmount = infeasiblePop.Count - populationSize / 2;
                for (int j = 0; j < delAmount; j++)
                {
                    GameObject del = infeasiblePop[infeasiblePop.Count - 1];
                    infeasiblePop.RemoveAt(infeasiblePop.Count - 1);
                    population.Remove(del);
                    Destroy(del);
                }
            }

            //NSGA on feasible population
            if (feasiblePop.Count > 4)
            {
                fronts = FastNonDominatedSort.CalculateFronts(feasiblePop);
                int feasCount = feasiblePop.Count > populationSize / 2 ? 15 : feasiblePop.Count;
                feasiblePop = NSGA3.NSGAIII(fronts, new List<ReferencePoint>(refPoints), feasCount);
            }

            //update population
            population = infeasiblePop.Concat(feasiblePop).ToList();

            Debug.Log(infeasiblePop.Count + " , trimmed , " + feasiblePop.Count);

            //rearrange population
            machineSp.RearrangeMachines(infeasiblePop, 2);
            machineSp.RearrangeMachines(feasiblePop, 1);

            Debug.Log(infeasiblePop.Count + " , rearrange , " + feasiblePop.Count);

            //Step 3a: Selection infeasible -> tournament
            cur = new Task(mS.SelectMachines(infeasiblePop, bestInfParents, false));
            while (cur.Running) yield return null;

            //Step 3b: Selection feasible -> random
            cur = new Task(mS.SelectMachines(feasiblePop, bestFParents, true));
            while (cur.Running) yield return null;

            Debug.Log(bestInfParents.Count + " , parents , " + bestFParents.Count);

            //Step 4: Crossover & Breeding
            //infeasible
            cur = new Task(mB.BreedMachines(bestInfParents, false, population));
            while (cur.Running) yield return null;
            //feasible
            cur = new Task(mB.BreedMachines(bestFParents, true, population));
            while (cur.Running) yield return null;

            //Step 5: Mutate all children
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
        machineSp = populationHolder.AddComponent<MachineSpawner>();
        machineSp.GenerateMachineGrid();
    }

    private IEnumerator WaitForMachineGeneration()
    {
        foreach (Transform child in populationHolder.transform)
        {
            child.GetComponent<MachineGenerator>().machineCompleteEvent += OnMachineGenerated;
        }
        while (count < SettingsReader.Instance.EASettings.PopulationSize)
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
        foreach (Transform child in populationHolder.transform)
        {
            population.Add(child.gameObject);
        }
        bestInfParents = new List<GameObject>();
        bestFParents = new List<GameObject>();
        feasiblePop = new List<GameObject>();
        infeasiblePop = new List<GameObject>();
        mR = gameObject.AddComponent<MachineRater>();
        mS = gameObject.AddComponent<MachineSelector>();
        mB = gameObject.AddComponent<MachineBreeder>();
        mB.machineSp = machineSp;
        mT = gameObject.AddComponent<MachineMutator>();
        gameObject.AddComponent<MachineTestManager>();
    }
}