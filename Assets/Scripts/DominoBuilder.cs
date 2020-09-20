using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoBuilder : Segment
{

    private List<GameObject> dominos;
    private List<Vector2> dominoSpawnPositions;
    private List<Quaternion> dominoSpawnRotations;

    private GameObject currDomino;

    public override Vector2 GenerateRandomOutput(Vector2 directionPrev)
    {

        int randomSize = Random.Range(2, 6) * 2;

        if (directionPrev.x > 0) //right
        {
            return new Vector2(randomSize, 0);
        }
        else if (directionPrev.x < 0) //left
        {
            return new Vector2(-randomSize, 0);
        }
        else //only vertical direction -> not suited for domino
        {
            //do not generate segment
            Debug.Log("previous segment output not suited for domino");
            return Vector2.zero;
        }
    }

    public override void GenerateSegment(GameObject parent)
    {
        //beginning at input location move down 5/6th of a Domino Piece and place floor tiles until output location

        int _floorLen = Mathf.Abs((int)(output.x - input.x));

        Vector2 spawnPos = Vector2.zero;

        dominoSpawnPositions = new List<Vector2>();
        dominoSpawnRotations = new List<Quaternion>();
        dominos = new List<GameObject>();

        for (int i = 0; i < _floorLen; i++)
        {

            spawnPos = new Vector2(input.x + 0.5f * Mathf.Sign(GetDirection().x), input.y - 2f);

            if (GetDirection().x > 0)
            {
                spawnPos.x += i;
            } else
            {
                spawnPos.x -= i;
            }

            //tiles and dominos
            Instantiate(Resources.Load("Prefabs/Floor"), spawnPos, Quaternion.identity, parent.transform);

            //place domino ontop of tile (small offset)
            spawnPos.y += 1f;

            currDomino = Instantiate(Resources.Load("Prefabs/DominoPiece"), spawnPos, Quaternion.identity, parent.transform) as GameObject;

            //save initial transform

            dominoSpawnPositions.Add(spawnPos);
            dominoSpawnRotations.Add(currDomino.transform.rotation);
            dominos.Add(currDomino);
        }

    }

    public override void ResetSegment()
    {
        if(dominos.Count > 0)
        {
            for (int i = 0; i < dominos.Count; i++)
            {
                //reset velocity
                dominos[i].SetActive(false);
                dominos[i].SetActive(true);

                //reset transform
                dominos[i].transform.position = dominoSpawnPositions[i];
                dominos[i].transform.rotation = dominoSpawnRotations[i];
            }
        }
    }
}
