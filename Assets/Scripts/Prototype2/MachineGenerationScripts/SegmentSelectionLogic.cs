using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SegmentSelectionLogic : MonoBehaviour
{
    //all possible segments (by ID)
    List<int> possibleSegments = new List<int>();

    public int NumPossibleSegments
    {
        get
        {
            return possibleSegments.Count;
        }
    }

    private void Awake()
    {
        possibleSegments.Add(0);
        possibleSegments.Add(1);
        possibleSegments.Add(2);
        possibleSegments.Add(3);
        possibleSegments.Add(4);
        possibleSegments.Add(5);
    }

    //decide which segment to add based on ID
    //{ 0:Domino, 1:BallTrack, 2:Mill }
    public void AssignSegment(GameObject segHol, int segID)
    {
        switch (segID)
        {
            case 0:
                segHol.AddComponent<DominoLogic>();
                segHol.GetComponent<DominoLogic>().Domino = segHol.AddComponent<Domino>();
                break;
            case 1:
                segHol.AddComponent<BezierTrackLogic>();
                segHol.GetComponent<BezierTrackLogic>().BezierTrack = segHol.AddComponent<BezierTrack>();
                break;
            case 2:
                segHol.AddComponent <MillLogic>();
                segHol.GetComponent<MillLogic>().Mill = segHol.AddComponent<Mill>();
                segHol.GetComponent<Mill>().SegmentID = 2; //MILL DOWN
                break;
            case 3:
                segHol.AddComponent<MillLogic>();
                segHol.GetComponent<MillLogic>().Mill = segHol.AddComponent<Mill>();
                segHol.GetComponent<Mill>().SegmentID = 3; //MILL UP
                break;
            case 4:
                segHol.AddComponent<HammerLogic>();
                segHol.GetComponent<HammerLogic>().Hammer = segHol.AddComponent<Hammer>();
                break;
            case 5:
                segHol.AddComponent<CarTrackLogic>();
                segHol.GetComponent<CarTrackLogic>().Car = segHol.AddComponent<Car>();
                break;
            default:
                Debug.Log("Could not identify segmentID: " + segID);
                break;
        }
    }

    public bool CheckSegmentRoom(GameObject segHol)
    {
        SegmentLogic segLog = segHol.GetComponent<SegmentLogic>();
        return segLog.CheckSegmentOverlap(Vector2.zero, "", false, false, 10);
    }

    public List<int> RemainingSegments(List<int> triedSegments)
    {
        List<int> remaining = new List<int>(possibleSegments);
        foreach(int i in triedSegments)
        {
            if (remaining.Contains(i))
            {
                remaining.Remove(i);
            }
        }
        return remaining;
    }

    public void SetSegmentIO(GameObject segmentHolder, Vector2 input, Vector2 prevDir)
    {
        SegmentPart segP = segmentHolder.GetComponent<SegmentPart>();
        SegmentLogic segL = segmentHolder.GetComponent<SegmentLogic>();

        //input & input dir -> output & output dir of previous
        segP.InputDirection = prevDir;
        segP.Input = input;
        //generate random output
        segP.Output = input + segL.GenerateRandomOutput(prevDir);
        //output direction
        segL.SetOutputDirection(prevDir);
    }

    public void DestroySegmentComponents(GameObject segmentHolder)
    {
        Destroy(segmentHolder.GetComponent<SegmentPart>());
        Destroy(segmentHolder.GetComponent<SegmentLogic>());
    }

    public void DestroySegment(GameObject segmentHolder)
    {
        foreach(Transform child in segmentHolder.transform)
        {
            Destroy(child.gameObject);
        }
        DestroySegmentComponents(segmentHolder);
    }
}
