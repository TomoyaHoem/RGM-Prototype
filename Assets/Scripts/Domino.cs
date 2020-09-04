using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Domino
{

    [SerializeField]
    private GameObject _dominoPrefab;
    [SerializeField]
    private GameObject _floorPrefab;

    private int _floorLen;

    /*

    public Domino()
    {
        IO = new IO(Vector2.zero, Vector2.zero);
    }

    public override void Generate()
    {
        //beginning at input location move down 5/6th of a Domino Piece and place floor tiles until output location

        _floorLen = Mathf.Abs((int)(this.IO.Output.x - this.IO.Input.x));

        for (int i = 0; i < _floorLen; i++)
        {
            Vector2 spawnPos = new Vector2(IO.Input.x + i,
                    IO.Input.y - (5 / 6 * _dominoPrefab.GetComponent<SpriteRenderer>().bounds.size.y - _floorPrefab.GetComponent<SpriteRenderer>().bounds.size.y));

            //place smaller tile at beginning and end
            if (i == 0 || i == _floorLen - 1)
            {
                //place smaller tile so first and last domino are reachable easier
                Instantiate(_floorPrefab, spawnPos, Quaternion.identity);

                //place domino ontop of tile (small offset)
                spawnPos.y += _floorPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

                Instantiate(_dominoPrefab, spawnPos, Quaternion.identity);

            } else //place full tiles and dominos
            {
                Instantiate(_floorPrefab, spawnPos, Quaternion.identity);

                //place domino ontop of tile (small offset)
                spawnPos.y += _floorPrefab.GetComponent<SpriteRenderer>().bounds.size.y;

                Instantiate(_dominoPrefab, spawnPos, Quaternion.identity);
            }
        }
        //


    }

    public override void GenerateRandomEndPoint(IO prevIO)
    {
        //first segment -> direction irrelevant
        if(prevIO.Direction().x == 0 && prevIO.Direction().y == 0)
        {
            int randomDir = Random.Range(0, 2);
            int randomSize = Random.Range(4, 10);

            IO.Input = prevIO.Output;

            if (randomDir == 0) //right
            {
                IO.Output = new Vector2(randomSize, prevIO.Input.y);
            } else //left
            {
                IO.Output = new Vector2(-randomSize, prevIO.Input.y);
            }

        } else if (IO.Direction().x > 0) //right
        {

            int randomSize = Random.Range(4, 10);
            IO.Output = new Vector2(randomSize, prevIO.Input.y);

        } else if(IO.Direction().x < 0) //left
        {

            int randomSize = Random.Range(4, 10);
            IO.Output = new Vector2(-randomSize, prevIO.Input.y);

        } else //only vertical direction -> not suited for domino
        {
            //do not generate segment
            return;
        }
    }

    */
}
