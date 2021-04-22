using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : SegmentPart
{
    public List<Vector2> CarPartSpawnPos { get; set; }
    public List<Quaternion> CarPartSpawnRotation { get; set; }

    public GameObject CarPiece { get; set; }

    public Vector2[] EvenPoints { get; set; }

    private void Awake()
    {
        SegmentID = 5;
        CarPartSpawnPos = new List<Vector2>();
        CarPartSpawnRotation = new List<Quaternion>();
    }

    public override void ResetSegment()
    {
        if (CarPiece != null)
        {
            //reset color
            foreach (Transform child in gameObject.transform.GetChild(1))
            {
                child.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            }

            //reset velocity 
            for (int i = 0; i < CarPiece.transform.childCount; i++)
            {
                CarPiece.transform.GetChild(i).gameObject.SetActive(false);
                CarPiece.transform.GetChild(i).gameObject.SetActive(true);
            }

            //reset transform
            for (int i = 0; i < CarPiece.transform.childCount; i++)
            {
                CarPiece.transform.GetChild(i).position = CarPartSpawnPos[i];
                CarPiece.transform.GetChild(i).rotation = CarPartSpawnRotation[i];
            }

            //reset testing
            CarPiece.transform.GetChild(0).GetComponent<SegmentPiece>().ResetTest();
        }
    }

    public override void CopyProperties(GameObject seg, GameObject parent, Vector3 offset)
    {
        Vector2[] pEvenPoints = parent.GetComponent<Car>().EvenPoints;
        EvenPoints = new Vector2[pEvenPoints.Length];

        for (int i = 0; i < EvenPoints.Length; i++)
        {
            EvenPoints[i] = pEvenPoints[i] + (Vector2)offset;
        }

        foreach (Transform child in seg.transform)
        {
            if (child.tag == "SegmentPiece")
            {
                CarPiece = child.gameObject;
                for (int i = 0; i < child.transform.childCount; i++)
                {
                    CarPartSpawnPos[i] = child.transform.GetChild(i).position;
                    CarPartSpawnRotation[i] = child.transform.GetChild(i).rotation;
                }
            }
        }
        //copy io
        CopyIO(parent.GetComponent<SegmentPart>(), offset);
    }

    public override void MoveSegmentBy(Vector2 offset)
    {
        gameObject.transform.position += (Vector3)offset;

        MoveIO(offset);

        for (int i = 0; i < CarPartSpawnPos.Count; i++)
        {
            CarPartSpawnPos[i] += offset;
        }

        for (int i = 0; i < EvenPoints.Length; i++)
        {
            EvenPoints[i] += offset;
        }
    }

    public override void MirrorSegment()
    {
        MirrorIO();

        MirrorEvenPoints();

        Transform parent = gameObject.transform.parent;
        GameObject mirrorAnchor = new GameObject();
        mirrorAnchor.transform.position = Input;
        gameObject.transform.parent = mirrorAnchor.transform;

        mirrorAnchor.transform.localScale = new Vector3(mirrorAnchor.transform.localScale.x * (-1), 1, 1);

        if (CarPiece != null)
        {
            for (int i = 0; i < CarPartSpawnPos.Count; i++)
            {
                CarPartSpawnPos[i] = CarPiece.transform.GetChild(i).position;
                CarPartSpawnRotation[i] = CarPiece.transform.GetChild(i).rotation;
            }
        }

        gameObject.transform.parent = parent;
        Destroy(mirrorAnchor);
    }

    private void MirrorEvenPoints()
    {
        float startPointX = EvenPoints[0].x;

        for (int i = 1; i < EvenPoints.Length; i++)
        {
            EvenPoints[i].x += (EvenPoints[i].x - startPointX) * -2;
        }
    }
}
