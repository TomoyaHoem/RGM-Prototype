using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SegmentPart : MonoBehaviour
{
    //in and output location of segment
    public Vector2 Input { get; set; }
    public Vector2 Output { get; set; }

    //unique identifier for segments
    public int SegmentID { get; set; }

    public virtual Vector2 GetDirection()
    { return new Vector2(RGMTest.Sign(Output.x - Input.x), RGMTest.Sign(Output.y - Input.y)); }

    //get GO references
    public abstract void CopyProperties(GameObject seg, GameObject parent, Vector3 offset);

    //reset segments
    public abstract void ResetSegment();
}
