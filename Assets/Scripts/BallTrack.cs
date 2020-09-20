using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallTrack : Segment
{

    private Vector2 ballSpawnPos;
    private Quaternion ballSpawnRotation;

    private GameObject ball;

    public override Vector2 GenerateRandomOutput(Vector2 directionPrev)
    {
        int ranH = Random.Range(1,5);
        int ranL = Random.Range(ranH+2, ranH * 2);

        if(directionPrev.x > 0) //right
        {
            return new Vector2(ranL, -ranH);
        } else if(directionPrev.x < 0) //left
        {
            return new Vector2(-ranL, -ranH);
        } else { //no horizontal dir
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
        ball = Instantiate(Resources.Load("Prefabs/Ball"), ballSpawnPos, Quaternion.identity, parent.transform) as GameObject;
        ballSpawnRotation = ball.transform.rotation;

        //end
        Vector2 endSpawnPos = new Vector2(output.x - 0.5f * dir, output.y - 0.36f);
        GameObject end = Instantiate(Resources.Load("Prefabs/Platform"), endSpawnPos, Quaternion.identity, parent.transform) as GameObject;
        end.transform.localScale += new Vector3(1,0,0);

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
        Vector2 perp = new Vector2(middleToEndDir.y * dir, - middleToEndDir.x * dir);
        //Debug.DrawLine(middlepoint, middlepoint + perp, Color.blue, 300);

        Vector2 rampSpawn = middlepoint + 0.125f * perp;
        GameObject ramp = Instantiate(Resources.Load("Prefabs/Ramp"), rampSpawn, Quaternion.FromToRotation(Vector2.right, startToEndDir), parent.transform) as GameObject;
        //scale ramp to magnitude of vector between start and end
        ramp.transform.localScale = new Vector3((startToEndDir.magnitude / ramp.GetComponent<SpriteRenderer>().size.x), 1f, 1f);

    }

    public void Update()
    {
        //destroy ball after reaching end of ramp (to not trigger other parts further ahead)
        if (ball != null && ball.transform.position.y < output.y - 0.5f)
        {
            ball.SetActive(false);
        }
    }

    public override void ResetSegment()
    {
        if(ball != null)
        {
            //reset velocity
            ball.SetActive(false);
            ball.SetActive(true);

            //reset transform
            ball.transform.position = ballSpawnPos;
            ball.transform.rotation = ballSpawnRotation;
        }
    }

}
