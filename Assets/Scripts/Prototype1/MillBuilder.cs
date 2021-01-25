using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MillBuilder : Segment
{

    private Vector2 millSpawnPos;
    private Quaternion millSpawnRotation;

    private GameObject mill;

    public Vector2 dirChange = new Vector2(1, 0);

    Vector2 boundingBoxTopCorner;
    Vector2 boundingBoxBottomCorner;

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
        CalculateBoundingBoxes(input, output);

        if (Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner) != null)
        {
            //Debug.Log(Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner));
            return false;
        }
        return true;
    }

    private void CalculateBoundingBoxes(Vector2 input, Vector2 output)
    {
        if(DirVert == 0)
        {
            DirVert = (int)Mathf.Sign(output.y - input.y);
        }
        boundingBoxTopCorner = new Vector2(input.x - 0.1f * (int)(Mathf.Sign(dirChange.x)), input.y - 0.9f * DirVert);
        //output -1 unit in dirChange direction 
        boundingBoxBottomCorner = new Vector2(output.x - 2.9f * (int)(Mathf.Sign(dirChange.x)), output.y + 0.9f * DirVert);
    }

    private void OnDrawGizmosSelected()
    {
        CalculateBoundingBoxes(Input, Output);
        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.red);
    }

    private void DrawRectangle(Vector2 topCorner, Vector2 bottomCorner, Color color)
    {
        Vector2 topOppositeCorner = new Vector2(bottomCorner.x, topCorner.y);
        Vector2 bottomOppositeCorner = new Vector2(topCorner.x, bottomCorner.y);

        Debug.DrawLine(topCorner, topOppositeCorner, color);
        Debug.DrawLine(topOppositeCorner, bottomCorner, color);
        Debug.DrawLine(bottomCorner, bottomOppositeCorner, color);
        Debug.DrawLine(bottomOppositeCorner, topCorner, color);
    }
}
