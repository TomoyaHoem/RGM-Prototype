using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Domino : SegmentPart
{
    //initial dominopiece properties for reset
    public List<GameObject> dominos { get; set; }
    public List<Vector2> dominoSpawnPositions { get; set; }
    public List<Quaternion> dominoSpawnRotations { get; set; }

    public float[] Heights { get; set; }
    public float HeightOffset { get; set; }

    public float ScaleX { get; set; }
    public float ScaleY { get; set; }

    private void Awake()
    {
        SegmentID = 0;
        dominos = new List<GameObject>();
        dominoSpawnPositions = new List<Vector2>();
        dominoSpawnRotations = new List<Quaternion>();
    }

    public override void ResetSegment()
    {
        if (dominos.Count > 0)
        {
            for (int i = 0; i < dominos.Count; i++)
            {
                //reset color
                dominos[i].GetComponent<SpriteRenderer>().color = Color.gray;
                //reset velocity
                dominos[i].SetActive(false);
                dominos[i].SetActive(true);

                //reset transform
                dominos[i].transform.position = dominoSpawnPositions[i];
                dominos[i].transform.rotation = dominoSpawnRotations[i];

                //reset testing
                dominos[i].GetComponent<SegmentPiece>().ResetTest();
            }
        }
    }

    public override void CopyProperties(GameObject seg, GameObject parent, Vector3 offset)
    {
        float[] pH = parent.GetComponent<Domino>().Heights;
        Heights = new float[pH.Length];
        HeightOffset = parent.GetComponent<Domino>().HeightOffset;

        for(int i = 0; i < Heights.Length; i++)
        {
            Heights[i] = pH[i] + offset.y;
        }

        foreach (Transform child in seg.transform)
        {
            if(child.tag == "SegmentPiece")
            {
                dominos.Add(child.gameObject);
                dominoSpawnPositions.Add(child.position);
                dominoSpawnRotations.Add(child.rotation);
            }
        }
        //copy io
        CopyIO(parent.GetComponent<SegmentPart>(), offset);
    }

    public override void MoveSegmentBy(Vector2 offset)
    {
        gameObject.transform.position += (Vector3)offset;

        MoveIO(offset);

        for (int i = 0; i < dominoSpawnPositions.Count; i++)
        {
            dominoSpawnPositions[i] += offset;
        }
        for (int i = 0; i < Heights.Length; i++)
        {
            Heights[i] += offset.y;
        }
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

        int count = 0;
        foreach (Transform child in gameObject.transform)
        {
            if (child.tag == "SegmentPiece")
            {
                dominoSpawnPositions[count] = child.transform.position;
                dominoSpawnRotations[count] = child.transform.rotation;
                count++;
            }
        }

        gameObject.transform.parent = parent;
        Destroy(mirrorAnchor);
    }
}
