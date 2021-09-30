using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EASettings")]
public class EASettings : ScriptableObject
{
    [Header("EA Settings")]
    [SerializeField] private int populationSize = 16;
    public int PopulationSize { get { return populationSize; } }

    [SerializeField] [Range(0, 1)]private float mutationRate = 0;
    public float MutationRate { get { return mutationRate; } }

    [SerializeField] [Range(0, 1)] private float parentSize = 0.5f;
    public float ParentSize { get { return parentSize; } }

    [SerializeField] [Range(0, 1)] private float crossoverRate = 0;
    public float CrossoverRate { get { return crossoverRate; } }

    [SerializeField]
    private int iterations = 10;
    public int Iterations { get { return iterations; } }

    [Space]
    [SerializeField]
    [Range(2, 3)]
    //2: nsga 2 3: nsga 3
    private int nsga = 1;
    public int NSGA { get { return nsga; } }


    [Space]
    [SerializeField] [Range(0, 1)] private float frequency = 1;
    [SerializeField] [Range(0, 1)] private float linearity = 1;
    [SerializeField] [Range(0, 1)] private float compactness = 1;
    [SerializeField] [Range(0, 1)] private float coverage = 1;

    [SerializeField] [Range(0, 1)] private float feasibility = 1;

    private Dictionary<string, float> fitFunc = new Dictionary<string, float>();
    public Dictionary<string, float> FitFunc
    {
        get
        {
            if(fitFunc.Count == 0)
            {
                if (frequency > 0) fitFunc.Add("freq", frequency);
                if (linearity > 0) fitFunc.Add("lin", linearity);
                if (compactness > 0) fitFunc.Add("comp", compactness);
                if (coverage > 0) fitFunc.Add("cov", coverage);
                if (feasibility > 0) fitFunc.Add("feas", feasibility);
            }
            return fitFunc;
        }
    }

    [Space]
    [Space]
    [Space]
    [Space]
    [SerializeField]
    [Range(1, 3)]
    //1: auto 2: manual 3: interactive
    private int evolutionMethod = 1;
    public int EvolutionMethod { get { return evolutionMethod; } }

}
