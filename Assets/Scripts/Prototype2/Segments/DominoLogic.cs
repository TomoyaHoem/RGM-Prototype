using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DominoLogic : SegmentLogic
{
    //reference to Domino data container
    public Domino Domino { get; set; }

    public override void GetDataReference()
    {
        Domino = gameObject.GetComponent<Domino>();
    }

    //save BoundingBoxData for DrawGizmo
    Vector2 boundingBoxTopCorner;
    Vector2 boundingBoxBottomCorner;

    public override Vector2 GenerateRandomOutput(Vector2 prevDir)
    {
        //randomize size of domino pieces
        Domino.ScaleX = Random.Range(0.5f, 1.5f);
        Domino.ScaleY = Random.Range(0.5f, 1.0f);

        //randomize lenght of domino segment
        int ranL = Random.Range(2, 6) * 2;
        //random seed for perlin noise
        int ranS = Random.Range(0, 10000);

        //list of random heights for each domino platform
        Domino.Heights = new float[ranL];

        //perlin noise parameters
        float scale = 10f;
        float amplitude = 5f;

        //init heights with perlin noise
        for (int i = ranS; i < ranS + ranL; i++)
        {
            Domino.Heights[i - ranS] = Mathf.PerlinNoise(i / scale, 0) * amplitude;
        }

        //save heightoffset of input
        Domino.HeightOffset = Domino.Input.y - Domino.Heights[0];

        if (prevDir.x > 0) //right
        {
            return new Vector2(ranL, Domino.Heights[Domino.Heights.Length - 1] + Domino.HeightOffset);
        }
        else if (prevDir.x < 0) //left
        {
            return new Vector2(-ranL, Domino.Heights[Domino.Heights.Length - 1] + Domino.HeightOffset);
        }
        else //only vertical direction -> not suited for domino
        {
            //do not generate segment
            Debug.Log("previous segment output not suited for domino");
            return Vector2.zero;
        }
    }

    public override void SetOutputDirection(Vector2 prevDir)
    {
        Domino.OutputDirection = new Vector2(prevDir.x, 0);
    }

    public override void GenerateSegment()
    {
        //get data reference
        if (Domino == null)
        {
            Domino = gameObject.GetComponent<Domino>();
        }

        //beginning at input location move down 5/6th of a Domino Piece and place floor tiles until output location

        int _floorLen = Mathf.Abs((int)(Domino.Output.x - Domino.Input.x));

        Vector2 spawnPos = Vector2.zero;

        Domino.Output = new Vector2(Domino.Output.x, Domino.Heights[Domino.Heights.Length-1] + Domino.HeightOffset);

        for (int i = 0; i < _floorLen; i++)
        {
            //Debug.Log(Domino.Input.y + " , " + heights[i] + " , " + heightOffset + " , " + Domino.Output.y);
            spawnPos = new Vector2(Domino.Input.x + 0.5f * RGMTest.Sign(Domino.InputDirection.x), Domino.Heights[i] + Domino.HeightOffset - (2f * Domino.ScaleY));

            if (Domino.InputDirection.x > 0)
            {
                spawnPos.x += i;
            }
            else
            {
                spawnPos.x -= i;
            }

            //tiles and dominos
            Instantiate(Resources.Load("Prefabs/Floor"), spawnPos, Quaternion.identity, gameObject.transform);

            //place domino ontop of tile (small offset)
            spawnPos.y += 1f;

            //tiny random domino pos offset for variation
            spawnPos.x += Random.Range(-0.1f, 0.1f);

            //if dominos are small, sometimes place two per tile
            int two = Random.Range(0, 3);

            if (Domino.ScaleX < 1.0f && (two == 1))
            {
                for (int j = 0; j < 2; j++)
                {
                    spawnPos.x = j == 0 ? spawnPos.x + 0.25f : spawnPos.x - 0.5f;
                    GameObject currDomino = Instantiate(Resources.Load("Prefabs/DominoPiece"), spawnPos, Quaternion.identity, gameObject.transform) as GameObject;
                    currDomino.transform.localScale = new Vector3(currDomino.transform.localScale.x * Domino.ScaleX, currDomino.transform.localScale.y * Domino.ScaleY, currDomino.transform.localScale.z);

                    //save initial transform
                    Domino.dominoSpawnPositions.Add(spawnPos);
                    Domino.dominoSpawnRotations.Add(currDomino.transform.rotation);
                    Domino.dominos.Add(currDomino);
                }
            } else
            {
                GameObject currDomino = Instantiate(Resources.Load("Prefabs/DominoPiece"), spawnPos, Quaternion.identity, gameObject.transform) as GameObject;
                currDomino.transform.localScale = new Vector3(currDomino.transform.localScale.x * Domino.ScaleX, currDomino.transform.localScale.y * Domino.ScaleY, currDomino.transform.localScale.z);

                //save initial transform
                Domino.dominoSpawnPositions.Add(spawnPos);
                Domino.dominoSpawnRotations.Add(currDomino.transform.rotation);
                Domino.dominos.Add(currDomino);
            }
        }
    }

    private void CalculateBoundingBoxes(Vector2 input, Vector2 output, Vector2 offset)
    {
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        for(int i = 0; i < Domino.Heights.Length; i++)
        {
            float yPos = Domino.Heights[i] + Domino.HeightOffset + offset.y;

            if (yPos < minY) minY = yPos;
            if (yPos > maxY) maxY = yPos;
        }

        float minX = input.x < output.x ? input.x : output.x;
        float maxX = input.x > output.x ? input.x : output.x;

        //Debug.Log("Bot: " + minX + ":" + minY + " , Top: " + maxX + ":" + maxY);

        boundingBoxBottomCorner = new Vector2(minX + 0.1f, minY - 2.5f);
        boundingBoxTopCorner = new Vector2(maxX - 0.1f, maxY + 0.9f);
    }

    private void CalcBoundingBox()
    {
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        for (int i = 0; i < Domino.Heights.Length; i++)
        {
            float yPos = Domino.Heights[i] + Domino.HeightOffset;

            if (yPos < minY) minY = yPos;
            if (yPos > maxY) maxY = yPos;
        }

        float minX = Domino.Input.x < Domino.Output.x ? Domino.Input.x : Domino.Output.x;
        float maxX = Domino.Input.x > Domino.Output.x ? Domino.Input.x : Domino.Output.x;

        boundingBoxBottomCorner = new Vector2(minX + 0.1f, minY - 2.5f);
        boundingBoxTopCorner = new Vector2(maxX - 0.1f, maxY + 0.9f);
    }

    public override bool CheckSegmentOverlap(Vector2 offset, string s, bool mode, bool mirrored, float duration)
    {
        CalcBoundingBox();

        //mirror if needed 
        if (mirrored)
        {
            //calculate signed distance between input and output
            float xDistanceSegment = Domino.Input.x - Domino.Output.x;
            //mirror bounding box by adding signed distance
            boundingBoxBottomCorner.x += xDistanceSegment;
            boundingBoxTopCorner.x += xDistanceSegment;
        }

        //add offset
        boundingBoxBottomCorner += offset;
        boundingBoxTopCorner += offset;

        //draw for testing purposes
        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.blue, duration);

        //calculate collider box
        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        //check for collision
        if ((Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner)) != null && (collider.name.Equals(s) == mode))
        {
            return false;
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        CalcBoundingBox();
        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.red, 0);
    }

    private void DrawRectangle(Vector2 topCorner, Vector2 bottomCorner, Color color, float duration)
    {
        Vector2 topOppositeCorner = new Vector2(bottomCorner.x, topCorner.y);
        Vector2 bottomOppositeCorner = new Vector2(topCorner.x, bottomCorner.y);

        Debug.DrawLine(topCorner, topOppositeCorner, color, duration);
        Debug.DrawLine(topOppositeCorner, bottomCorner, color, duration);
        Debug.DrawLine(bottomCorner, bottomOppositeCorner, color, duration);
        Debug.DrawLine(bottomOppositeCorner, topCorner, color, duration);
    }

    /*
    DEPRECATED
    public override bool CheckEnoughRoom(Vector2 input, Vector2 output)
    {
        CalculateBoundingBoxes(input, output, Vector2.zero);

        if (Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner) != null)
        {
            //Debug.Log(Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner));
            return false;
        }
        return true;
    }

    public override bool CheckEnoughRoom(Vector2 input, Vector2 output, Vector2 offset, string s, bool mode)
    {
        CalculateBoundingBoxes(input + offset, output + offset, offset);

        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        if (collider != null && (collider.name.Equals(s) == mode))
        {
            //Debug.Log(Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner));
            return false;
        }
        return true;
    }

    public override bool CheckEnoughRoomMirrored(Vector2 input, Vector2 output, Vector2 offset, string s, bool mode)
    {
        CalculateBoundingBoxes(input, output, offset);
        //Debug.Log(gameObject.name + boundingBoxTopCorner + boundingBoxBottomCorner);
        Collider2D collider = Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner);

        DrawRectangle(boundingBoxTopCorner, boundingBoxBottomCorner, Color.yellow, 100);

        if (Physics2D.OverlapArea(boundingBoxTopCorner, boundingBoxBottomCorner) != null && (collider.name.Equals(s) == mode))
        {
            return false;
        }
        return true;
    }
    */
}
