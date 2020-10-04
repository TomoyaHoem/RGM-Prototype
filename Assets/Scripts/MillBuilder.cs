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

    //1 if going up -1 if going down
    public int DirVert { get; set; }

    public override Vector2 GenerateRandomOutput(Vector2 directionPrev)
    {
        //save inverted previous direction
        dirChange *= Mathf.Sign(-directionPrev.x);

        //output is 3 units below/above input
        if(DirVert == -1)
        {
            dirChange.y = -1;
            return new Vector2(0, -3f);
        } else
        {
            dirChange.y = 1;
            return new Vector2(0, +3f);
        }

    }

    public override void GenerateSegment(GameObject parent)
    {
        //spawnposition
        if(dirChange.y == -1)
        {
            millSpawnPos = new Vector2(input.x + 0.5f * (-dirChange.x), input.y - 1.75f);
        } else
        {
            millSpawnPos = new Vector2(input.x + 0.5f * (-dirChange.x), input.y + 1.75f);
        }

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

    public override bool CheckEnoughRoom(Vector2 input, Vector2 output)
    {
        Vector2 InputTopCorner = new Vector2(input.x - 0.1f * (int)(Mathf.Sign(dirChange.x)), input.y - 0.9f * DirVert);
        //output -1 unit in dirChange direction 
        Vector2 OutputBottomCorner = new Vector2(output.x - 2.9f * (int)(Mathf.Sign(dirChange.x)), output.y + 0.9f * DirVert);

        if (Physics2D.OverlapArea(InputTopCorner, OutputBottomCorner) != null)
        {
            return false;
        }
        return true;
    }
}
