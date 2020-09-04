using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Segment : MonoBehaviour
{

    //in- and output of segment
    protected Vector2 input;
    protected Vector2 output;

    public Vector2 Output
    {
        get
        {
            return output;
        }
    }

    public Vector2 Input
    {
        get
        {
            return input;
        }
    }

    //generate segment using in- and output
    public abstract void GenerateSegment();

    //generate random end point for Segments considering its limitations (e.g. Dominos on a straight platform)
    public abstract Vector2 GenerateRandomOutput(Vector2 input);
}
