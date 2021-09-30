using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RGMEA : MonoBehaviour
{
    //Evolutionary Algorithm

    //populations and generation number
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

    //main coroutine that simulates evolution
    public IEnumerator EvolveMachines()
    {
        settings = SettingsReader.Instance.MachineSettings;

        //reset settings (could contain data from previous run)
        settings.SuccessfullMachines = 0;

        settings.Coverage = 0;
        settings.CoverageCount = 0;

        settings.EAStatistics = new List<List<float>>();
        for (int i = 0; i < 43; i++)
        {
            settings.EAStatistics.Add(new List<float>());
        }

        //turn off autoSimulation, only turn on when simulating machines
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

        Debug.Log("Fully generated machines: " + SettingsReader.Instance.MachineSettings.SuccessfullMachines);

        Debug.Log("Press Space to start evolution.");

        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        int populationSize = SettingsReader.Instance.EASettings.PopulationSize;

        refPoints = ReferencePointCalculator.CalculateReferencePoints(7, SettingsReader.Instance.EASettings.FitFunc.Count - 1);

        //init UI
        UIStatistics.Instance.InitUIData();

        float timer = 0;

        if(SettingsReader.Instance.EASettings.NSGA == 2)
        {
            Debug.Log("STARTING EA WITH NSGA-II");
        } else
        {
            Debug.Log("STARTING EA WITH NSGA-III");
        }

        //Loop
        for (int i = 0; i < SettingsReader.Instance.EASettings.Iterations; i++)
        {
            //save iteration in statistics
            settings.EAStatistics[0].Add(i);

            timer = Time.realtimeSinceStartup;

            //UIStatistics.Instance.Iteration = i;

            //Step 1: Rate population
            cur = new Task(mR.RateMachines(population));
            while (cur.Running) yield return null;

            //update children statistics
            UIStatistics.Instance.FeasChildCount = feasibleChildren.Count;
            settings.EAStatistics[6].Add(feasibleChildren.Count);
            UIStatistics.Instance.InfeasChildCount = infeasibleChildren.Count;
            settings.EAStatistics[7].Add(infeasibleChildren.Count);
            //Step 2: Split population by feasibility
            int feasIndex = SettingsReader.Instance.EASettings.FitFunc.Count - 1;
            //clear and refilter, save references for migration stats
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

            //Step 2.1: Environment seleciton

            List<GameObject> destroy = new List<GameObject>();

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
                    destroy.Add(del);
                }
            }

            //NSGA on feasible population
            if (feasiblePop.Count > populationSize / 2)
            {
                //invert fitness for NSGA
                foreach (GameObject machine in feasiblePop)
                {
                    List<float> fit = machine.GetComponent<Machine>().FitnessVals;
                    for (int f = 0; f < fit.Count - 1; f++)
                    {
                        fit[f] = 1 - fit[f];
                    }
                }

                fronts = FastNonDominatedSort.CalculateFronts(feasiblePop);
                newFeasiblePop = SettingsReader.Instance.EASettings.NSGA == 2 ? NSGA2.NSGAII(fronts, populationSize/2) : NSGA3.NSGAIII(fronts, new List<ReferencePoint>(refPoints), populationSize / 2);
                int delC = 0;
                int shoulDel = feasiblePop.Count - newFeasiblePop.Count;

                for(int j = feasiblePop.Count-1; j > -1; j--)
                {
                    if (delC == shoulDel) break;
                    if (!newFeasiblePop.Contains(feasiblePop[j]))
                    {
                        delC++;
                        GameObject migrate = feasiblePop[j];
                        feasiblePop.Remove(feasiblePop[j]);
                        destroy.Add(migrate);
                    }
                }

                //revert fitness values
                foreach (GameObject machine in feasiblePop)
                {
                    List<float> fit = machine.GetComponent<Machine>().FitnessVals;
                    for (int f = 0; f < fit.Count - 1; f++)
                    {
                        fit[f] = 1 - fit[f];
                    }
                }
            }

            //destroy unwanted 
            foreach (GameObject machine in destroy)
            {
                Destroy(machine);
            }

            destroy.Clear();

            //update population
            population = infeasiblePop.Concat(feasiblePop).ToList();

            //segment distribution
            if (i == 0)
            {
                SegmentDistribution(feasiblePop, "feasible", "first");
                SegmentDistribution(infeasiblePop, "infeasible", "first");
            }
            if (i == SettingsReader.Instance.EASettings.Iterations - 1)
            {
                SegmentDistribution(feasiblePop, "feasible", "end");
                SegmentDistribution(infeasiblePop, "infeasible", "end");
            }
            if (i == ((SettingsReader.Instance.EASettings.Iterations - 1) / 2))
            {
                SegmentDistribution(feasiblePop, "feasible", "middle");
                SegmentDistribution(infeasiblePop, "infeasible", "middle");
            }

            settings.EAStatistics[2].Add(feasiblePop.Count);
            settings.EAStatistics[3].Add(infeasiblePop.Count);

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
                if (infeasiblePop.Contains(g))
                {
                    transfer++;
                }
            }
            UIStatistics.Instance.FeasStay = stay;
            settings.EAStatistics[8].Add(stay);
            UIStatistics.Instance.FeasTransfer = transfer;
            settings.EAStatistics[10].Add(transfer);
            settings.EAStatistics[12].Add(feasibleChildren.Count - (stay + transfer));

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
            settings.EAStatistics[9].Add(stay);
            UIStatistics.Instance.InfeasTransfer = transfer;
            settings.EAStatistics[11].Add(transfer);
            settings.EAStatistics[13].Add(infeasibleChildren.Count - (stay + transfer));

            //update population statistics
            UIStatistics.Instance.PopulationSize = population.Count;
            UIStatistics.Instance.FeasSize = feasiblePop.Count;
            UIStatistics.Instance.InfeasSize = infeasiblePop.Count;

            //Debug.Log(infeasiblePop.Count + " , trimmed , " + feasiblePop.Count);

            //rearrange population
            machineSp.RearrangeMachines(infeasiblePop, 2);
            machineSp.RearrangeMachines(feasiblePop, 1);

            //Debug.Log(infeasiblePop.Count + " , rearrange , " + feasiblePop.Count);

            TrackFitnessAndLength(feasiblePop, true);
            TrackFitnessAndLength(infeasiblePop, false);

            //Step 3a: Selection infeasible -> tournament
            cur = new Task(mS.SelectMachines(infeasiblePop, bestInfParents, false, false));
            while (cur.Running) yield return null;

            //Step 3b: Selection feasible -> random or tournament + crowding
            cur = new Task(mS.SelectMachines(feasiblePop, bestFParents, true, (i > 0)));
            while (cur.Running) yield return null;

            //Debug.Log(bestInfParents.Count + " , parents , " + bestFParents.Count);

            //Step 4: Crossover & Breeding
            //infeasible
            cur = new Task(mB.BreedMachines(bestInfParents, false, population, infeasibleChildren));
            while (cur.Running) yield return null;
            //feasible
            cur = new Task(mB.BreedMachines(bestFParents, true, population, feasibleChildren));
            while (cur.Running) yield return null;

            //Step 5: Mutate only children
            //infeasible
            cur = new Task(mT.MutateMachines(infeasibleChildren, false));
            while (cur.Running) yield return null;
            //feasible
            cur = new Task(mT.MutateMachines(feasibleChildren, true));
            while (cur.Running) yield return null;

            Debug.Log("iteration: " + i);

            //update UI
            UIStatistics.Instance.UpdateUI();

            //while (!Input.GetKeyDown(KeyCode.Space))
            //{
            //    yield return null;
            //}

            //save time in statistics
            timer = Time.realtimeSinceStartup - timer;
            settings.EAStatistics[1].Add(timer);
        }

        cur = new Task(mR.RateMachines(population));
        while (cur.Running) yield return null;

        /*

        //write statistics to csv
        CSVWriter.WriteEAResultsToCSV(settings.EAStatistics);
        if(fronts != null)
        {
            if (fronts.Count > 0) CSVWriter.WriteParetoDatatoCSV(fronts[0]);
        }

        */

        //highlight pareto front
        /*!!
        foreach (GameObject machine in fronts[0])
        {
            if (newFeasiblePop.Contains(machine))
            {
                machine.GetComponent<Machine>().SwitchSelect();
            }
        }
        */

        /*
        cur = new Task(mT.MutateMachines(population, false));
        while (cur.Running) yield return null;
        */

        //add end object
        foreach (GameObject machine in population)
        {
            SegmentPart last = machine.GetComponent<Machine>().Segments[machine.GetComponent<Machine>().Segments.Count - 1].GetComponent<SegmentPart>();
            Vector2 endPos = last.Output + last.OutputDirection * 0.5f;
            machine.GetComponent<Machine>().End = Instantiate(Resources.Load("Prefabs/End"), endPos, Quaternion.identity, machine.transform) as GameObject;
            if (!fronts[0].Contains(machine)){
                machine.SetActive(false);
            }
        }

        //Debug.Log("Average Coverage: " + (SettingsReader.Instance.MachineSettings.Coverage / SettingsReader.Instance.MachineSettings.CoverageCount) + " for " + SettingsReader.Instance.MachineSettings.CoverageCount + " machines.");

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

    private void SegmentDistribution(List<GameObject> population, string feas, string first)
    {
        //domino, ramp, mill, hammer, track
        int[] distribution = { 0, 0, 0, 0, 0 };

        foreach (GameObject g in population)
        {
            foreach (GameObject s in g.GetComponent<Machine>().Segments)
            {
                //if (g.GetComponent<Machine>().Segments.Count != SettingsReader.Instance.MachineSettings.NumSegments) continue;
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
        string at = feas + " " + first;
        Debug.Log("In: " + at + " Of " + numSegments + " segments -> Domino: " + distribution[0] + ", Ball: " + distribution[1] + ", Mill: " + distribution[2] + ", Hammer: " + distribution[3] + ", Car: " + distribution[4]);
    }

    private void TrackFitnessAndLength(List<GameObject> pop, bool feas)
    {
        int numFit = pop[0].GetComponent<Machine>().FitnessVals.Count;
        //if feasible population do not record feasibility as always 1
        if (feas) numFit--;

        //0 freq, 1 lin, 2 comp, 3 cov, 4 feas
        List<float> fitSums = new List<float>();
        for (int i = 0; i < numFit; i++)
        {
            fitSums.Add(0);
        }

        float lengthSum = 0;

        if (feas)
        {
            foreach (GameObject machine in pop)
            {
                for (int i = 0; i < numFit; i++)
                {
                    fitSums[i] += machine.GetComponent<Machine>().FitnessVals[i];
                }
                lengthSum += machine.GetComponent<Machine>().Segments.Count;
            }

            for (int i = 0; i < numFit; i++)
            {
                settings.EAStatistics[29 + i].Add(fitSums[i] / pop.Count);
            }
            settings.EAStatistics[14].Add(lengthSum / pop.Count);
        }
        else
        {
            foreach (GameObject machine in pop)
            {
                for (int i = 0; i < numFit; i++)
                {
                    fitSums[i] += machine.GetComponent<Machine>().FitnessVals[i];
                }
                lengthSum += machine.GetComponent<Machine>().Segments.Count;
            }

            for (int i = 0; i < numFit; i++)
            {
                settings.EAStatistics[24 + i].Add(fitSums[i] / pop.Count);
            }
            settings.EAStatistics[15].Add(lengthSum / pop.Count);
        }
    }
}