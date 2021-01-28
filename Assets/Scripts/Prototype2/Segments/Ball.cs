using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : SegmentPart
{
    public Vector2 BallSpawnPos { get; set; }
    public Quaternion BallSpawnRotation { get; set; }

    public GameObject BallPiece { get; set; }

    private void Awake()
    {
        SegmentID = 1;
    }

    public override void ResetSegment()
    {
        if (BallPiece != null)
        {
            //reset color
            BallPiece.GetComponent<SpriteRenderer>().color = Color.white;
            //reset velocity
            BallPiece.SetActive(false);
            BallPiece.SetActive(true);

            //reset transform
            BallPiece.transform.position = BallSpawnPos;
            BallPiece.transform.rotation = BallSpawnRotation;

            //reset testing
            BallPiece.GetComponent<SegmentPiece>().ResetTest();
        }
    }

    public override void CopyProperties(GameObject seg, GameObject parent, Vector3 offset)
    {
        foreach (Transform child in seg.transform)
        {
            if (child.tag == "SegmentPiece")
            {
                BallPiece = child.gameObject;
                BallSpawnPos = child.position;
                BallSpawnRotation = child.rotation;
            }
        }
    }

    public override void MoveSegment(Vector2 offset)
    {
        gameObject.transform.position += (Vector3)offset;

        Input += offset;
        Output += offset;

        BallSpawnPos += offset;
    }
}
