using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTrackLogic : SegmentLogic
{
    //settings
    public float spacing = 0.05f;
    public float resolution = 1;
    private float meshWidth = 0.5f;

    //reference to Ball data container
    public BezierTrack BezierTrack { get; set; }

    public override void GetDataReference()
    {
        BezierTrack = gameObject.GetComponent<BezierTrack>();
    }

    //save BoundingBoxData for DrawGizmo
    Vector2 boundingBoxTopCorner;
    Vector2 boundingBoxBottomCorner;

    public override void SetOutputDirection(Vector2 prevDir)
    {
        BezierTrack.OutputDirection = new Vector2(prevDir.x, 0);
    }

    public override Vector2 GenerateRandomOutput(Vector2 prevDir)
    {
        //Debug.Log(BezierTrack.Input);

        float ranH = Random.Range(-5, .5f);
        int ranL = Random.Range(4, 16) * (int)prevDir.x;

        Vector2 output = new Vector2(ranL, ranH);

        int numPoints = Random.Range(1, (Mathf.Abs(ranL) / 4) + 1);
        List<Vector2> midPoints = CalculateMidPoints(numPoints, prevDir, output);

        //input ramp 0.5, output 1
        Path path = new Path(BezierTrack.Input + new Vector2(prevDir.x * 0.5f, -0.68f), new Vector2(BezierTrack.Input.x + output.x - 1 * prevDir.x, BezierTrack.Input.y + output.y - 0.68f), prevDir, midPoints);

        BezierTrack.EvenPoints = path.CalculateEvenlySpacedPoints(spacing, resolution);

        //Debug.Log(output);

        return new Vector2(output.x, output.y);
    }

    private List<Vector2> CalculateMidPoints(int numPoints, Vector2 prevDir, Vector2 output)
    {
        List<Vector2> points = new List<Vector2>();

        float start = BezierTrack.Input.x + prevDir.x * 0.5f;
        float end = start + output.x - 1.5f * prevDir.x;
        float dst = end - start;
        float step = dst / (numPoints + 1);

        for (int i = 0; i < numPoints; i++)
        {
            float pX = start + step + i * step;
            float hOffset = Random.Range(0, 2) * 2 - 1;
            float pY = Random.Range(BezierTrack.Input.y + hOffset, BezierTrack.Input.y + output.y + hOffset);

            points.Add(new Vector2(pX, pY));
        }

        return points;
    }

    public override void GenerateSegment()
    {
        //MESH
        gameObject.AddComponent<MeshRenderer>().material = Resources.Load("Materials/RampMaterial") as Material;
        MeshFilter bezierMesh = gameObject.AddComponent<MeshFilter>();
        bezierMesh.mesh = BezierMeshCreator.CreateBezierMesh(BezierTrack.EvenPoints, meshWidth);
        int textureRepeat = Mathf.RoundToInt(1 * BezierTrack.EvenPoints.Length * spacing * 0.5f);
        gameObject.GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);

        BezierMeshCreator.CreateBezierCollider(gameObject, bezierMesh.mesh, 0.5f);

        if(Random.Range(0, 2) > 0)
        {
            //Marble
            BezierTrack.BallSpawnPos = new Vector2(BezierTrack.EvenPoints[0].x + 0.2f * BezierTrack.InputDirection.x, BezierTrack.EvenPoints[0].y + meshWidth);
            BezierTrack.BallPiece = Instantiate(Resources.Load("Prefabs/Ball"), BezierTrack.BallSpawnPos, Quaternion.identity, gameObject.transform) as GameObject;
            BezierTrack.BallSpawnRotation = BezierTrack.BallPiece.transform.rotation;
            BezierTrack.BallPiece.GetComponent<Rigidbody2D>().mass = Random.Range(1, 5);
        }
    }

    public override bool CheckEnoughRoom(Vector2 input, Vector2 output)
    {
        CalcBoundingBox(BezierTrack.EvenPoints);

        if (Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner) != null)
        {
            //Debug.Log(Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner).name);
            return false;
        }
        return true;
    }

    public override bool CheckEnoughRoom(Vector2 input, Vector2 output, Vector2 offset, string s, bool mode)
    {
        CalcBoundingBox(BezierTrack.EvenPoints);

        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        if (collider != null && (collider.name.Equals(s) == mode))
        {
            //Debug.Log(Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner));
            return false;
        }
        return true;
    }

    public override bool CheckEnoughRoomMirrored(Vector2 input, Vector2 output, Vector2 offset, string s, bool mode)
    {
        CalcBoundingBox(BezierTrack.EvenPoints);
        //mirror bounding box
        //Debug.Log(input + " , " + BezierTrack.Input);
        Vector2 inputOffset = input - BezierTrack.Input;
        boundingBoxBottomCorner = new Vector2(input.x + (-0.1f) * BezierTrack.InputDirection.x, boundingBoxBottomCorner.y + inputOffset.y);
        boundingBoxTopCorner = new Vector2(output.x + (0.1f) * BezierTrack.InputDirection.x, boundingBoxTopCorner.y + inputOffset.y);

        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.yellow, 100);

        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        if (Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner) != null && (collider.name.Equals(s) == mode))
        {
            //Debug.Log(collider);
            return false;
        }
        return true;
    }

    private void CalcBoundingBox(Vector2[] points)
    {
        float minX = points[0].x, minY = points[0].y, maxX = points[points.Length - 1].x, maxY = points[0].y;

        foreach (Vector2 p in points)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

        boundingBoxBottomCorner = new Vector2(minX, minY - meshWidth * 2 / 3) + new Vector2(0.1f, 0);
        boundingBoxTopCorner = new Vector2(maxX, maxY + meshWidth) - new Vector2(0.1f, -0.25f);
    }

    private void OnDrawGizmosSelected()
    {
        if (BezierTrack.EvenPoints == null || BezierTrack.EvenPoints.Length == 0) return;
        CalcBoundingBox(BezierTrack.EvenPoints);
        //Debug.Log(boundingBoxBottomCorner + " , " + boundingBoxTopCorner);
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

    public override bool CheckSegmentOverlap(Vector2 offset, string s, bool mode, bool mirrored, float duration)
    {
        CalcBoundingBox(BezierTrack.EvenPoints);

        //mirror if needed 
        if (mirrored)
        {
            //calculate signed distance between input and output
            float xDistanceSegment = BezierTrack.Input.x - BezierTrack.Output.x;
            //mirror bounding box by adding signed distance
            boundingBoxBottomCorner.x += xDistanceSegment;
            boundingBoxTopCorner.x += xDistanceSegment;
        }

        //add offset
        boundingBoxBottomCorner += offset;
        boundingBoxTopCorner += offset;

        //draw for testing purposes
        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.green, duration);

        //calculate collider box
        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);
        
        //check for collision
        if ((Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner)) != null && (collider.name.Equals(s) == mode))
        {
            return false;
        }
        return true;
    }
}
