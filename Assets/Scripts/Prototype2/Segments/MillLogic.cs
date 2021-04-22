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

    public override void SetOutputDirection(Vector2 prevDir)
    {
        Mill.OutputDirection = new Vector2(prevDir.x * -1, RGMTest.Sign(Mill.Output.y - Mill.Input.y));
    }

    public override void GenerateSegment()
    {
        //spawnposition
        float xOffset = 0.25f + 0.5f * Mill.Scale;
        float yOffset = -0.5f + 2 * Mill.Scale;
        Mill.MillSpawnPos = new Vector2(Mill.Input.x + Mill.InputDirection.x * xOffset, Mill.Input.y + Mill.OutputDirection.y * yOffset);

        //instantiate
        Mill.MillPiece = Instantiate(Resources.Load("Prefabs/Mill"), Mill.MillSpawnPos, Quaternion.identity, gameObject.transform) as GameObject;
        Mill.MillPiece.transform.localScale *= Mill.Scale;
        Mill.MillPiece.GetComponent<Rigidbody2D>().mass = Random.Range(3, 8);

        Mill.MillSpawnRotation = Mill.transform.rotation;
    }

    private void CalcBoundingBox()
    {
        float a = Mill.Input.x + 0.1f * Mill.InputDirection.x;
        float b = Mill.Output.x + (Mill.Scale * 0.5f + 2.9f) * Mill.InputDirection.x;

        float c = Mill.Input.y - 0.9f * Mill.OutputDirection.y;
        float d = Mill.Output.y + 0.9f * Mill.OutputDirection.y;

        float minX = Mathf.Min(a, b);
        float maxX = Mathf.Max(a, b);

        float minY = Mathf.Min(c, d);
        float maxY = Mathf.Max(c, d);

        boundingBoxBottomCorner = new Vector2(minX, minY);
        boundingBoxTopCorner = new Vector2(maxX, maxY);
    }

    public override bool CheckSegmentOverlap(Vector2 offset, string s, bool mode, bool mirrored, float duration)
    {
        CalcBoundingBox();

        //mirror if needed 
        if (mirrored)
        {
            //calculate signed distance between input and output
            float xDistanceSegment = (boundingBoxBottomCorner.x - boundingBoxTopCorner.x - 0.2f) * Mill.InputDirection.x;
            
            //mirror bounding box by adding signed distance
            boundingBoxBottomCorner.x += xDistanceSegment;
            boundingBoxTopCorner.x += xDistanceSegment;
        }

        //add offset
        boundingBoxBottomCorner += offset;
        boundingBoxTopCorner += offset;

        //draw for testing purposes
        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.yellow, duration);

        //calculate collider box
        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        //check for collision
        if ((Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner)) != null && (collider.name.Equals(s) == mode))
        {
            return false;
        }
        return true;
    }
    
    private void OnDrawGizmosSelected()
    {
        CalcBoundingBox();
        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.red, 0);
    }

    private void DrawRectangle(Vector2 topCorner, Vector2 bottomCorner, Color color, float duration)
    {
        Vector2 topOppositeCorner = new Vector2(bottomCorner.x, topCorner.y);
        Vector2 bottomOppositeCorner = new Vector2(topCorner.x, bottomCorner.y);

        Debug.DrawLine(topCorner, topOppositeCorner, color, duration);
        Debug.DrawLine(topOppositeCorner, bottomCorner, color, duration);
        Debug.DrawLine(bottomCorner, bottomOppositeCorner, color, duration);
        Debug.DrawLine(bottomOppositeCorner, topCorner, color, duration);
    }

    /*
     * DEPRECATED
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

    public override bool CheckEnoughRoomMirrored(Vector2 input, Vector2 output, Vector2 offset, string s, bool mode)
    {
        CalculateBoundingBoxesMirrored(input, output);
        //Debug.Log(gameObject.name + boundingBoxTopCorner + boundingBoxBottomCorner);

        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.yellow, 100);

        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);
        if (Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner) != null && (collider.name.Equals(s) == mode))
        {
            return false;
        }
        return true;
    }

    private void CalculateBoundingBoxes(Vector2 input, Vector2 output)
    {
        boundingBoxTopCorner = new Vector2(input.x + 0.1f * Mill.InputDirection.x, input.y - 0.9f * Mill.OutputDirection.y);
        //output -1 unit in dirChange direction 
        boundingBoxBottomCorner = new Vector2(output.x + 2.9f * Mill.InputDirection.x, output.y + 0.9f * Mill.OutputDirection.y);
    }

    private void CalculateBoundingBoxesMirrored(Vector2 input, Vector2 output)
    {
        boundingBoxTopCorner = new Vector2(input.x - 0.1f * Mill.InputDirection.x, input.y - 0.9f * Mill.OutputDirection.y);
        //output -1 unit in dirChange direction 
        boundingBoxBottomCorner = new Vector2(output.x - 2.9f * Mill.InputDirection.x, output.y + 0.9f * Mill.OutputDirection.y);
    }
    */
}
