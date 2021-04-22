using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : SegmentPart
{
    public float Scale { get; set; }

    public Vector2 HammerSpawnPos { get; set; }
    public Quaternion HammerSpawnRotation { get; set; }

    public GameObject HammerPiece { get; set; }

    private void Awake()
    {
        SegmentID = 4;
    }

    public override void ResetSegment()
    {
        //reset color
        foreach (Transform child in gameObject.transform.GetChild(0))
        {
            child.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        }

        //reset velocity and rigidbody type
        HammerPiece.SetActive(false);
        HammerPiece.SetActive(true);
        HammerPiece.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        //reset transform
        HammerPiece.transform.position = HammerSpawnPos;
        HammerPiece.transform.rotation = HammerSpawnRotation;

        //reset testing
        HammerPiece.GetComponent<SegmentPiece>().ResetTest();

        //reset switch
        gameObject.transform.GetChild(1).transform.GetChild(1).GetComponent<HammerSwitch>().IsActive = true;
    }

    public override void CopyProperties(GameObject seg, GameObject parent, Vector3 offset)
    {
        foreach (Transform child in seg.transform)
        {
            if (child.tag == "SegmentPiece")
            {
                HammerPiece = child.gameObject;
                HammerSpawnPos = child.position;
                HammerSpawnRotation = child.rotation;
            }
        }
        //copy Scale
        Scale = parent.GetComponent<Hammer>().Scale;

        //copy io + offset
        CopyIO(parent.GetComponent<SegmentPart>(), offset);
    }

    public override void MoveSegmentBy(Vector2 offset)
    {
        gameObject.transform.position += (Vector3)offset;

        MoveIO(offset);

        HammerSpawnPos += offset;
    }

    public override void MirrorSegment()
    {
        MirrorIO();

        Transform parent = gameObject.transform.parent;
        GameObject mirrorAnchor = new GameObject();
        mirrorAnchor.transform.position = Input;
        gameObject.transform.parent = mirrorAnchor.transform;

        mirrorAnchor.transform.localScale = new Vector3(mirrorAnchor.transform.localScale.x * (-1), 1, 1);

        HammerSpawnPos = HammerPiece.transform.position;
        HammerSpawnRotation = HammerPiece.transform.rotation;

        gameObject.transform.parent = parent;
        Destroy(mirrorAnchor);
    }
}
