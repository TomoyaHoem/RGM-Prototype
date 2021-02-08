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
        //copy io
        CopyIO(parent.GetComponent<SegmentPart>(), offset);
    }

    public override void MoveSegmentBy(Vector2 offset)
    {
        gameObject.transform.position += (Vector3)offset;

        MoveIO(offset);

        BallSpawnPos += offset;
    }

    public override void MirrorSegment()
    {
        MirrorIO();

        Transform parent = gameObject.transform.parent;
        GameObject mirrorAnchor = new GameObject();
        mirrorAnchor.transform.position = Input;
        gameObject.transform.parent = mirrorAnchor.transform;

        Quaternion rot = mirrorAnchor.transform.rotation;
        Quaternion newRot = new Quaternion(rot.x, rot.y + 180, rot.z, rot.w);
        mirrorAnchor.transform.rotation = newRot;

        gameObject.transform.parent = parent;
    
        Destroy(mirrorAnchor);
    }
}
