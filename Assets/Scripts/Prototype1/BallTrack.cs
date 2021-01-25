using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTrack : Segment
{

    private Vector2 ballSpawnPos;
    private Quaternion ballSpawnRotation;

    [SerializeField]
    private GameObject ball;

    Vector2 boundingCircleCenterCir1;
    Vector2 boundingCircleCenterCir2;
    float boundingCircleRad;
    Vector2 boundingBoxTopCornerStart;
    Vector2 boundingBoxBottomCornerStartEnd;
    Vector2 boundingBoxTopCornerEnd;
    Vector2 boundingBoxBottomCornerEnd;

    Vector2 startToEnd;
    Vector2 circleCenter;

    public GameObject Ball { get => ball; set => ball = value; }

    public override Vector2 GenerateRandomOutput(Vector2 directionPrev)
    {
        int ranH = Random.Range(1, 5);
        int ranL = Random.Range(ranH + 2, ranH * 2);

        if (directionPrev.x > 0) //right
        {
            return new Vector2(ranL, -ranH);
        }
        else if (directionPrev.x < 0) //left
        {
            return new Vector2(-ranL, -ranH);
        }
        else
        { //no horizontal dir
            Debug.Log("vertical input direction not suited for BallTrack");
            return Vector2.zero;
        }
    }

    public override void GenerateSegment(GameObject parent)
    {

        float dir = Mathf.Sign(GetDirection().x);
        //place start and end platform at in and output

        //start
        Vector2 startSpawnPos = new Vector2(input.x + 0.25f * dir, input.y - 0.36f);
        GameObject start = Instantiate(Resources.Load("Prefabs/Platform"), startSpawnPos, Quaternion.identity, parent.transform) as GameObject;
        //ball
        ballSpawnPos = new Vector2(startSpawnPos.x - 0.2f * dir, startSpawnPos.y + 0.38f);
        Ball = Instantiate(Resources.Load("Prefabs/Ball"), ballSpawnPos, Quaternion.identity, parent.transform) as GameObject;
        ballSpawnRotation = Ball.transform.rotation;

        //end
        Vector2 endSpawnPos = new Vector2(output.x - 0.5f * dir, output.y - 0.36f);
        GameObject end = Instantiate(Resources.Load("Prefabs/Platform"), endSpawnPos, Quaternion.identity, parent.transform) as GameObject;
        end.transform.localScale += new Vector3(1, 0, 0);

        //connect start and end with Ramp

        //find top-right/top-left corner of start/end platform
        BoxCollider2D collider = start.GetComponent<BoxCollider2D>();
        Vector2 startTopRight = new Vector2(collider.bounds.center.x + collider.bounds.extents.x * dir, collider.bounds.center.y + collider.bounds.extents.y);

        collider = end.GetComponent<BoxCollider2D>();
        Vector2 endTopLeft = new Vector2(collider.bounds.center.x - 2 * collider.bounds.extents.x * dir, collider.bounds.center.y + collider.bounds.extents.y);

        //show points
        //Debug.DrawLine(startTopRight, startTopRight +  new Vector2(0.025f,0) * dir, Color.red, 30);
        //Debug.DrawLine(endTopLeft, endTopLeft - new Vector2(0.025f, 0) * dir, Color.magenta, 30);

        //find middle point
        Vector2 startToEndDir = endTopLeft - startTopRight;
        Vector2 middlepoint = startTopRight + startToEndDir * 0.5f;
        //Debug.DrawLine(middlepoint, middlepoint + new Vector2(0.025f, 0) * dir, Color.green, 30);

        //perpendicular vector to dir from middle point
        Vector2 middleToEndDir = startToEndDir.normalized;
        Vector2 perp = new Vector2(middleToEndDir.y * dir, -middleToEndDir.x * dir);
        //Debug.DrawLine(middlepoint, middlepoint + perp, Color.blue, 300);

        Vector2 rampSpawn = middlepoint + 0.125f * perp;
        GameObject ramp = Instantiate(Resources.Load("Prefabs/Ramp"), rampSpawn, Quaternion.FromToRotation(Vector2.right, startToEndDir), parent.transform) as GameObject;
        //scale ramp to magnitude of vector between start and end
        ramp.transform.localScale = new Vector3((startToEndDir.magnitude / ramp.GetComponent<SpriteRenderer>().size.x) - 0.0001f, 1f, 1f);

    }

    public void Update()
    {
        //destroy ball after reaching end of ramp (to not trigger other parts further ahead)
        if (Ball != null && Ball.transform.position.y < Output.y - 0.5f)
        {
            Ball.SetActive(false);
        }
    }

    public override void ResetSegment()
    {
        if (Ball != null)
        {
            //reset velocity
            Ball.SetActive(false);
            Ball.SetActive(true);

            //reset transform
            Ball.transform.position = ballSpawnPos;
            Ball.transform.rotation = ballSpawnRotation;
        }
    }

    public override bool CheckEnoughRoom(Vector2 input, Vector2 output)
    {
        CalculateBoundingBoxes(input, output);

        //check starting platform
        if (Physics2D.OverlapArea(boundingBoxTopCornerStart, boundingBoxBottomCornerStartEnd) != null)
        {
            return false;
        }

        //check end platform
        if (Physics2D.OverlapArea(boundingBoxTopCornerEnd, boundingBoxBottomCornerEnd) != null)
        {
            return false;
        }

        //check ramp
        //if ramp is long do multiple smaller circles
        if (startToEnd.magnitude < 3.0f)
        {
            if (Physics2D.OverlapCircle(circleCenter, (startToEnd.magnitude * 0.5f)) != null)
            {
                return false;
            }
        }
        else
        {
            if ((Physics2D.OverlapCircle(boundingCircleCenterCir1, boundingCircleRad) != null) || (Physics2D.OverlapCircle(boundingCircleCenterCir2, boundingCircleRad) != null))
            {
                return false;
            }
        }
        //no collision
        return true;
    }

    private void CalculateBoundingBoxes(Vector2 input, Vector2 output)
    {
        float dir = Mathf.Sign(GetDirection().x);

        boundingBoxTopCornerStart = new Vector2(input.x + 0.05f * dir, input.y + 0.5f);
        boundingBoxBottomCornerStartEnd = new Vector2(input.x + 0.5f * dir, input.y - 0.5f);

        boundingBoxTopCornerEnd = new Vector2(output.x - 1.0f * dir, output.y + 0.5f);
        boundingBoxBottomCornerEnd = new Vector2(output.x - 0.1f * dir, output.y - 0.5f);

        //find middle of ramp
        //input end +- 0.5x, -0.24y output end +-1x, -0.24y
        Vector2 startCorner = new Vector2(input.x + 0.5f * dir, input.y - 0.24f);
        Vector2 endCorner = new Vector2(output.x - 1.0f * dir, output.y - 0.24f);
        startToEnd = endCorner - startCorner;
        Vector2 middlepoint = startCorner + startToEnd * 0.5f;
        //perpendicular vector to dir from middle point
        Vector2 middleToEndDir = startToEnd.normalized;
        Vector2 perp = new Vector2(middleToEndDir.y * dir, -middleToEndDir.x * dir);
        circleCenter = middlepoint + 0.125f * perp;

        if (startToEnd.magnitude < 3.0f)
        {
            //Debug
            boundingCircleCenterCir1 = circleCenter;
            boundingCircleRad = startToEnd.magnitude * 0.5f;
        }
        else
        {
            //Debug
            boundingCircleCenterCir1 = circleCenter + startToEnd * 0.25f;
            boundingCircleCenterCir2 = circleCenter - startToEnd * 0.25f;

            boundingCircleRad = startToEnd.magnitude * 0.25f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        CalculateBoundingBoxes(Input, Output);
        Gizmos.color = Color.red;
        //Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
        Gizmos.DrawWireSphere(boundingCircleCenterCir1, boundingCircleRad);
        if (!boundingCircleCenterCir2.Equals(Vector2.zero))
        {
            Gizmos.DrawWireSphere(boundingCircleCenterCir2, boundingCircleRad);
        }
        //draw ramp bounding boxes
        DrawRectangle(boundingBoxTopCornerStart, boundingBoxBottomCornerStartEnd, Color.red);
        DrawRectangle(boundingBoxTopCornerEnd, boundingBoxBottomCornerEnd, Color.red);
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
