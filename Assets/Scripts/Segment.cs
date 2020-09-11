using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Segment : MonoBehaviour
{

    //in- and output of segment
    protected Vector2 input;
    protected Vector2 output;

    //direction of segment
    protected Vector2 direction;

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

    public Vector2 Direction
    {
        get { return output-input; }
    }

    //generate segment using in- and output
    public abstract void GenerateSegment(GameObject parent);

    //generate random end point for Segments considering its limitations (e.g. Dominos on a straight platform)
    public abstract Vector2 GenerateRandomOutput(Vector2 input, Vector2 directionPrev);
}
