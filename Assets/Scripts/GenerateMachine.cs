using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//randomly generates a RGM machine by starting from a random point and connecting generated arbitrary segments
public class GenerateMachine : MonoBehaviour
{
    //starting position
    [SerializeField]
    private Vector2 start = new Vector2(0,0);
    //number of segments in the machine
    [SerializeField]
    private int numOfSegments = 5;
    private List<Segment> _machine = new List<Segment>();

    /*

    void Awake()
    {
        while(numOfSegments > 0)
        {
            //empty List -> first Segment starts at starting position
            if(_machine.Count == 0)
            {
                _machine.Add(GenerateSegment(new IO(start, new Vector2(0,0))));
            } else //next Segment begins at current (last segment) endpoint 
            {
                _machine.Add(GenerateSegment(_machine[_machine.Count-1].IO));
            }
            numOfSegments--;
        }
    }

    //generate arbitrary segments
    //needs logic for randomizing segments and connection
    Segment GenerateSegment(IO prev) //probably need direction aswell
    {
        //choose and instantiate arbitrary segment
        Segment temp;

        //0 -> Domino, 1 -> BallTrack
        int random = Random.Range(0, 2);

        
        if (random == 0)
        {
            temp = new Domino();
        } else
        {
            temp = new BallTrack();
        }
        

        temp = gameObject.AddComponent<Domino>();

        //random end point
        temp.GenerateRandomEndPoint(prev);

        //generate segment
        temp.Generate();

        //return segment
        if(temp == null)
        {
            Debug.Log("could not generate segment");
        }
        return temp;
    }

    */

}
