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

    [SerializeField]
    private int iterations = 10;
    public int Iterations { get { return iterations; } }

    [Space]
    [SerializeField] [Range(1, 3)]
    //1: auto 2: manual 3: interactive
    private int evolutionMethod = 1;
    public int EvolutionMethod { get { return evolutionMethod; } }

    [Space]
    [SerializeField] [Range(0, 1)] private float frequency = 1;
    [SerializeField] [Range(0, 1)] private float linearity = 1;
    [SerializeField] [Range(0, 1)] private float compactness = 1;
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
                if (feasibility > 0) fitFunc.Add("feas", feasibility);
            }
            return fitFunc;
        }
    }


}
