using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerLogic : SegmentLogic
{
    //reference to Hammer data container
    public Hammer Hammer { get; set; }

    public override void GetDataReference()
    {
        Hammer = gameObject.GetComponent<Hammer>();
    }

    //save BoundingBoxData for DrawGizmo
    Vector2 boundingBoxTopCorner;
    Vector2 boundingBoxBottomCorner;

    public override Vector2 GenerateRandomOutput(Vector2 prevDir)
    {
        //Debug.Log(Hammer.Input);

        Hammer.Scale = Random.Range(0.5f, 2.0f);
        //Debug.Log(Hammer.Scale);

        //3.2 distance + shaft * scale + hammerwidth
        float xOutput = 3.2f + 1.8f * Hammer.Scale + 0.4f;

        if (prevDir.x > 0) //right
        {
            return new Vector2(xOutput, -(0.2f + 1f + 1.8f * Hammer.Scale));
        }
        else //left
        {
            return new Vector2(-xOutput, -(0.2f + 1f + 1.8f * Hammer.Scale));
        }
    }

    public override void SetOutputDirection(Vector2 prevDir)
    {
        Hammer.OutputDirection = new Vector2(prevDir.x, 0);
    }

    public override void GenerateSegment()
    {
        //GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //g.transform.position = Hammer.Output;
        //g.transform.localScale = Vector3.one * 0.1f;

        //Hammer & Switch spawn positions
        Vector2 SwitchSpawnPos = new Vector2(Hammer.Input.x + 0.5f * Hammer.InputDirection.x, Hammer.Input.y - 0.42f);
        Hammer.HammerSpawnPos = new Vector2(Hammer.Input.x + (3.2f + 1.8f * Hammer.Scale) * Hammer.InputDirection.x, SwitchSpawnPos.y);

        //Instantiate Hammer and move Switch
        Hammer.HammerPiece = Instantiate(Resources.Load("Prefabs/Hammer"), Hammer.HammerSpawnPos, Quaternion.identity, gameObject.transform) as GameObject;
        GameObject Switch = Instantiate(Resources.Load("Prefabs/Switch"), SwitchSpawnPos, Quaternion.identity) as GameObject;

        //adjust hammer based on scaling
        //move sprites
        float scaleOffset = (Hammer.Scale - 1) * 1.8f;
        //Hammer Head
        Hammer.HammerPiece.transform.GetChild(0).transform.position += new Vector3(0, scaleOffset, 0);
        //Hammer Shaft
        Hammer.HammerPiece.transform.GetChild(1).transform.localScale = new Vector3(1, Hammer.Scale, 1);
        Hammer.HammerPiece.transform.GetChild(1).transform.position += new Vector3(0, scaleOffset / 2, 0);
        //adjust center of mass
        Hammer.HammerPiece.GetComponent<CenterOfMassChanger>().CenterOfMassNew.y += scaleOffset;
        //adjust collider
        Vector2[] newPoints = new Vector2[12];
        for(int i = 0; i < 12; i++)
        {
            if(i > 9)
            {
                newPoints[i] = Hammer.HammerPiece.GetComponent<PolygonCollider2D>().points[i];
            } else
            {
                newPoints[i] = Hammer.HammerPiece.GetComponent<PolygonCollider2D>().points[i] + new Vector2(0, scaleOffset / 4);
            }
        }

        Hammer.HammerPiece.GetComponent<PolygonCollider2D>().points = newPoints;

        //Random Hammer Rotation
        float rotationAngle = Random.Range(1, 90) * Hammer.InputDirection.x;
        Quaternion rotation = new Quaternion();
        rotation.eulerAngles = new Vector3(0, 0, rotationAngle);

        Hammer.HammerPiece.transform.rotation = rotation;
        Hammer.HammerSpawnRotation = rotation;

        //flip switch if direction is left and assign hammer and parent reference
        if (Hammer.InputDirection.x < 0)
        {
            Switch.transform.localScale = new Vector3(-1, 1, 1);
        }
        Switch.transform.parent = gameObject.transform;
        Switch.transform.GetChild(1).GetComponent<HammerSwitch>().Hammer = Hammer.HammerPiece;
    }

    public override bool CheckSegmentOverlap(Vector2 offset, string s, bool mode, bool mirrored, float duration)
    {
        CalcBoundingBox();

        //mirror if needed 
        if (mirrored)
        {
            //calculate signed distance between input and output
            float xDistanceSegment = Hammer.Input.x - Hammer.Output.x;
            //mirror bounding box by adding signed distance
            boundingBoxBottomCorner.x += xDistanceSegment;
            boundingBoxTopCorner.x += xDistanceSegment;
        }

        //add offset
        boundingBoxBottomCorner += offset;
        boundingBoxTopCorner += offset;

        //draw for testing purposes
        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.cyan, duration);

        //calculate collider box
        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        //check for collision
        if ((Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner)) != null && (collider.name.Equals(s) == mode))
        {
            return false;
        }
        return true;
    }

    private void CalcBoundingBox()
    {
        float a = Hammer.Input.x + 0.1f * Hammer.InputDirection.x;
        float b = Hammer.Output.x + 0.1f * Hammer.InputDirection.x;

        float minY = Hammer.Output.y - 0.5f;
        float maxY = Hammer.Input.y + (1f + 1.8f * Hammer.Scale);

        float minX = Mathf.Min(a, b);
        float maxX = Mathf.Max(a, b);

        boundingBoxBottomCorner = new Vector2(minX, minY);
        boundingBoxTopCorner = new Vector2(maxX, maxY);
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
}
