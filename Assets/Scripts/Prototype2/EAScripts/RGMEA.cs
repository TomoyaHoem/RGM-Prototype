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
    List<GameObject> population, bestInfParents, bestFParents, feasiblePop, infeasiblePop, feasibleChildren, infeasibleChildren, newFeasiblePop;
    public int Generation { get; private set; }

    //script references
    MachineRater mR;
    MachineSelector mS;
    MachineBreeder mB;
    MachineMutator mT;
    MachineSpawner machineSp;
    MachineSettings settings;

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
        while (cur.Running)
        {
            yield return null;
        }

        //EA-Setup -> Population, Scripts
        EASetup();

        SegmentDistribution(population);

        Debug.Log("Press Space to start evolution.");

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        int populationSize = SettingsReader.Instance.EASettings.PopulationSize;

        refPoints = ReferencePointCalculator.CalculateReferencePoints(populationSize, SettingsReader.Instance.EASettings.FitFunc.Count - 1);

        //init UI
        UIStatistics.Instance.InitUIData();

        //Loop
        for (int i = 0; i < SettingsReader.Instance.EASettings.Iterations; i++)
        {
            UIStatistics.Instance.Iteration = i;

            //Step 1: Rate population
            cur = new Task(mR.RateMachines(population));
            while (cur.Running) yield return null;

            //Debug.Log("tested machines");

            //update children statistics
            UIStatistics.Instance.FeasChildCount = feasibleChildren.Count;
            UIStatistics.Instance.InfeasChildCount = infeasibleChildren.Count;

            //Step 2: Split population by feasibility
            int feasIndex = SettingsReader.Instance.EASettings.FitFunc.Count - 1;
            //clear and refilter
            infeasiblePop.Clear(); feasiblePop.Clear();

            //split
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

            //Debug.Log("split machines");

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
                    Destroy(del);
                }
            }

            //NSGA on feasible population
            if (feasiblePop.Count > populationSize / 2)
            {
                fronts = FastNonDominatedSort.CalculateFronts(feasiblePop);
                newFeasiblePop = NSGA3.NSGAIII(fronts, new List<ReferencePoint>(refPoints), populationSize / 2);
                for (int j = feasiblePop.Count - 1; j > -1; j--)
                {
                    if (!newFeasiblePop.Contains(feasiblePop[j]))
                    {
                        GameObject del = feasiblePop[j];
                        feasiblePop.RemoveAt(j);
                        Destroy(del);
                    }
                }
            }

            //update population
            population = infeasiblePop.Concat(feasiblePop).ToList();

            //calc UI stats
            CalculateFitnessStatistics(feasIndex);

            //count stay and transfer of children
            int stay = 0;
            int transfer = 0;
            foreach (GameObject g in feasibleChildren)
            {
                if (feasiblePop.Contains(g))
                {
                    stay++;
                }
                if (infeasibleChildren.Contains(g))
                {
                    transfer++;
                }
            }
            UIStatistics.Instance.FeasStay = stay;
            UIStatistics.Instance.FeasTransfer = transfer;
            stay = 0;
            transfer = 0;
            foreach (GameObject g in infeasibleChildren)
            {
                if (feasiblePop.Contains(g))
                {
                    transfer++;
                }
                if (infeasiblePop.Contains(g))
                {
                    stay++;
                }
            }
            UIStatistics.Instance.InfeasStay = stay;
            UIStatistics.Instance.InfeasTransfer = transfer;

            //update population statistics
            UIStatistics.Instance.PopulationSize = population.Count;
            UIStatistics.Instance.FeasSize = feasiblePop.Count;
            UIStatistics.Instance.InfeasSize = infeasiblePop.Count;

            //Debug.Log(infeasiblePop.Count + " , trimmed , " + feasiblePop.Count);

            //rearrange population
            machineSp.RearrangeMachines(infeasiblePop, 2);
            machineSp.RearrangeMachines(feasiblePop, 1);

            //Debug.Log(infeasiblePop.Count + " , rearrange , " + feasiblePop.Count);

            //Step 3a: Selection infeasible -> tournament
            cur = new Task(mS.SelectMachines(infeasiblePop, bestInfParents, false));
            while (cur.Running) yield return null;

            //Step 3b: Selection feasible -> random
            cur = new Task(mS.SelectMachines(feasiblePop, bestFParents, true));
            while (cur.Running) yield return null;

            //Debug.Log(bestInfParents.Count + " , parents , " + bestFParents.Count);

            //Step 4: Crossover & Breeding
            //infeasible
            cur = new Task(mB.BreedMachines(bestInfParents, false, population, infeasibleChildren));
            while (cur.Running) yield return null;
            //feasible
            cur = new Task(mB.BreedMachines(bestFParents, true, population, feasibleChildren));
            while (cur.Running) yield return null;

            //Step 5: Mutate all children
            cur = new Task(mT.MutateMachines(population));
            while (cur.Running) yield return null;

            Debug.Log("iteration: " + i);

            //update UI
            UIStatistics.Instance.UpdateUI();

            //while (!Input.GetKeyDown(KeyCode.Space))
            //{
            //    yield return null;
            //}
        }

        Physics2D.autoSimulation = true;
    }

    private void CalculateFitnessStatistics(int feasIndex)
    {
        //calculate average feasibility
        float feasSum = 0;
        foreach (GameObject g in population)
        {
            feasSum += g.GetComponent<Machine>().FitnessVals[feasIndex];
        }
        feasSum /= population.Count;

        //Debug.Log("average feas: " + feasSum);
        UIStatistics.Instance.AverageFeasibility = feasSum;
        //calculate average objectives
        float[] objectiveSums = new float[SettingsReader.Instance.EASettings.FitFunc.Count - 1];

        foreach (GameObject g in population)
        {
            for (int k = 0; k < objectiveSums.Length; k++)
            {
                objectiveSums[k] += g.GetComponent<Machine>().FitnessVals[k];
            }
        }
        for (int l = 0; l < objectiveSums.Length; l++)
        {
            objectiveSums[l] /= population.Count;
        }

        UIStatistics.Instance.AverageObjectives = objectiveSums;
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
        feasibleChildren = new List<GameObject>();
        infeasibleChildren = new List<GameObject>();
        mR = gameObject.AddComponent<MachineRater>();
        mS = gameObject.AddComponent<MachineSelector>();
        mB = gameObject.AddComponent<MachineBreeder>();
        mB.machineSp = machineSp;
        mT = gameObject.AddComponent<MachineMutator>();
        gameObject.AddComponent<MachineTestManager>();
    }

    private void SegmentDistribution(List<GameObject> population)
    {
        //domino, ramp, mill, hammer, track
        int[] distribution = { 0, 0, 0, 0, 0 };

        foreach (GameObject g in population)
        {
            foreach (GameObject s in g.GetComponent<Machine>().Segments)
            {
                int id = s.GetComponent<SegmentPart>().SegmentID;
                if (id == 2 || id == 3)
                {
                    distribution[2]++;
                }
                else if (id > 3)
                {
                    distribution[id - 1]++;
                }
                else
                {
                    distribution[id]++;
                }
            }
        }

        int numSegments = distribution[0] + distribution[1] + distribution[2] + distribution[3] + distribution[4];

        Debug.Log("Of " + numSegments + " segments -> Domino: " + distribution[0] + ", Ball: " + distribution[1] + ", Mill: " + distribution[2] + ", Hammer: " + distribution[3] + ", Car: " + distribution[4]);
    }
}