﻿using System.Collections;
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
                dominos[i].GetComponent<SpriteRenderer>().color = Color.white;
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
    }

    public override void MoveSegment(Vector2 offset)
    {
        gameObject.transform.position += (Vector3)offset;

        Input += offset;
        Output += offset;

        for (int i = 0; i < dominoSpawnPositions.Count; i++)
        {
            dominoSpawnPositions[i] += offset;
        }
        for (int i = 0; i < Heights.Length; i++)
        {
            Heights[i] += offset.y;
        }
    }
}
