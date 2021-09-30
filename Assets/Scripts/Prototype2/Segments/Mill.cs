using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mill : SegmentPart
{
    public Vector2 MillSpawnPos { get; set; }
    public Quaternion MillSpawnRotation { get; set; }

    public GameObject MillPiece { get; set; }

    //Mill can not calculate Direction through in-/output -> set through Logic with ID
    //public Vector2 Direction { get; set; }

    //public override Vector2 GetDirection()
    //{
    //    return Direction;
    //}

    public float Scale { get; set; }

    public override void ResetSegment()
    {
        //reset color
        MillPiece.GetComponent<SpriteRenderer>().color = new Color(99f / 255, 99f / 255, 99f / 255);
        //reset velocity
        MillPiece.SetActive(false);
        MillPiece.SetActive(true);

        //reset transform
        MillPiece.transform.position = MillSpawnPos;
        MillPiece.transform.rotation = MillSpawnRotation;

        //reset testing
        MillPiece.GetComponent<SegmentPiece>().ResetTest();
    }

    public override void CopyProperties(GameObject seg, GameObject parent, Vector3 offset)
    {
        foreach (Transform child in seg.transform)
        {
            if (child.tag == "SegmentPiece")
            {
                MillPiece = child.gameObject;
                MillSpawnPos = child.position;
                MillSpawnRotation = child.rotation;
            }
        }
        //copy Scale
        Scale = parent.GetComponent<Mill>().Scale;

        //copy io + offset
        CopyIO(parent.GetComponent<SegmentPart>(), offset);
    }

    public override void MoveSegmentBy(Vector2 offset)
    {
        gameObject.transform.position += (Vector3)offset;

        MoveIO(offset);

        MillSpawnPos += offset;
    }

    public override void MirrorSegment()
    {
        MirrorIO();

        Transform parent = gameObject.transform.parent;
        GameObject mirrorAnchor = new GameObject();
        mirrorAnchor.transform.position = Input;
        gameObject.transform.parent = mirrorAnchor.transform;

        /*
        Quaternion rot = mirrorAnchor.transform.rotation;
        Quaternion newRot = new Quaternion(rot.x, rot.y + 180, rot.z, rot.w);
        mirrorAnchor.transform.rotation = newRot;
        */
        mirrorAnchor.transform.localScale = new Vector3(mirrorAnchor.transform.localScale.x * (-1), 1, 1);

        MillSpawnPos = MillPiece.transform.position;
        MillSpawnRotation = MillPiece.transform.rotation;

        gameObject.transform.parent = parent;
        Destroy(mirrorAnchor);
    }
}
