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
}
