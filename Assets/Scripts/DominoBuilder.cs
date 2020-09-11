using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoBuilder : Segment
{

    public override Vector2 GenerateRandomOutput(Vector2 input, Vector2 directionPrev)
    {

        int randomSize = Random.Range(2, 6) * 2;

        //first segment -> direction irrelevant
        if (directionPrev.x == 0 && directionPrev.y == 0)
        {
            int randomDir = Random.Range(0, 2);

            if (randomDir == 0) //right
            {
                return new Vector2(randomSize, input.y);
            }
            else //left
            {
                return new Vector2(-randomSize, input.y);
            }
        }
        else if (directionPrev.x > 0) //right
        {
            return new Vector2(randomSize, input.y);
        }
        else if (directionPrev.x < 0) //left
        {
            return new Vector2(-randomSize, input.y);
        }
        else //only vertical direction -> not suited for domino
        {
            //do not generate segment
            Debug.Log("previous segment output not suited for domino");
            return input;
        }
    }

    public override void GenerateSegment(GameObject parent)
    {
        //beginning at input location move down 5/6th of a Domino Piece and place floor tiles until output location

        int _floorLen = Mathf.Abs((int)(output.x - input.x));

        for (int i = 1; i < _floorLen + 1; i++)
        {

            Vector2 spawnPos = new Vector2(0, 0);

            //right
            if (Direction.x > 0)
            {
                spawnPos = new Vector2(input.x + i,
                    input.y - (5 / 6 * 1.92f - 1));

            }
            else //left
            {
                spawnPos = new Vector2(input.x - i,
                    input.y - (5 / 6 * 1.92f - 1));
            }

            //tiles and dominos
            Instantiate(Resources.Load("Prefabs/Floor"), spawnPos, Quaternion.identity, parent.transform);

            //place domino ontop of tile (small offset)
            spawnPos.y += 1;

            Instantiate(Resources.Load("Prefabs/DominoPiece"), spawnPos, Quaternion.identity, parent.transform);
        }

    }
}
