using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CoroutineMG : MonoBehaviour
{

    [SerializeField]
    public Vector2 start = new Vector2(0, 0);
    [SerializeField]
    private int numOfSegments = 2;
    [SerializeField]
    private Vector2 startDir = new Vector2(1, 0);

    [Range(0, 4)]
    [SerializeField]
    private float timeBetweenSegments = 1.0f;
    [SerializeField]
    bool generateOnKeyInput = false;

    private GameObject autoStart;

    public List<Segment> machine = new List<Segment>();

    //stores which segments have been tried at position i, to prevent generation getting stuck on same paths
    [SerializeField]
    private List<List<string>> triedSegments = new List<List<string>>();

    private void Awake()
    {
        //autoStart
        SpawnAutoStart();
        //build Machine
        StartCoroutine(BuildMachine());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {

            Debug.Log("Resetting Level");

            foreach (Segment segment in machine)
            {
                segment.ResetSegment();
            }
        }
    }

    void SpawnAutoStart()
    {
        Vector2 spawnPos = Vector2.zero;

        if (startDir.x > 0) //place to the left of start
        {
            spawnPos = new Vector2(-0.75f, start.y);
        }
        else //place to the right of start
        {
            spawnPos = new Vector2(0.75f, start.y);
        }

        //instantiate
        autoStart = Instantiate(Resources.Load("Prefabs/AutoStart"), spawnPos, Quaternion.identity) as GameObject;
        autoStart.GetComponent<AutoStart>().PistonDirection = startDir;
    }

    IEnumerator BuildMachine()
    {
        for (int i = 0; i < numOfSegments; i++)
        {
            yield return StartCoroutine(BuildRandomSegment(i));
        }
    }

    IEnumerator BuildRandomSegment(int segmentNum)
    {
        GameObject segmentHolder = new GameObject("Segment " + segmentNum);

        //check if list already exists (in case of retry)
        if (triedSegments.Count() == segmentNum)
        {
            triedSegments.Add(new List<string>());
        }

        //first segment BallTrack
        if (segmentNum == 0)
        {
            segmentHolder.AddComponent<DominoBuilder>();

            //set input as start
            segmentHolder.GetComponent<Segment>().Input = start;
            //generate random output for current segment based on start pos
            segmentHolder.GetComponent<Segment>().Output = segmentHolder.GetComponent<Segment>().GenerateRandomOutput(startDir);
        }
        else //other Segments try to find random Fitting Segment
        {
            int r = UnityEngine.Random.Range(0, 3);

            if (!FindFittingSegment(segmentHolder, r, 0, segmentNum))
            {
                //unable to find fitting Segment -> add to List, destroy previous and current and rebuild, continue
                //add previous segmenttype to list
                //Debug.Log(IdentifySegment(machine[machine.Count - 1].gameObject));
                triedSegments[segmentNum-1].Add(IdentifySegment(machine[machine.Count - 1].gameObject));
                //destroy previous
                Destroy(machine[machine.Count - 1].gameObject);
                //remove previous 
                machine.RemoveAt(machine.Count - 1);
                //destroy current
                Destroy(segmentHolder);
                //do a new call for the destroyed object, build a new segment and try again for current segment
                yield return StartCoroutine(BuildRandomSegment(segmentNum - 1));
                yield return StartCoroutine(BuildRandomSegment(segmentNum));
                //go to next iteration
                yield break;
            }

            //clear after unstuck will affect how much will be deleted when stuck
            if(triedSegments.Count > segmentNum+20)
            {
                triedSegments.RemoveRange(segmentNum+1, triedSegments.Count - (segmentNum+1));
            }
        }

        //get last added component since wrong components are still attached at this point in execution
        segmentHolder.GetComponents<Segment>().Last().GenerateSegment(segmentHolder);

        machine.Add(segmentHolder.GetComponents<Segment>().Last());

        if (generateOnKeyInput)
        {
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(timeBetweenSegments);
    }

    //check Type of Segment and return identifying String
    string IdentifySegment(GameObject prevSegment)
    {
        Segment prev = prevSegment.GetComponent<Segment>();

        if (typeof(DominoBuilder).IsInstanceOfType(prev))
        {
            return "Domino";
        }
        else if (typeof(BallTrack).IsInstanceOfType(prev))
        {
            return "Ball";
        }
        else if (typeof(MillBuilder).IsInstanceOfType(prev))
        {
            if (prev.GetDirection().y > 0)
            {
                return "MillUp";
            }
            else
            {
                return "MillDown";
            }
        }

        //should not happen
        Debug.Log("Could not identify Segment");
        return null;
    }

    bool FindFittingSegment(GameObject segmentHolder, int r, int iteration, int segmentNum)
    {
        if (iteration > 2)
        {
            //cant find fitting segment
            return false;
        }

        if (r == 0)
        {
            segmentHolder.AddComponent<DominoBuilder>();

            SetInOutput(segmentHolder);

            //check if segment has been considered at this point yet and if not, wether there is enough room for it
            if (triedSegments[segmentNum].Contains("Domino") || !segmentHolder.GetComponent<DominoBuilder>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
            {
                Destroy(segmentHolder.GetComponent<DominoBuilder>());
                //try to find other Segment (>BallTrack)
                return FindFittingSegment(segmentHolder, 1, ++iteration, segmentNum);
            }
            return true;
        }
        else if (r == 1)
        {
            segmentHolder.AddComponent<BallTrack>();

            SetInOutput(segmentHolder);

            if (triedSegments[segmentNum].Contains("Ball") || !segmentHolder.GetComponent<BallTrack>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
            {
                Destroy(segmentHolder.GetComponent<BallTrack>());
                //try to find other Segment (>Mill)
                return FindFittingSegment(segmentHolder, 2, ++iteration, segmentNum);
            }
            return true;
        }
        else
        {
            segmentHolder.AddComponent<MillBuilder>();

            if (triedSegments[segmentNum].Contains("MillUp") && triedSegments[segmentNum].Contains("MillDown"))
            {
                Destroy(segmentHolder.GetComponent<MillBuilder>());
                //try to find other Segment (>Domino)
                return FindFittingSegment(segmentHolder, 0, ++iteration, segmentNum);
            }
            else if (triedSegments[segmentNum].Contains("MillUp") || triedSegments[segmentNum].Contains("MillDown"))
            {
                if (triedSegments[segmentNum].Contains("MillUp"))
                {
                    //try mill down
                    segmentHolder.GetComponent<MillBuilder>().DirVert = -1;
                    SetInOutput(segmentHolder);
                    if (!segmentHolder.GetComponent<MillBuilder>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
                    {
                        Destroy(segmentHolder.GetComponent<MillBuilder>());
                        //try to find other Segment (>Domino)
                        return FindFittingSegment(segmentHolder, 0, ++iteration, segmentNum);
                    }
                    return true;
                }
                else
                {
                    //try mill up
                    segmentHolder.GetComponent<MillBuilder>().DirVert = 1;
                    SetInOutput(segmentHolder);
                    if (!segmentHolder.GetComponent<MillBuilder>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
                    {
                        Destroy(segmentHolder.GetComponent<MillBuilder>());
                        //try to find other Segment (>Domino)
                        return FindFittingSegment(segmentHolder, 0, ++iteration, segmentNum);
                    }
                    return true;
                }
            }
            else
            {
                //vertical direciton random
                segmentHolder.GetComponent<MillBuilder>().DirVert = UnityEngine.Random.Range(0, 2) * 2 - 1;
                //check if previous segment was mill, if so, set vertical direction accordingly
                if(IdentifySegment(machine[machine.Count-1].gameObject) == "MillUp")
                {
                    segmentHolder.GetComponent<MillBuilder>().DirVert = 1;
                } else if (IdentifySegment(machine[machine.Count - 1].gameObject) == "MillDown")
                {
                    segmentHolder.GetComponent<MillBuilder>().DirVert = -1;
                }
                SetInOutput(segmentHolder);
                if (!segmentHolder.GetComponent<MillBuilder>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
                {
                    Destroy(segmentHolder.GetComponent<MillBuilder>());
                    //try to find other Segment (>Domino)
                    return FindFittingSegment(segmentHolder, 0, ++iteration, segmentNum);
                }
                return true;
            }
        }
    }

    void SetInOutput(GameObject segmentHolder)
    {
        Segment seg = segmentHolder.GetComponents<Segment>().Last();
        //set input to previous output location
        seg.Input = machine[machine.Count - 1].GetComponent<Segment>().Output;
        //generate random output for current segment based on previous direction
        seg.Output = seg.Input + seg.GenerateRandomOutput(machine[machine.Count - 1].GetComponent<Segment>().GetDirection());
    }

}
