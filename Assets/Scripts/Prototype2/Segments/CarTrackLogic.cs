using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTrackLogic : SegmentLogic
{
    //settings
    public float spacing = 0.1f;
    public float resolution = 1;
    private float meshWidth = 0.5f;

    //reference to Ball data container
    public Car Car { get; set; }

    public override void GetDataReference()
    {
        Car = gameObject.GetComponent<Car>();
    }

    //save BoundingBoxData for DrawGizmo
    Vector2 boundingBoxTopCorner;
    Vector2 boundingBoxBottomCorner;

    public override void SetOutputDirection(Vector2 prevDir)
    {
        Car.OutputDirection = new Vector2(prevDir.x, 0);
    }

    public override Vector2 GenerateRandomOutput(Vector2 prevDir)
    {
        //Debug.Log(BezierTrack.Input);

        float ranH = Random.Range(-4, 5);
        int ranL = Random.Range(16, 32) * (int)prevDir.x;

        Vector2 output = new Vector2(ranL, ranH);

        int numPoints = Random.Range(1, (Mathf.Abs(ranL) / 4) + 1);
        List<Vector2> midPoints = CalculateMidPoints(numPoints, prevDir, output);

        //Debug.Log(numPoints + " points " + ranL);

        //input ramp 0.5, output 1
        Path path = new Path(Car.Input + new Vector2(prevDir.x * 2.5f, -0.68f), new Vector2(Car.Input.x + output.x - 3.5f * prevDir.x, Car.Input.y + output.y - 0.68f), prevDir, midPoints);

        Car.EvenPoints = path.CalculateEvenlySpacedPoints(spacing, 2.5f, resolution);

        //Debug.Log(output);

        return output;
    }

    private List<Vector2> CalculateMidPoints(int numPoints, Vector2 prevDir, Vector2 output)
    {
        List<Vector2> points = new List<Vector2>();

        float start = Car.Input.x + prevDir.x * 2.5f;
        float end = Car.Input.x + output.x - 3.5f * prevDir.x;
        float dst = end - start;
        float step = dst / (numPoints + 1);

        Vector2 beg = new Vector2(start, Car.Input.y - 0.68f);
        Vector2 fin = new Vector2(end, Car.Input.y + output.y - 0.68f);
        Vector2 con = fin - beg;
        Vector2 curP = Vector2.zero;
        //Debug.DrawLine(beg, fin, Color.white, 100f);

        float pY = 0;

        for (int i = 0; i < numPoints; i++)
        {
            //x position of point
            float pX = start + step + i * step;

            //y position origin line between start and end
            //Debug.Log(beg + " , " + con + " , " + (i + 1) + " / " + (numPoints + 1));
            curP = beg + con * (i + 1) / (numPoints + 1);
            pY = curP.y;
            //Debug.Log("middle height: " + pY);
            
            //random heightoffset direction up or down
            float hOffsetDir = Random.Range(0, 2) * 2 - 1;
            
            //heightoffset based on point
            float maxOffset = 0;
            //first after start or before end
            if(i == 0 || i == (numPoints - 1))
            {
                maxOffset = 0 + CalcStepOffset(step);
            } else if(i == 1 || i == (numPoints - 2))
            {
                maxOffset = 0.5f + CalcStepOffset(step);
            } else
            {
                maxOffset = 1 + CalcStepOffset(step);
            }
            
            pY = Random.Range(pY - maxOffset, pY + maxOffset);
            
            //Debug.Log("Max possible at " + i + ": " + maxOffset + " pY: " + pY);

            points.Add(new Vector2(pX, pY));
        }

        return points;
    }

    float CalcStepOffset(float step)
    {
        if(step < 3)
        {
            return 0.5f;
        } else if(step < 5)
        {
            return 1.5f;
        } else if(step < 8)
        {
            return 2.5f;
        } else
        {
            return 3.5f;
        }
    }

    public override void GenerateSegment()
    {
        //CURVE MESH
        GameObject curve = new GameObject("Curve");
        curve.transform.parent = gameObject.transform;
        curve.AddComponent<MeshRenderer>().material = Resources.Load("Materials/RampMaterial") as Material;
        MeshFilter bezierMesh = curve.AddComponent<MeshFilter>();
        bezierMesh.mesh = BezierMeshCreator.CreateBezierMesh(Car.EvenPoints, meshWidth);
        int textureRepeat = Mathf.RoundToInt(1 * Car.EvenPoints.Length * spacing * 0.5f);
        curve.GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        //Rigidbody
        Rigidbody2D rb = curve.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        rb.sharedMaterial = Resources.Load("Materials/RoadMaterial") as PhysicsMaterial2D;

        BezierMeshCreator.CreateBezierCollider(curve, bezierMesh.mesh, 0.5f);


        if (Random.Range(0, 4) > 0)
        {
            //Marble
            Vector2 carSpawn = new Vector2(Car.EvenPoints[0].x + 1.25f * Car.InputDirection.x, Car.EvenPoints[0].y + meshWidth + 0.1f);
            Car.CarPiece = Instantiate(Resources.Load("Prefabs/NewCar"), carSpawn, Quaternion.identity, gameObject.transform) as GameObject;
            
            //flip car if direction is left
            if (Car.InputDirection.x < 0)
            {
                Car.CarPiece.transform.localScale = new Vector3(-1, 1, 1);
                JointMotor2D newMotor = new JointMotor2D();
                newMotor.motorSpeed = 1000f;
                newMotor.maxMotorTorque = 10000f;
                Car.CarPiece.transform.GetChild(0).transform.GetChild(0).GetComponent<CarEngine>().Tire1.motor = newMotor;
                Car.CarPiece.transform.GetChild(0).transform.GetChild(0).GetComponent<CarEngine>().Tire2.motor = newMotor;
                Car.CarPiece.transform.GetChild(0).transform.GetChild(0).GetComponent<CarEngine>().Tire1.useMotor = false;
                Car.CarPiece.transform.GetChild(0).transform.GetChild(0).GetComponent<CarEngine>().Tire2.useMotor = false;
            }

            for (int i = 0; i < Car.CarPiece.transform.childCount; i++)
            {
                Car.CarPartSpawnPos.Add(Car.CarPiece.transform.GetChild(i).position);
                Car.CarPartSpawnRotation.Add(Car.CarPiece.transform.GetChild(i).rotation);
            }
        }
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
        boundingBoxTopCorner = new Vector2(maxX, maxY + meshWidth + 1.5f) - new Vector2(0.1f, -0.25f);
    }

    private void OnDrawGizmosSelected()
    {
        if (Car.EvenPoints == null || Car.EvenPoints.Length == 0) return;
        CalcBoundingBox(Car.EvenPoints);
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
        CalcBoundingBox(Car.EvenPoints);

        //mirror if needed 
        if (mirrored)
        {
            //calculate signed distance between input and output
            float xDistanceSegment = Car.Input.x - Car.Output.x;
            //mirror bounding box by adding signed distance
            boundingBoxBottomCorner.x += xDistanceSegment;
            boundingBoxTopCorner.x += xDistanceSegment;
        }

        //add offset
        boundingBoxBottomCorner += offset;
        boundingBoxTopCorner += offset;

        //draw for testing purposes
        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.black, duration);

        //calculate collider box
        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        //check for collision
        if ((Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner)) != null && (collider.name.Equals(s) == mode))
        {
            return false;
        }
        return true;
    }

    public override float CalcCoverage()
    {
        CalcBoundingBox(Car.EvenPoints);

        float a = Mathf.Abs(boundingBoxTopCorner.x - boundingBoxBottomCorner.x);
        float b = Mathf.Abs(boundingBoxTopCorner.y - boundingBoxBottomCorner.y);

        return a * b;
    }
}
