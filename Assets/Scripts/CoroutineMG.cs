using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CoroutineMG : MonoBehaviour
{

    public delegate void SelectionDelegate(GameObject machine);
    public event SelectionDelegate selectEvent;

    [SerializeField]
    private Vector2 start = new Vector2(0, 0);
    [SerializeField]
    private int numOfSegments = 4;
    [SerializeField]
    private Vector2 startDir = new Vector2(1, 0);

    [Range(0, 4)]
    [SerializeField]
    private float timeBetweenSegments = 0f;
    [SerializeField]
    bool generateOnKeyInput = false;

    [SerializeField]
    private int stuckCount;
    [SerializeField]
    private int backtrackAmount = 5;

    [SerializeField]
    private int areaSize = 25;

    private GameObject autoStart;
    public int AreaSize { get => areaSize; private set => areaSize = value; }
    public bool IsSelected { get => isSelected; set => isSelected = value; }
    public Vector2 Start { get => start; set => start = value; }
    public GameObject AutoStart { get => autoStart; set => autoStart = value; }

    public List<Segment> machine = new List<Segment>();

    bool isSelected = false;

    SpriteRenderer spriteRenderer;

    //stores which segments have been tried at position i, to prevent generation getting stuck on same paths
    [SerializeField]
    private List<List<string>> triedSegments = new List<List<string>>();

    private void Awake()
    {
        //bounding box ignores collision
        gameObject.layer = 8;

        Start = new Vector2(transform.position.x, transform.position.y);
        startDir.x = UnityEngine.Random.Range(0, 2) * 2 - 1;
        //spawn bounding area
        SpawnWalls();
        //autoStart
        SpawnAutoStart();
        //build Machine
        StartCoroutine(BuildMachine());
        //add selection sprite
        AddSelectionSprite();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {

            Debug.Log("Resetting Level");

            foreach (Segment segment in machine)
            {
                segment.ResetSegment();
            }
        }
    }

    void OnMouseDown()
    {
        if (IsSelected)
        {
            Physics2D.autoSimulation = false;
            spriteRenderer.enabled = false;
            IsSelected = false;
        } else
        {
            Physics2D.autoSimulation = true;
            spriteRenderer.enabled = true;
            IsSelected = true;
            //broadcast selection
            selectEvent?.Invoke(gameObject);
        }
    }

    void AddSelectionSprite()
    {
        GameObject sprite = new GameObject("Sprite");
        sprite.transform.position = Start;
        sprite.transform.parent = gameObject.transform;

        sprite.AddComponent<SpriteRenderer>();
        spriteRenderer = sprite.GetComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;

        spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/select");
        spriteRenderer.color = new Color(1, 0, 0, 0.4f);
        spriteRenderer.sortingOrder = -1;

        spriteRenderer.transform.localScale = new Vector2(areaSize * 2, areaSize * 2);
    }

    void SpawnWalls()
    {
        GameObject walls = new GameObject("Walls");
        walls.transform.position = Start;
        walls.transform.parent = gameObject.transform;

        //north wall
        GameObject wallN = new GameObject("Wall North");
        wallN.transform.parent = walls.transform;
        wallN.transform.position = new Vector2(Start.x, Start.y + AreaSize);
        wallN.AddComponent<BoxCollider2D>().transform.localScale = new Vector2(AreaSize * 2, 1);
        //south wall
        GameObject wallS = new GameObject("Wall South");
        wallS.transform.parent = walls.transform;
        wallS.transform.position = new Vector2(Start.x, Start.y - AreaSize);
        wallS.AddComponent<BoxCollider2D>().transform.localScale = new Vector2(AreaSize * 2, 1);
        //west wall
        GameObject wallW = new GameObject("Wall West");
        wallW.transform.parent = walls.transform;
        wallW.transform.position = new Vector2(Start.x + AreaSize, Start.y);
        wallW.AddComponent<BoxCollider2D>().transform.localScale = new Vector2(1, AreaSize * 2);
        //east wall
        GameObject wallE = new GameObject("Wall East");
        wallE.transform.parent = walls.transform;
        wallE.transform.position = new Vector2(Start.x - AreaSize, Start.y);
        wallE.AddComponent<BoxCollider2D>().transform.localScale = new Vector2(1, AreaSize * 2);
    }

    void AddSelectionArea()
    {
        //add BoxCollider2D 2 * size of area to detect when user selects machine
        gameObject.AddComponent<BoxCollider2D>();
        BoxCollider2D machineColldier = gameObject.GetComponent<BoxCollider2D>();
        machineColldier.size = new Vector2(areaSize * 2, areaSize * 2);
    }

    void SpawnAutoStart()
    {
        Vector2 spawnPos = Vector2.zero;

        if (startDir.x > 0) //place to the left of start
        {
            spawnPos = new Vector2(Start.x - 0.75f, Start.y);
        }
        else //place to the right of start
        {
            spawnPos = new Vector2(Start.x + 0.75f, Start.y);
        }

        //instantiate
        AutoStart = Instantiate(Resources.Load("Prefabs/AutoStart"), spawnPos, Quaternion.identity, gameObject.transform) as GameObject;
        AutoStart.GetComponent<AutoStart>().PistonDirection = startDir;
    }

    IEnumerator BuildMachine()
    {
        Physics2D.autoSimulation = false;

        for (int i = 0; i < numOfSegments; i++)
        {
            if(stuckCount > 200)
            {
                break;
            }
            yield return StartCoroutine(BuildRandomSegment(i));
        }
        //add selection boundingbox
        AddSelectionArea();
        //add end
    }

    IEnumerator BuildRandomSegment(int segmentNum)
    {
        GameObject segmentHolder = new GameObject("Segment " + segmentNum);

        //check if list already exists (in case of retry)
        if (triedSegments.Count() == segmentNum)
        {
            triedSegments.Add(new List<string>());
        }

        //first segment BallTrack
        if (segmentNum == 0)
        {
            segmentHolder.AddComponent<BallTrack>();

            //set input as start
            segmentHolder.GetComponent<Segment>().Input = Start;
            //generate random output for current segment based on start pos
            segmentHolder.GetComponent<Segment>().Output = Start + segmentHolder.GetComponent<Segment>().GenerateRandomOutput(startDir);

            if (!segmentHolder.GetComponent<Segment>().CheckEnoughRoom(Start, segmentHolder.GetComponent<Segment>().Output))
            {
                yield break;
            }
        }
        else //other Segments try to find random Fitting Segment
        {
            int r = UnityEngine.Random.Range(0, 3);

            if (!FindFittingSegment(segmentHolder, r, 0, segmentNum))
            {
                stuckCount++;
                //unable to find fitting Segment -> add to List, destroy previous and current and rebuild, continue
                //add previous segmenttype to list
                //Debug.Log(IdentifySegment(machine[machine.Count - 1].gameObject));
                triedSegments[segmentNum-1].Add(IdentifySegment(machine[machine.Count - 1].gameObject));
                //destroy previous
                Destroy(machine[machine.Count - 1].gameObject);
                //remove previous 
                machine.RemoveAt(machine.Count - 1);
                //destroy current
                Destroy(segmentHolder);
                //do a new call for the destroyed object, build a new segment and try again for current segment
                yield return StartCoroutine(BuildRandomSegment(segmentNum - 1));
                yield return StartCoroutine(BuildRandomSegment(segmentNum));
                //go to next iteration
                yield break;
            }

            //clear after unstuck will affect how much will be deleted when stuck
            if(triedSegments.Count > segmentNum + backtrackAmount)
            {
                triedSegments.RemoveRange(segmentNum+1, triedSegments.Count - (segmentNum+1));
            }
        }

        //get last added component since wrong components are still attached at this point in execution
        segmentHolder.GetComponents<Segment>().Last().GenerateSegment(segmentHolder);

        machine.Add(segmentHolder.GetComponents<Segment>().Last());

        segmentHolder.transform.parent = gameObject.transform;

        if (generateOnKeyInput)
        {
            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }
        }

        yield return new WaitForSeconds(timeBetweenSegments);
    }

    //check Type of Segment and return identifying String
    string IdentifySegment(GameObject prevSegment)
    {
        Segment prev = prevSegment.GetComponent<Segment>();

        if (typeof(DominoBuilder).IsInstanceOfType(prev))
        {
            return "Domino";
        }
        else if (typeof(BallTrack).IsInstanceOfType(prev))
        {
            return "Ball";
        }
        else if (typeof(MillBuilder).IsInstanceOfType(prev))
        {
            if (prev.GetDirection().y > 0)
            {
                return "MillUp";
            }
            else
            {
                return "MillDown";
            }
        }

        //should not happen
        Debug.Log("Could not identify Segment");
        return null;
    }

    bool FindFittingSegment(GameObject segmentHolder, int r, int iteration, int segmentNum)
    {
        if (iteration > 2)
        {
            //cant find fitting segment
            return false;
        }

        if (r == 0)
        {
            segmentHolder.AddComponent<DominoBuilder>();

            SetInOutput(segmentHolder);

            //check if segment has been considered at this point yet and if not, wether there is enough room for it
            if (triedSegments[segmentNum].Contains("Domino") || !segmentHolder.GetComponent<DominoBuilder>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
            {
                Destroy(segmentHolder.GetComponent<DominoBuilder>());
                //try to find other Segment (>BallTrack)
                return FindFittingSegment(segmentHolder, 1, ++iteration, segmentNum);
            }
            return true;
        }
        else if (r == 1)
        {
            segmentHolder.AddComponent<BallTrack>();

            SetInOutput(segmentHolder);

            if (triedSegments[segmentNum].Contains("Ball") || !segmentHolder.GetComponent<BallTrack>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
            {
                Destroy(segmentHolder.GetComponent<BallTrack>());
                //try to find other Segment (>Mill)
                return FindFittingSegment(segmentHolder, 2, ++iteration, segmentNum);
            }
            return true;
        }
        else
        {
            segmentHolder.AddComponent<MillBuilder>();

            if (triedSegments[segmentNum].Contains("MillUp") && triedSegments[segmentNum].Contains("MillDown"))
            {
                Destroy(segmentHolder.GetComponent<MillBuilder>());
                //try to find other Segment (>Domino)
                return FindFittingSegment(segmentHolder, 0, ++iteration, segmentNum);
            }
            else if (triedSegments[segmentNum].Contains("MillUp") || triedSegments[segmentNum].Contains("MillDown"))
            {
                if (triedSegments[segmentNum].Contains("MillUp"))
                {
                    //try mill down
                    segmentHolder.GetComponent<MillBuilder>().DirVert = -1;
                    SetInOutput(segmentHolder);
                    if (!segmentHolder.GetComponent<MillBuilder>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
                    {
                        Destroy(segmentHolder.GetComponent<MillBuilder>());
                        //try to find other Segment (>Domino)
                        return FindFittingSegment(segmentHolder, 0, ++iteration, segmentNum);
                    }
                    return true;
                }
                else
                {
                    //try mill up
                    segmentHolder.GetComponent<MillBuilder>().DirVert = 1;
                    SetInOutput(segmentHolder);
                    if (!segmentHolder.GetComponent<MillBuilder>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
                    {
                        Destroy(segmentHolder.GetComponent<MillBuilder>());
                        //try to find other Segment (>Domino)
                        return FindFittingSegment(segmentHolder, 0, ++iteration, segmentNum);
                    }
                    return true;
                }
            }
            else
            {
                //vertical direciton random
                segmentHolder.GetComponent<MillBuilder>().DirVert = UnityEngine.Random.Range(0, 2) * 2 - 1;
                //check if previous segment was mill, if so, set vertical direction accordingly
                if(IdentifySegment(machine[machine.Count-1].gameObject) == "MillUp")
                {
                    segmentHolder.GetComponent<MillBuilder>().DirVert = 1;
                } else if (IdentifySegment(machine[machine.Count - 1].gameObject) == "MillDown")
                {
                    segmentHolder.GetComponent<MillBuilder>().DirVert = -1;
                }
                SetInOutput(segmentHolder);
                if (!segmentHolder.GetComponent<MillBuilder>().CheckEnoughRoom(segmentHolder.GetComponents<Segment>().Last().Input, segmentHolder.GetComponents<Segment>().Last().Output))
                {
                    Destroy(segmentHolder.GetComponent<MillBuilder>());
                    //try to find other Segment (>Domino)
                    return FindFittingSegment(segmentHolder, 0, ++iteration, segmentNum);
                }
                return true;
            }
        }
    }

    void SetInOutput(GameObject segmentHolder)
    {
        Segment seg = segmentHolder.GetComponents<Segment>().Last();
        //set input to previous output location
        seg.Input = machine[machine.Count - 1].GetComponent<Segment>().Output;
        //generate random output for current segment based on previous direction
        seg.Output = seg.Input + seg.GenerateRandomOutput(machine[machine.Count - 1].GetComponent<Segment>().GetDirection());
    }

    public void DeleteMachine()
    {
        Destroy(AutoStart);
        foreach(Segment seg in machine)
        {
            Destroy(seg.gameObject);
        }
        machine.Clear();
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }
}
