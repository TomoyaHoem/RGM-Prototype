using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoBuilder : Segment
{

    [SerializeField]
    private GameObject _dominoPrefab;
    [SerializeField]
    private GameObject _floorPrefab;

    private Vector2 direction;

    private MachineGenerator mG;

    void Awake()
    {
        mG = FindObjectOfType<MachineGenerator>();

        if (mG.machine.Count == 0)
        {
            //empty List -> first segment, so direction does not matter
            input = mG.start;
            direction = Vector2.zero;
            output = GenerateRandomOutput(input);
        } else
        {
            //get output and direction of previous segment
            input = mG.machine[mG.machine.Count - 1].Output;
            direction = mG.machine[mG.machine.Count - 1].Output - mG.machine[mG.machine.Count - 1].Input;
            output = GenerateRandomOutput(input);
        }

        GenerateSegment();

    }

    public override Vector2 GenerateRandomOutput(Vector2 input)
    {

        int randomSize = Random.Range(2, 6) * 2;

        //first segment -> direction irrelevant
        if (direction.x == 0 && direction.y == 0)
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
        else if (direction.x > 0) //right
        {
            return new Vector2(randomSize, input.y);
        }
        else if (direction.x < 0) //left
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

    public override void GenerateSegment()
    {
        //beginning at input location move down 5/6th of a Domino Piece and place floor tiles until output location

        int _floorLen = Mathf.Abs((int)(output.x - input.x));

        for (int i = 0; i < _floorLen; i++)
        {
            Vector2 spawnPos = new Vector2(input.x + i,
                    input.y - (5 / 6 * 1.92f - 1));

            //place smaller tile at beginning and end
            if (i == 0 || i == _floorLen - 1)
            {
                //place smaller tile so first and last domino are reachable easier
                Instantiate(_floorPrefab, spawnPos, Quaternion.identity);

                //place domino ontop of tile (small offset)
                spawnPos.y += 1;

                Instantiate(_dominoPrefab, spawnPos, Quaternion.identity);

            }
            else //place full tiles and dominos
            {
                Instantiate(_floorPrefab, spawnPos, Quaternion.identity);

                //place domino ontop of tile (small offset)
                spawnPos.y += 1;

                Instantiate(_dominoPrefab, spawnPos, Quaternion.identity);
            }
        }

    }
}
