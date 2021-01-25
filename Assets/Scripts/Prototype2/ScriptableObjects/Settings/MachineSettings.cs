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
}
