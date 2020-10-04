using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGenerator : MonoBehaviour
{
    [SerializeField]
    public Vector2 start = new Vector2(0,0);
    [SerializeField]
    private int numOfSegments = 2;
    [SerializeField]
    private Vector2 startDir = new Vector2(1, 0);

    public List<Segment> machine = new List<Segment>();

    private GameObject autoStart;

    void Awake()
    {

        //autoStart
        SpawnAutoStart();
        //build machine
        for (int i = 1; i < numOfSegments+1; i++)
        {
            machine.Add(BuildRandomSegment(i));
        }
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

        if(startDir.x > 0) //place to the left of start
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

    Segment BuildRandomSegment(int i)
    {
        GameObject segment = new GameObject("Segment "+ i);

        
        int ranSeg = 1;

        if(i > 1)
        {
            ranSeg = Random.Range(0,3);
        }

        //choose segment
        if (ranSeg == 0)
        {
            segment.AddComponent<DominoBuilder>();
        }
        else if(ranSeg == 1)
        {
            segment.AddComponent<BallTrack>();
        } else
        {
            segment.AddComponent<MillBuilder>();
        }

        if(machine.Count == 0)
        {
            //set input as start
            segment.GetComponent<Segment>().Input = start;
            //generate random output for current segment based on start pos
            segment.GetComponent<Segment>().Output = segment.GetComponent<Segment>().GenerateRandomOutput(startDir);
        } else
        {
            //set input to previous output location
            segment.GetComponent<Segment>().Input = machine[machine.Count-1].GetComponent<Segment>().Output;
            //generate random output for current segment based on previous direction
            segment.GetComponent<Segment>().Output = segment.GetComponent<Segment>().Input + segment.GetComponent<Segment>().GenerateRandomOutput(machine[machine.Count - 1].GetComponent<Segment>().GetDirection());
        }

        segment.GetComponent<Segment>().GenerateSegment(segment);

        return segment.GetComponent<Segment>();
    }

}
