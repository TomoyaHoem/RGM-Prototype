using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MachineSettings")]
public class MachineSettings : ScriptableObject
{
    [Header("Machine Settings")]
    [SerializeField] private int numSegments = 10;
    public int NumSegments { get { return numSegments; } }

    [SerializeField] private float machineArea = 30;
    public float MachineArea { get { return machineArea; } }

    [Space]
    [SerializeField]
    [Range(1, 3)]
    //1: square 2: circle 3: triangle
    private int areaShape = 1;
    public int AreaShape { get { return areaShape; } }

    [Space]
    [SerializeField] private bool manualGeneration = false;
    public bool ManualGeneration { get { return manualGeneration; } }

    //machine generation tests
    public int SuccessfullMachines { get; set; }
    public float GenerationTime { get; set; }
    public int StuckCount { get; set; }

    public int CoverageCount { get; set; }
    public float Coverage { get; set; }

    //EA tests
    public float TotalTestingTime { get; set; }
    public float AverageCompleteTime { get; set; }
    public int CompleteCount { get; set; }

    public List<float> Fps { get; set; }

    [SerializeField] private float limit = 1.2f;
    public float Limit { get { return limit; } }

    [SerializeField] private int speedUp = 30;
    public int SpeedUp { get { return speedUp; } }
    [SerializeField] private int parallelMachines = 8;
    public int ParallelMachines { get { return parallelMachines; } }
    public float MaxTime { get; set; }

    public List<List<float>> EAStatistics { get; set; }
}
