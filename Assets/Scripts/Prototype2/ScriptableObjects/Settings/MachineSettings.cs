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

    //EA tests
    public float TotalTestingTime { get; set; }
    public float AverageCompleteTime { get; set; }
    public int CompleteCount { get; set; }

    public int SpeedUp { get; set; }
    public List<float> Fps { get; set; }
    public float Limit { get; set; }

    public int ParallelMachines { get; set; }
    public float MaxTime { get; set; }
}
