using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Segment : MonoBehaviour
{
    //in- and output of segment
    [SerializeField]
    protected Vector2 input;
    [SerializeField]
    protected Vector2 output;

    public Vector2 Output
    {
        get { return output;  }
        set { output = value; }
    }

    public Vector2 Input
    {
        get { return input; }
        set { input = value; }
    }

    public virtual Vector2 GetDirection()
    { return (output - input).normalized; }

    
    //generate segment using in- and output
    public abstract void GenerateSegment(GameObject parent);

    //generate random end point for Segments considering its limitations (e.g. Dominos on a straight platform)
    public abstract Vector2 GenerateRandomOutput(Vector2 directionPrev);

    //reset segments
    public abstract void ResetSegment();

    //check if enough space for segment (max size + delta)
    public abstract bool CheckEnoughRoom(Vector2 input, Vector2 output);
}
