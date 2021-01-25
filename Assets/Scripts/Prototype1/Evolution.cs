using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Evolution : MonoBehaviour
{

    [SerializeField]
    private List<GameObject> population, cameraGrid, bestParents, emptyMachines;
    [SerializeField]
    private int populationSize = 1;
    GameObject populationHolder, cameraHolder;

    private Camera main;

    [SerializeField]
    private int iterations = 10;
    private int generation = 0;
    [SerializeField]
    private bool autoSelect = false;

    [SerializeField]
    [Range(0, 1)]
    private float frequency = 1f;
    [SerializeField]
    [Range(0, 1)]
    private float lineraity = 1f;

    void Awake()
    {
        main = Camera.main;
        population = new List<GameObject>();
        cameraGrid = new List<GameObject>();
        bestParents = new List<GameObject>();
        emptyMachines = new List<GameObject>();
        populationHolder = new GameObject("Population");
        cameraHolder = new GameObject("Cameras");

        //call evolution
        StartCoroutine(EvolveMachines());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("FPS: " + (int)1.0f / Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchCameras();
        }
    }

    private void SwitchCameras()
    {
        if (Camera.main != null)
        {
            main.enabled = false;
            foreach (GameObject c in cameraGrid)
            {
                c.GetComponent<Camera>().enabled = true;
            }
        }
        else
        {
            foreach (GameObject c in cameraGrid)
            {
                c.GetComponent<Camera>().enabled = false;
            }
            main.enabled = true;
        }
    }

    IEnumerator EvolveMachines()
    {
        //initialize polpulation
        //arrange generated machines in grid (size based on population size)
        GenerateMachineGrid();

        //generate CameraGrid to display multiple machines
        //GenerateCameraGrid();

        //loop
        for (int i = 0; i < iterations; i++)
        {

            while (!Input.GetKeyDown(KeyCode.Space))
            {
                yield return null;
            }

            //Debug.Log("ITERATION NUMBER: " + i);
            //rate and select population
            if (autoSelect)
            {
                yield return StartCoroutine(RateAndSelectMachines());
            } else
            {
                yield return StartCoroutine(SelectMachines());
            }

            //breed best
            BreedBestParents();

            //mutate

            //count generation
            generation++;
        }

        Physics2D.autoSimulation = true;

        //machines will still generate after breeding -> wait
        yield return new WaitForSeconds(1.0f);

        //disable selection boxes
        foreach (GameObject machine in population)
        {
            if(machine.GetComponent<BoxCollider2D>() == null)
            {
                Debug.Log(machine.name + " missing collider");
                continue;
            }
            machine.GetComponent<BoxCollider2D>().enabled = false;
        }
    }

    private void GenerateMachineGrid()
    {
        int count = 0;

        int machineGridSize = (int)Mathf.Ceil(Mathf.Sqrt(populationSize));

        //fill up grid in x direction first
        for (int y = 0; y < machineGridSize; y++)
        {
            for (int x = 0; x < machineGridSize; x++)
            {
                //only fill grid until populationsize is reached
                if (count < populationSize)
                {
                    GenerateNewMachine(new Vector2(55 * x, 55 * y), count);
                }
                count++;
            }
        }
    }

    IEnumerator RateAndSelectMachines()
    {
        //rate population
        foreach(GameObject machineHolder in population)
        {
            RateMachine(machineHolder);
        }
        //sort population
        population = population.OrderByDescending(x => x.GetComponent<CoroutineMG>().Fitness).ToList();
        //add best half to parents and delete rest
        for(int i = 0; i < population.Count; i++)
        {
            if(i < population.Count / 2)
            {
                bestParents.Add(population[i]);
            } else
            {
                population[i].GetComponent<CoroutineMG>().DeleteMachine();
                emptyMachines.Add(population[i]);
            }
        }

        yield return null;
    }

    void RateMachine(GameObject machineHolder)
    {
        List<Segment> machine = machineHolder.GetComponent<CoroutineMG>().machine;

        //calculate various fitness values
        machineHolder.GetComponent<CoroutineMG>().Fitness = SegmentFrequency(machine) + SegmentLinearity(machine);
    }

    float SegmentFrequency(List<Segment> machine)
    {
        float result = 0f;

        float num = machine.Count;

        float bCount = 0, dCount = 0, mCount = 0;

        //count segments
        foreach(Segment seg in machine)
        {
            if(seg is DominoBuilder)
            {
                dCount++;
            }
            if (seg is BallTrack)
            {
                bCount++;
            }
            if (seg is MillBuilder)
            {
                mCount++;
            }
        }

        //calulate frequencies
        float bFreq = bCount == 0 ? 0 : (num / bCount) /3;
        float dFreq = dCount == 0 ? 0 : (num / dCount) /3;
        float mFreq = mCount == 0 ? 0 : (num / mCount) /3;

        //normalization value (#diff seg)
        float perfectScore = num / 3;

        result = bFreq + dFreq + mFreq;

        result /= perfectScore;

        return result * frequency;
    }

    float SegmentLinearity(List<Segment> machine)
    {
        float result = 0f;
        float maxCount = 0;
        float currCount = 0;
        float num = machine.Count;
        Segment prev = machine[0];

        //count most consecutive segments
        for(int i = 1; i < machine.Count; i++)
        {
            if(machine[i].GetType() == prev.GetType())
            {
                currCount++;
            } else
            {
                currCount = 1;
            }
            if(currCount > maxCount)
            {
                maxCount = currCount;
            }
            prev = machine[i];
        }

        result = (num - maxCount) / (num - 1);

        return result * lineraity;
    }

    IEnumerator SelectMachines()
    {
        foreach (GameObject machine in population)
        {
            machine.GetComponent<CoroutineMG>().selectEvent += OnMachineSelected;
        }

        while (bestParents.Count < populationSize/2)
        {
            //Debug.Log("Select: " + (8 - bestParents.Count) + " more Machines to continue");
            yield return null;
        }

        //unsubscribe events
        foreach (GameObject machine in population)
        {
            machine.GetComponent<CoroutineMG>().selectEvent -= OnMachineSelected;
            //destroy all machines that were not selected
            if (!machine.GetComponent<CoroutineMG>().IsSelected)
            {
                //destroy machine
                machine.GetComponent<CoroutineMG>().DeleteMachine();
                emptyMachines.Add(machine);
            }
        }
    }

    public void OnMachineSelected(GameObject machine)
    {
        //Debug.Log("Adding: " + machine.name + " to parents.");
        if (bestParents.Contains(machine))
        {
            bestParents.Remove(machine);
        }else
        {
            bestParents.Add(machine);
        }
    }

    void BreedBestParents()
    {
        //find 2 empty machines for each pair of parents
        for (int i = 0; i < bestParents.Count; i += 2)
        {
            //crossover with parents (flipped)
            //parent1 x parent2
            Crossover(bestParents[i], bestParents[i + 1], emptyMachines[i]);
            //parent2 x parent1
            Crossover(bestParents[i + 1], bestParents[i], emptyMachines[i + 1]);
        }
        //unselect all machines, clear best parents & empty
        foreach (GameObject machine in bestParents)
        {
            if (!autoSelect)
            {
                machine.GetComponent<CoroutineMG>().SwitchSelect();
            }
        }
        bestParents.Clear();
        emptyMachines.Clear();
    }

    void Crossover(GameObject parent1, GameObject parent2, GameObject emptyMachine)
    {
        List<Segment> parentMachine1 = parent1.GetComponent<CoroutineMG>().machine;
        List<Segment> parentMachine2 = parent2.GetComponent<CoroutineMG>().machine;

        int count = 0;
        int middle = (parentMachine1.Count / 2)-1;
        int index = middle;

        //disable box collider during crossover
        parent1.GetComponent<BoxCollider2D>().enabled = false;

        //loop until find
        do
        {
            //split index -> count/2 + 0, 1, -1, 2, -2, ...
            index = count % 2 == 0 ? middle + (count / 2) * (-1) : middle + (count / 2) + 1;
            //check wether segments at split are compatible
            if(CheckSegmentCompability(parentMachine1[index], parentMachine2[index+1])){
                //if so, disable remainder of first parent 
                //Debug.Log("found suitable at: " + index);
                for(int i = index; i < parentMachine1.Count; i++)
                {
                    parentMachine1[i].gameObject.SetActive(false);
                }
                //check if remainder of second parents fits onto split first parent
                if(CheckCollision(parentMachine2.GetRange(index+1, parentMachine2.Count-index-1), parentMachine1[index].Output - parentMachine2[index+1].Input))
                {
                    //Debug.Log("found fitting crossover");
                    for (int i = index; i < parentMachine1.Count; i++)
                    {
                        parentMachine1[i].gameObject.SetActive(true);
                    }
                    break;
                }
                //reenable remainder of first parent if split at index is invalid crossover
                for (int i = index; i < parentMachine1.Count; i++)
                {
                    parentMachine1[i].gameObject.SetActive(true);
                }
            }
        
            count++;
  
        } while (count < parentMachine1.Count-1);

        if(count >= parentMachine1.Count - 1)
        {
            Debug.Log("could not crossover");
            //save position and delete old machine
            Vector2 machinePos = emptyMachine.transform.position;
            Destroy(emptyMachine);
            population.Remove(emptyMachine);

            GenerateNewMachine(machinePos, -1);
        } else
        {
            //do crossover
            CombineMachineParts(index, parent1, parent2, emptyMachine);

            //enable select box & change name
            emptyMachine.GetComponent<BoxCollider2D>().enabled = true;
            //emptyMachine.name = "Child gen: " + generation;
            emptyMachine.name = parent1.name + parent2.name;
        }

        //reenable box collider
        parent1.GetComponent<BoxCollider2D>().enabled = true;
    }

    bool CheckSegmentCompability(Segment seg1, Segment seg2)
    {
        Vector2 dir1 = new Vector2(Mathf.Sign(seg1.GetDirection().x), Mathf.Sign(seg1.GetDirection().y));
        Vector2 dir2 = new Vector2(Mathf.Sign(seg2.GetDirection().x), Mathf.Sign(seg2.GetDirection().y));

        if (seg1 is MillBuilder)
        {
            if (seg2 is MillBuilder)
            {
                //both segments Mills -> check wether vertical direction identical and horizontal direction opposite
                if ((dir1.x != dir2.x) && (dir1.y == dir2.y))
                {
                    return true;
                }
                return false;
            }
            else
            {
                //only first segment Mill -> check wether horizontal direction same
                if (dir1.x == dir2.x)
                {
                    return true;
                }
                return false;
            }
        }
        else
        {
            if (seg2 is MillBuilder)
            {
                //only second segment Mill -> check wether hoizontal direction opposite
                if (dir1.x != dir2.x)
                {
                    return true;
                }
                return false;
            }
            else
            {
                //both segments non Mill -> check wether horizontal direction equal
                if (dir1.x == dir2.x)
                {
                    return true;
                }
                return false;
            }
        }
    }

    bool CheckCollision(List<Segment> machinePart, Vector2 offset)
    {
        foreach (Segment seg in machinePart)
        {
            if (!seg.CheckEnoughRoom(seg.Input + offset, seg.Output + offset))
            {
                return false;
            }
        }
        return true;
    }

    void CombineMachineParts(int index, GameObject parent1, GameObject parent2, GameObject empty)
    {
        GameObject copy;

        List<Segment> parentMachine1 = parent1.GetComponent<CoroutineMG>().machine;
        List<Segment> parentMachine2 = parent2.GetComponent<CoroutineMG>().machine;

        Vector3 offset1 = empty.transform.position - parent1.transform.position;
        Vector3 offset2 = (empty.transform.position + ((Vector3)parentMachine1[index].Output - parent1.transform.position)) - (Vector3)parentMachine2[index+1].Input;

        //copy auto start of first parent into empty machine
        copy = Instantiate(parent1.GetComponent<CoroutineMG>().AutoStart, (parent1.GetComponent<CoroutineMG>().AutoStart.transform.position + offset1), Quaternion.identity);
        copy.transform.parent = empty.transform;
        empty.GetComponent<CoroutineMG>().AutoStart = copy;
        copy.GetComponent<AutoStart>().PistonDirection = parent1.GetComponent<CoroutineMG>().AutoStart.GetComponent<AutoStart>().PistonDirection;

        //copy first parents segments up until index, then second parents segments
        for (int i = 0; i < parentMachine1.Count; i++)
        {
            if(i <= index)
            {
                //copy segments 
                copy = Instantiate(parentMachine1[i].gameObject, (parentMachine1[i].gameObject.transform.position + offset1), parentMachine1[i].gameObject.transform.rotation);
                copy.transform.parent = empty.transform;
                copy.name = parentMachine1[i].name;
                //change in and output
                copy.GetComponent<Segment>().Input += (Vector2)offset1;
                copy.GetComponent<Segment>().Output += (Vector2)offset1;
                if(copy.GetComponent<Segment>() is MillBuilder)
                {
                    copy.GetComponent<MillBuilder>().dirChange = parentMachine1[i].GetDirection();
                }
            } else
            {
                //copy segments
                copy = Instantiate(parentMachine2[i].gameObject, (parentMachine2[i].gameObject.transform.position + offset2), parentMachine2[i].gameObject.transform.rotation);
                copy.transform.parent = empty.transform;
                copy.name = parentMachine2[i].name;
                //change in and output
                copy.GetComponent<Segment>().Input += (Vector2)offset2;
                copy.GetComponent<Segment>().Output += (Vector2)offset2;
                if (copy.GetComponent<Segment>() is MillBuilder)
                {
                    copy.GetComponent<MillBuilder>().dirChange = parentMachine2[i].GetDirection();
                }
            }
            empty.GetComponent<CoroutineMG>().machine.Add(copy.GetComponent<Segment>());
        }
    }

    private void GenerateCameraGrid()
    {
        int count = 0;

        int cameraGridSize = Mathf.Min((int)Mathf.Floor(Mathf.Sqrt(populationSize)), 4);

        for (int y = 0; y < cameraGridSize; y++)
        {
            for (int x = 0; x < cameraGridSize; x++)
            {
                cameraGrid.Add(GenerateCamera(population[count], y, x, cameraGridSize));
                count++;
            }
        }
    }

    private GameObject GenerateCamera(GameObject machine, int y, int x, int cameraGridSize)
    {
        GameObject machineCamera = new GameObject("Camera: " + machine.name);
        //set parent
        machineCamera.transform.parent = cameraHolder.transform;
        //add camera
        machineCamera.AddComponent<Camera>();
        //turn camera off
        machineCamera.GetComponent<Camera>().enabled = false;
        //set camera parameters
        machineCamera.GetComponent<Camera>().transform.position = new Vector3(machine.transform.position.x, machine.transform.position.y, machine.transform.position.z - 10f);
        machineCamera.GetComponent<Camera>().orthographic = true;
        machineCamera.GetComponent<Camera>().orthographicSize = machine.GetComponent<CoroutineMG>().AreaSize;
        //normalize viewport for gridview
        machineCamera.GetComponent<Camera>().rect = new Rect(x * 1.0f / cameraGridSize, y * 1.0f / cameraGridSize, 1.0f / cameraGridSize, 1.0f / cameraGridSize);
        //add controls
        //machineCamera.AddComponent<CameraController>();

        return machineCamera;
    }

    void GenerateNewMachine(Vector2 position, int count)
    {
        GameObject machine = new GameObject("Machine " + count);
        machine.transform.position = position;
        //parent object for clean hierarchy
        machine.transform.parent = populationHolder.transform;
        //add generator script
        machine.AddComponent<CoroutineMG>();
        //add to population
        population.Add(machine);
    }
}
