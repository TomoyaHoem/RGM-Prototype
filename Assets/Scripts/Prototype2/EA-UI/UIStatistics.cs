using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStatistics : MonoBehaviour
{
    //Statistics Class for UI
    //also responsible for calling Updates on UI parts

    //Singleton
    private static UIStatistics instance;
    public static UIStatistics Instance { get { return instance; } }

    //list of each UI object that needs updating
    public List<GameObject> UI = new List<GameObject>();
    //toggle for when update not needed
    public bool Active { get; set; }

    private void Awake()
    {
        instance = this;
    }

    public void InitUIData()
    {
        AverageObjectives = new float[SettingsReader.Instance.EASettings.FitFunc.Count - 1];
        CrossoverChance = new float[2];
    }

    //called when update needed
    //called each iteration of EA
    public void UpdateUI()
    {
        foreach (GameObject g in UI)
        {
            g.GetComponent<UIPart>().UpdateStatistics();
        }
    }

    //population statistics
    public int PopulationSize { get; set; }
    public int FeasSize { get; set; }
    public int InfeasSize { get; set; }
    //population transfer statistics
    public int FeasChildCount { get; set; }
    public int InfeasChildCount { get; set; }
    public int FeasTransfer { get; set; }
    public int InfeasTransfer { get; set; }
    public int FeasStay { get; set; }
    public int InfeasStay { get; set; }
    //EA information
    public int Iteration { get; set; }
    public string CurrentStep { get; set; }
    public string NSGAMethod { get; set; }
    //Plot statistics
    //Feasibility
    public float AverageFeasibility { get; set; }
    //Objectives
    public float[] AverageObjectives { get; set; }
    //Crossover 0 -> infeas, 1 -> feas
    public float[] CrossoverChance { get; set; }
    //Mutation
    public float MutationChance { get; set; }
    //Machine Length
    public float AverageMachineLength { get; set; }
    //Performance
}
