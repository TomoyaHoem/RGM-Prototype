using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MillLogic : SegmentLogic
{
    //reference to Mill data container
    public Mill Mill { get; set; }

    public override void GetDataReference()
    {
        Mill = gameObject.GetComponent<Mill>();
    }

    //save BoundingBoxData for DrawGizmo
    Vector2 boundingBoxTopCorner;
    Vector2 boundingBoxBottomCorner;

    public override Vector2 GenerateRandomOutput(Vector2 prevDir)
    {
        //set mill direction to:
        //horizontal -> inverse of previous, vertical -> same as previous or if previous is 0 random (1 or -1)
        float verDir = Mill.SegmentID == 2 ? -1 : 1;
        Mill.Direction = new Vector2(prevDir.x * (-1), verDir);

        Mill.Scale = Random.Range(0.5f, 2f);
        float displacement = Mill.Scale * 4 - 1;

        //output is 3 units below/above input
        if (Mill.SegmentID == 2)
        {
            return new Vector2(0, -displacement);
        }
        else
        {
            return new Vector2(0, +displacement);
        }
    }

    public override void GenerateSegment()
    {
        //spawnposition
        float xOffset = 0.25f + 0.25f * Mill.Scale;
        float yOffset = -0.5f + 2 * Mill.Scale;
        Mill.MillSpawnPos = new Vector2(Mill.Input.x - Mill.Direction.x * xOffset, Mill.Input.y + Mathf.Sign(Mill.Output.y - Mill.Input.y) * yOffset);

        //instantiate
        Mill.MillPiece = Instantiate(Resources.Load("Prefabs/Mill"), Mill.MillSpawnPos, Quaternion.identity, gameObject.transform) as GameObject;
        Mill.MillPiece.transform.localScale *= Mill.Scale;
        Mill.MillPiece.GetComponent<Rigidbody2D>().mass = Random.Range(3, 8);

        Mill.MillSpawnRotation = Mill.transform.rotation;
    }

    public override bool CheckEnoughRoom(Vector2 input, Vector2 output)
    {
        CalculateBoundingBoxes(input, output);

        if (Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner) != null)
        {
            //Debug.Log(Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner).name);
            return false;
        }
        return true;
    }

    public override bool CheckEnoughRoom(Vector2 input, Vector2 output, Vector2 offset, string s, bool mode)
    {
        CalculateBoundingBoxes(input + offset, output + offset);

        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        if (collider != null && (collider.name.Equals(s) == mode))
        {
            return false;
        }
        return true;
    }

    private void CalculateBoundingBoxes(Vector2 input, Vector2 output)
    {
        boundingBoxTopCorner = new Vector2(input.x - 0.1f * (int)(Mathf.Sign(Mill.Direction.x)), input.y - 0.9f * Mill.Direction.y);
        //output -1 unit in dirChange direction 
        boundingBoxBottomCorner = new Vector2(output.x - 2.9f * (int)(Mathf.Sign(Mill.Direction.x)), output.y + 0.9f * Mill.Direction.y);
    }

    private void OnDrawGizmosSelected()
    {
        CalculateBoundingBoxes(Mill.Input, Mill.Output);
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
