using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MachineRater : MonoBehaviour
{

    Dictionary<string, float> fitFuncs = SettingsReader.Instance.EASettings.FitFunc;

    public IEnumerator RateMachines(List<GameObject> population)
    {
        int evolutionMethod = SettingsReader.Instance.EASettings.EvolutionMethod;

        if(evolutionMethod == 3)
        {
            //manual selection -> no rating needed
            yield break;
        }

        if(fitFuncs.ContainsKey("feas"))
        {
            Task testMachines = new Task(gameObject.GetComponent<MachineTestManager>().TestMachinePopulation(population));
            while (testMachines.Running) yield return null;
        }

        foreach(GameObject machine in population)
        {
            //only calculate fitness for new Machines
            if (machine.GetComponent<Machine>().Fitness == 0)
            {
                CalculateFitness(machine.GetComponent<Machine>());
                machine.GetComponent<Machine>().SegmentPieces.Clear();
            }
        }
    }

    private void CalculateFitness(Machine machine)
    {
        List<float> fit = new List<float>();
        float freq = 0, lin = 0, comp = 0, feas = 0;

        //add different weighted fitness metrics
        if (fitFuncs.ContainsKey("freq")) {freq = SegmentFrequency(machine) * fitFuncs["freq"]; fit.Add(freq); }
        if (fitFuncs.ContainsKey("lin")) { lin = SegmentLinearity(machine) * fitFuncs["lin"]; fit.Add(lin); }
        if (fitFuncs.ContainsKey("comp")) { comp = MachineCompactness(machine) * fitFuncs["comp"]; fit.Add(comp); }
        if (fitFuncs.ContainsKey("feas")) { feas = MachineFeasibility(machine) * fitFuncs["feas"]; fit.Add(feas); }

        //Debug.Log(machine.gameObject.name + " Fitness -> " + "Freq: " + freq + " Lin: " + lin + " Comp: " + comp + " Feas: " + feas);

        machine.Fitness = freq + lin + comp + feas;

        machine.Canvas.transform.GetChild(0).GetComponent<BarChart>().UpdateBars(fit);
    }

    private float SegmentFrequency(Machine machine)
    {
        float result = 0f;
        float num = machine.Segments.Count;

        float bCount = 0, dCount = 0, mCount = 0;

        //count segments
        foreach (GameObject seg in machine.Segments)
        {
            if (seg.GetComponent<SegmentPart>().SegmentID == 0) dCount++;
            if (seg.GetComponent<SegmentPart>().SegmentID == 1) bCount++;
            if (seg.GetComponent<SegmentPart>().SegmentID == 2 || seg.GetComponent<SegmentPart>().SegmentID == 3) mCount++;
        }

        //simpson index D = 1 - [Sum n*(n-1)]/[Count*(Count-1)] does not account 0 values
        //-> Shannon Equitability Index

        //calculate proportions
        dCount /= num; bCount /= num; mCount /= num;

        float shanDiv = dCount * RGMTest.LN(dCount) + bCount * RGMTest.LN(bCount) + mCount * RGMTest.LN(mCount);

        result = Mathf.Abs(shanDiv /Mathf.Log(3));

        return result;
    }

    private float SegmentLinearity(Machine machine)
    {
        float result = 0f;
        float maxCount = 0;
        float currCount = 1;
        float num = machine.Segments.Count;
        GameObject prev = machine.Segments[0];

        //count most consecutive segments
        for (int i = 1; i < machine.Segments.Count; i++)
        {
            if (machine.Segments[i].GetComponent<SegmentPart>().SegmentID == prev.GetComponent<SegmentPart>().SegmentID)
            {
                currCount++;
            }
            else
            {
                currCount = 1;
            }
            if (currCount > maxCount)
            {
                maxCount = currCount;
            }
            prev = machine.Segments[i];
        }

        //total #Segments - #maxConsecutiveSeg divided by #Seg-1
        //-> best case alternating segments = 1, worst case only same segments = 0
        result = (num - maxCount) / (num - 1);

        return result;
    }

    private float MachineCompactness(Machine machine)
    {
        float result = 0f;
        float num = machine.Segments.Count;

        Vector2 segMid;

        foreach (GameObject seg in machine.Segments)
        {
            //calculate segment middle
            segMid = seg.GetComponent<SegmentPart>().Input + (seg.GetComponent<SegmentPart>().Output - seg.GetComponent<SegmentPart>().Input) * 0.5f;

            //add gaussian value
            result += GaussianKernel(machine.transform.position, segMid, num);
        }

        //normalize
        result /= num;

        return result;
    }

    float GaussianKernel(Vector2 origin, Vector2 position, float num)
    {
        float res = 0f;
        //sigma -> #segments
        float sigma = num;

        res = Mathf.Pow(Vector2.Distance(origin, position),2);

        res /= 2 * Mathf.Pow(sigma, 2);

        res = Mathf.Exp(-res);

        return res;
    }

    private float MachineFeasibility(Machine machine)
    {
        List<GameObject> testSequence = machine.GetComponent<MachineTester>().TestSequence;
        List<GameObject> origSequence = machine.GetComponent<Machine>().SegmentPieces;
        //Debug.Log("Test: " + testSequence.Count + " - Original: " + origSequence.Count);
        //compare lists & calculate feasibility
        float res = LevenshteinDistance(origSequence, testSequence);
        //normalize
        res = origSequence.Count > testSequence.Count ? res / origSequence.Count : res / testSequence.Count;
        res = 1 - res;

        return res;
    }

    //https://www.csharpstar.com/csharp-string-distance-algorithm/#:~:text=The%20Levenshtein%20distance%20is%20a,is%20named%20after%20Vladimir%20Levenshtein.
    //https://de.wikipedia.org/wiki/Levenshtein-Distanz
    private float LevenshteinDistance(List<GameObject> orignial, List<GameObject> test)
    {
        int n = orignial.Count;
        int m = test.Count;
        int[,] d = new int[n + 1, m + 1];

        // Step 1 if either is empty distance is length of other
        if (n == 0) return m;
        if (m == 0) return n;
        
        // Step 2 fill first column and row with 0,1,2,3,...,length
        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        // Step 3 loop row
        for (int i = 1; i <= n; i++)
        {
            //Step 4 loop column
            for (int j = 1; j <= m; j++)
            {
                // Step 5 editing cost of current cell is 0 if same else 1
                int cost = ReferenceEquals(test[j - 1], orignial[i - 1]) ? 0 : 1;

                // Step 6 min
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }
        // Step 7
        return d[n, m];
    }
}
