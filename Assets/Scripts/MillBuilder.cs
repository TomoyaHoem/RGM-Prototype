using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MillBuilder : Segment
{

    private Vector2 millSpawnPos;
    private Quaternion millSpawnRotation;

    private GameObject mill;

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
        millSpawnPos = new Vector2(input.x + 0.5f * (-dirChange.x), input.y - 1.75f);

        //instantiate
        mill = Instantiate(Resources.Load("Prefabs/Mill"), millSpawnPos, Quaternion.identity, parent.transform) as GameObject;
        millSpawnRotation = mill.transform.rotation;
    }

    public override void ResetSegment()
    {
        //reset velocity
        mill.SetActive(false);
        mill.SetActive(true);

        //reset transform
        mill.transform.position = millSpawnPos;
        mill.transform.rotation = millSpawnRotation;
    }
}
