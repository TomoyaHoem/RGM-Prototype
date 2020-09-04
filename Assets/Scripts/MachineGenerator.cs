using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGenerator : MonoBehaviour
{
    [SerializeField]
    public Vector2 start = new Vector2(0,0);
    [SerializeField]
    private int numOfSegments = 5;

    public List<Segment> machine = new List<Segment>();

    void Awake()
    {
        while(numOfSegments > 0)
        {
            machine.Add(BuildRandomSegment());

            numOfSegments--;
        }
    }


    Segment BuildRandomSegment()
    {
        GameObject segment = new GameObject();

        //choose segment
        //for testing purposes only dominos

        segment.AddComponent<DominoBuilder>();

        return segment.GetComponent<DominoBuilder>();
    }

}
