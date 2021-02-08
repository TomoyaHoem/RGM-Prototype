using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SegmentPart : MonoBehaviour
{
    public Vector2 Input { get; set; }
    public Vector2 Output { get; set; }
    public Vector2 InputDirection { get; set; }
    public Vector2 OutputDirection { get; set; }

    public void CopyIO(SegmentPart parent, Vector2 offset)
    {
        Input = parent.Input + offset;
        Output = parent.Output + offset;
        InputDirection = parent.InputDirection;
        OutputDirection = parent.OutputDirection;
    }

    public void MoveIO(Vector2 offset)
    {
        Input += offset;
        Output += offset;
    }

    public void MirrorIO()
    {
        Vector2 mir = new Vector2(-1, 1);

        Output = Input + (Output - Input) * mir;
        InputDirection *= mir;
        OutputDirection *= mir;
    }

    //unique identifier for segments
    public int SegmentID { get; set; }
    
    //public virtual Vector2 GetDirection()
    //{ return new Vector2(RGMTest.Sign(Output.x - Input.x), RGMTest.Sign(Output.y - Input.y)); }

    //get GO references
    public abstract void CopyProperties(GameObject seg, GameObject parent, Vector3 offset);

    public abstract void MoveSegmentBy(Vector2 offset);

    public abstract void MirrorSegment();

    //reset segments
    public abstract void ResetSegment();
}
