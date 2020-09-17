using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MillBuilder : Segment
{

    private Vector2 dirChange = new Vector2(1, 0);
    
    //Mill direction is inverse of previous
    public override Vector2 GetDirection()
    {
        return dirChange;
    }

    public override Vector2 GenerateRandomOutput(Vector2 directionPrev)
    {
        //save inverted previous direction
        dirChange *= Mathf.Sign(-directionPrev.x);

        //output is 3 units below input
        return new Vector2(0, -3f);
    }

    public override void GenerateSegment(GameObject parent)
    {
        //spawnposition
        Vector2 spawnPos = new Vector2(input.x + 0.5f * (-dirChange.x), input.y - 1.75f);

        //instantiate
        Instantiate(Resources.Load("Prefabs/Mill"), spawnPos, Quaternion.identity, parent.transform);
    }
}
