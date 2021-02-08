using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SegmentLogic : MonoBehaviour
{
    //get data reference
    public abstract void GetDataReference();

    //generate segment using in- and output
    public abstract void GenerateSegment();

    //set output direction depending on type of segment
    public abstract void SetOutputDirection(Vector2 prevDir);

    //generate random end point for Segments considering its limitations (e.g. Dominos on a straight platform)
    public abstract Vector2 GenerateRandomOutput(Vector2 prevDir);

    //check if enough space for segment (max size + delta)
    public abstract bool CheckEnoughRoom(Vector2 input, Vector2 output);

    //check room only for string or anything but string depending on mode: true -> check only, false -> check any but
    public abstract bool CheckEnoughRoom(Vector2 input, Vector2 output, Vector2 offset, string s, bool mode);

    //calculate mirrored original bounding box and move to desired location
    public abstract bool CheckEnoughRoomMirrored(Vector2 input, Vector2 output, Vector2 offset);
}
