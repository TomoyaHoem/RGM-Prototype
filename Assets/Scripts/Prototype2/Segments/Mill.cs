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
        MillPiece.GetComponent<SpriteRenderer>().color = Color.white;
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
        
        Quaternion rot = mirrorAnchor.transform.rotation;
        Quaternion newRot = new Quaternion(rot.x, rot.y + 180, rot.z, rot.w);
        mirrorAnchor.transform.rotation = newRot;

        gameObject.transform.parent = parent;
        Destroy(mirrorAnchor);
    }
}
