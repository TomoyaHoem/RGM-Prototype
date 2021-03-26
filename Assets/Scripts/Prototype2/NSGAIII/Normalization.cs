using System.Collections.Generic;
using UnityEngine;

public static class Normalization
{
    static int NUM_OBJECTIVES = 3;//SettingsReader.Instance.EASettings.FitFunc.Count;

    //find indeal point and simultaneously translate objectives
    public static List<float> ComputeIdealPoint(List<List<GameObject>> fronts)
    {
        List<float> idealPoint = new List<float>();

        for (int f = 0; f < NUM_OBJECTIVES; f++)
        {
            float minF = float.MaxValue;
            for (int i = 0; i < fronts[0].Count; i++) //min values must be in first front
            {
                //foreach objective find minimum value, from each candidate in first front -> minimum fitness for each function of first front machines
                minF = Mathf.Min(minF, fronts[0][i].GetComponent<Machine>().FitnessVals[f]);
            }
            idealPoint.Add(minF);

            //translate objectives and store as attribute of individual
            foreach (List<GameObject> front in fronts)
            {
                foreach (GameObject machine in front)
                {
                    //initialize list during first objective loop
                    if (f == 0) machine.GetComponent<Machine>().TranslatedObjectives = new List<float>();

                    machine.GetComponent<Machine>().TranslatedObjectives.Add(machine.GetComponent<Machine>().FitnessVals[f] - minF);
                }
            }
        }

        return idealPoint;
    }

    //Achievement Scalarizing Function
    //index of objective uses 1.0, rest use 0.00001
    private static float ASF(GameObject machine, int index)
    {
        float maxRatio = float.NegativeInfinity;
        for (int i = 0; i < NUM_OBJECTIVES; i++)
        {
            float weight = (index == i) ? 1.0f : 0.00001f;
            maxRatio = Mathf.Max(maxRatio, machine.GetComponent<Machine>().FitnessVals[i] / weight);
        }
        return maxRatio;
    }

    //find extreme points (max vals on axis) -> minimize ASF
    public static List<GameObject> FindExtremePoints(List<List<GameObject>> fronts)
    {
        List<GameObject> extremePoints = new List<GameObject>();
        GameObject minIndv = null;

        for (int f = 0; f < NUM_OBJECTIVES; f++)
        {
            float minASF = float.MaxValue;
            foreach (GameObject machine in fronts[0])
            {
                float asf = ASF(machine, f);
                if (asf < minASF)
                {
                    minASF = asf;
                    minIndv = machine;
                }
            }
            extremePoints.Add(minIndv);
        }

        return extremePoints;
    }

    //construct hyperplane from extreme points to find axis intercepts
    public static List<float> ConstructHyperplane(List<GameObject> extremePoints)
    {
        //check for duplicate extreme points
        bool duplicate = false;

        for (int i = 0; !duplicate && i < extremePoints.Count; i++)
        {
            for (int j = i + 1; !duplicate && j < extremePoints.Count; j++)
            {
                duplicate = extremePoints[i] == extremePoints[j];
            }
        }

        Debug.Log(duplicate);

        List<float> intercepts = new List<float>();

        if (duplicate)
        {
            for (int f = 0; f < NUM_OBJECTIVES; f++)
            {
                //add individual with largest value of objective f
                intercepts.Add(extremePoints[f].GetComponent<Machine>().FitnessVals[f]);
            }
        }
        else
        {
            //find hyperplane equation
            List<float> b = new List<float>();
            for (int i = 0; i < NUM_OBJECTIVES; i++)
            {
                b.Add(1.0f);
            }
            List<List<float>> A = new List<List<float>>();
            foreach (GameObject machine in extremePoints)
            {
                List<float> aux = new List<float>();
                for (int i = 0; i < NUM_OBJECTIVES; i++)
                {
                    aux.Add(machine.GetComponent<Machine>().FitnessVals[i]);
                }
                A.Add(aux);
            }
            List<float> x = GaussianElimination(A, b);

            //find intercepts
            for (int f = 0; f < NUM_OBJECTIVES; f++)
            {
                intercepts.Add(1.0f / x[f]);
            }
        }

        return intercepts;
    }

    //gaussian elimination with partial pivoting
    public static List<float> GaussianElimination(List<List<float>> A, List<float> b)
    {
        int N = A.Count;
        for (int k = 0; k < N; k++)
        {
            //find pivot row -> max value in ith row at column k
            int max = k;
            for (int i = k + 1; i < N; i++)
            {
                if (Mathf.Abs(A[i][k]) > Mathf.Abs(A[max][k]))
                {
                    max = i;
                }
            }

            //swap rows in A
            List<float> temp = A[k];
            A[k] = A[max];
            A[max] = temp;

            //swap corresponding values in b
            float t = b[k];
            b[k] = b[max];
            b[max] = t;


            //pivot with A and b
            for (int i = k + 1; i < N; i++)
            {
                float factor = A[i][k] / A[k][k];
                b[i] -= factor * b[k];
                for (int j = 0; j < N; j++)
                {
                    A[i][j] -= factor * A[k][j];
                }
            }
        }
        List<float> x = new List<float>(new float[b.Count]);
        //back substitution
        for (int i = N - 1; i >= 0; i--)
        {
            float sum = 0;
            for (int j = i + 1; j < N; j++)
            {
                sum += A[i][j] * x[j];
            }
            x[i] = (b[i] - sum) / A[i][i];
        }

        return x;
    }

    //normalize objectives
    //no normalization can lead to non-uniformly distributed front because of differently scaled objectives
    public static void NormalizeObjectives(List<List<GameObject>> fronts, List<float> intercepts, List<float> idealPoint)
    {
        for (int t = 0; t < fronts.Count; t++)
        {
            foreach (GameObject machine in fronts[t])
            {
                for (int f = 0; f < NUM_OBJECTIVES; f++)
                {
                    List<float> transObj = machine.GetComponent<Machine>().TranslatedObjectives;
                    if (Mathf.Abs(intercepts[f] - idealPoint[f]) > 10e-10)
                    {
                        transObj[f] = transObj[f] / (intercepts[f] - idealPoint[f]);
                    }
                    else
                    {
                        transObj[f] = transObj[f] / (float)10e-10;
                    }
                }
            }
        }
    }
}
