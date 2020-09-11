using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGenerator : MonoBehaviour
{
    [SerializeField]
    public Vector2 start = new Vector2(0,0);
    [SerializeField]
    private int numOfSegments = 2;

    public List<Segment> machine = new List<Segment>();

    void Awake()
    {
        for(int i = 1; i < numOfSegments+1; i++)
        {
            machine.Add(BuildRandomSegment(i));
        }
    }


    Segment BuildRandomSegment(int i)
    {
        GameObject segment = new GameObject(" Segment"+ i);

        //choose segment
        //for testing purposes only dominos

        segment.AddComponent<DominoBuilder>();

        if(machine.Count == 0)
        {
            //set input as start
            segment.GetComponent<Segment>().Input = start;
            //generate random output for current segment based on start pos
            segment.GetComponent<Segment>().Output = segment.GetComponent<Segment>().GenerateRandomOutput(start, Vector2.zero);
        } else
        {
            //set input as previous output + 1 unit
            segment.GetComponent<Segment>().Input = machine[machine.Count-1].GetComponent<Segment>().Output;
            //generate random output for current segment based on previous direction
            segment.GetComponent<Segment>().Output = segment.GetComponent<Segment>().Input + segment.GetComponent<Segment>().GenerateRandomOutput(segment.GetComponent<Segment>().Input, machine[machine.Count - 1].GetComponent<Segment>().Direction);
        }

        segment.GetComponent<Segment>().GenerateSegment(segment);

        return segment.GetComponent<Segment>();
    }

}
