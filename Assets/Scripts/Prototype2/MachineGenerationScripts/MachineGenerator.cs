using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MachineGenerator : MonoBehaviour
{
    //responsible for the actual generation of machines and their parts

    //stores which segments have been tried at a position, to prevent generation getting stuck on same paths
    public List<List<int>> TriedSegments { get; set; }
    //counts how many times generation got stuck
    [SerializeField]
    private int stuckCount = 0;

    //reference to SegmentLogic and Machine scripts
    Machine machine;
    SegmentSelectionLogic sL;

    [SerializeField]
    Vector2 startDir;
    int backtrackAmount;

    //current MachineGeneration Process
    Task machineGeneration;
    //Event triggers when MachineGeneration finishes or Stops
    public event Action machineCompleteEvent;

    public void GenerateNewMachine()
    {
        float machineArea = SettingsReader.Instance.MachineSettings.MachineArea;
        int numSegments = SettingsReader.Instance.MachineSettings.NumSegments;

        MachineSetup(machineArea);

        //restriction area
        GenArea(machineArea);

        //start direction & start object
        /* REPLACE AUTO START AND DIR BY SPECIAL START SEGMENT? */
        GenAutoStart(startDir);

        //init TriedSeg
        TriedSegments = new List<List<int>>();
        backtrackAmount = (int)Mathf.Sqrt(numSegments);

        //generate machine
        machineGeneration = new Task(GenerateMachineSegments(numSegments));
    }

    private void MachineSetup(float machineArea)
    {
        //start direction for machine
        startDir = new Vector2(UnityEngine.Random.Range(0, 2) * 2 - 1, UnityEngine.Random.Range(-1, 2));
        sL = gameObject.AddComponent<SegmentSelectionLogic>();
        //data script
        machine = gameObject.AddComponent<Machine>();
        machine.AddSelectionArea(machineArea);
        machine.AddFitnessBarChart(machineArea);
    }

    public void StopMachineGeneration()
    {
        if(machineGeneration != null)
        {
            machineGeneration.Stop();
        }
        //notify Machine finished
        machineCompleteEvent?.Invoke();
    }

    private void GenArea(float machineArea)
    {
        GameObject area = new GameObject("restriction area");
        //set machine GO as parent and origin position
        area.transform.position = transform.position;
        area.transform.parent = transform;
        area.AddComponent<RestrictionArea>().GenerateRestrictionArea(machineArea);
    }

    private void GenAutoStart(Vector2 startDir)
    {
        GameObject start = new GameObject("AutoStart");
        //set machine GO as parent
        start.transform.parent = transform;
        start.AddComponent<AutoStart>().GenerateAutoStart(startDir, transform);
        //destroy placeholder GO
        Destroy(start);
    }

    public IEnumerator GenerateMachineSegments(int numSegments)
    {
        while(machine.Segments.Count < numSegments)
        {
            Task t = new Task(GenerateSegment(machine.Segments.Count));
            while (t.Running)
            {
                if(stuckCount > numSegments*2)
                {
                    StopMachineGeneration();
                }
                yield return null;
            }
            if (SettingsReader.Instance.MachineSettings.ManualGeneration)
            {
                while (!Input.GetKeyDown(KeyCode.Space))
                {
                    yield return null;
                }
            }
        }
        StopMachineGeneration();
    }

    private IEnumerator GenerateSegment(int SegNo)
    {
        //GO, TriedSegments for current segment
        GameObject segHol = new GameObject("Segment " + SegNo);
        if (TriedSegments.Count() == SegNo)
        {
            TriedSegments.Add(new List<int>());
        }
        //clear list ahead if stuck, backtrackamount sqrt of numsegments
        if (TriedSegments.Count > SegNo + backtrackAmount)
        {
            TriedSegments.RemoveRange(SegNo + 1, TriedSegments.Count - (SegNo + 1));
        }

        //list of segments not tried yet
        List<int> remaining = sL.RemainingSegments(TriedSegments[SegNo]);

        bool found = false;

        while(!found && remaining.Count > 0)
        {
            //sample random segment from remaining list
            int sampleR = UnityEngine.Random.Range(0, remaining.Count);
            int segID = remaining[sampleR]; remaining.Remove(segID);
            //assign segment & logic
            sL.AssignSegment(segHol, segID);
            //assign in- / output (first seg or intermitten)
            if(machine.Segments.Count == 0)
            {
                sL.SetSegmentIO(segHol, transform.position, startDir);
            } else
            {
                sL.SetSegmentIO(segHol, machine.Segments.Last().GetComponent<SegmentPart>().Output, machine.Segments.Last().GetComponent<SegmentPart>().OutputDirection);
            }
            //check if segment fits, if not destroy segment+logic script & wait 1 frame before next loop so that destroy finishes
            if (sL.CheckSegmentRoom(segHol))
            {
                found = true;
                break;
            } else
            {
                sL.DestroySegmentComponents(segHol);
            }
            yield return null;
        }
        //if a fittings segment was found generate it, else backtrack
        if (found)
        {
            segHol.GetComponent<SegmentLogic>().GenerateSegment();
            machine.Segments.Add(segHol);
            segHol.transform.parent = gameObject.transform;
        } else
        {
            if (machine.Segments.Count == 0)
            {
                //first segment can not be generated -> terminate
                Debug.Log("NO ROOM FOR FIRST SEGMENT, TERMINATING");
                StopMachineGeneration();
            }
            else
            {
                stuckCount++;
                TriedSegments[SegNo - 1].Add(machine.Segments.Last().GetComponent<SegmentPart>().SegmentID);
                Destroy(machine.Segments.Last());
                machine.Segments.RemoveAt(machine.Segments.Count - 1);
                Destroy(segHol);
            }
        }
    }
}
