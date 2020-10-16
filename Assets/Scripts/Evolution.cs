using System;
using System.Collections;
using System.Collections.Generic;
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
            //Debug.Log("ITERATION NUMBER: " + i);
            //rate and select population
            yield return StartCoroutine(SelectMachines());

            //breed best
            BreedBestParents();

            //mutate
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
                    GameObject machine = new GameObject("Machine " + count);
                    machine.transform.position = new Vector2(55 * x, 55 * y);
                    //parent object for clean hierarchy
                    machine.transform.parent = populationHolder.transform;
                    //add generator script
                    machine.AddComponent<CoroutineMG>();
                    //add to population
                    population.Add(machine);
                }
                count++;
            }
        }
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
        bestParents.Add(machine);
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

            //unselect all machines, clear best parents & empty
            foreach (GameObject machine in population)
            {
                machine.GetComponent<CoroutineMG>().IsSelected = false;
            }
            bestParents.Clear();
            emptyMachines.Clear();
        }
    }

    void Crossover(GameObject parent1, GameObject parent2, GameObject emptyMachine)
    {
        List<Segment> parentMachine1 = parent1.GetComponent<CoroutineMG>().machine;
        List<Segment> parentMachine2 = parent2.GetComponent<CoroutineMG>().machine;

        int count = 0;
        int index = (parentMachine1.Count / 2)-1;

        //disable box collider during crossover
        parent1.GetComponent<BoxCollider2D>().enabled = false;

        //loop until find
        do
        {
            //split index -> count/2 + 0, 1, -1, 2, -2, ...
            index += count % 2 == 0 ? (count / 2) * (-1) : (count / 2) + 1;
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
  
        } while (count < parentMachine1.Count);

        //do crossover
        CombineMachineParts(index, parent1, parent2, emptyMachine);

        //reenable box collider
        parent1.GetComponent<BoxCollider2D>().enabled = false;

        //enable select box & change name
        emptyMachine.GetComponent<BoxCollider2D>().enabled = true;
        emptyMachine.name = parent1.name + parent2.name;
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
                //Debug.Log(seg.Input + offset + "-" + seg.Output + offset);
                return false;
            }
        }
        return true;
    }

    void CombineMachineParts(int index, GameObject parent1, GameObject parent2, GameObject empty)
    {
        GameObject copy;

        //Vector3 offset1 = empty.transform.position - parent1.transform.position;
        //Vector3 offset2 = empty.transform.position - parent2.transform.position;

        List<Segment> parentMachine1 = parent1.GetComponent<CoroutineMG>().machine;
        List<Segment> parentMachine2 = parent2.GetComponent<CoroutineMG>().machine;

        Vector3 offset1 = empty.transform.position - parent1.transform.position;
        Vector3 offset2 = (empty.transform.position + ((Vector3)parentMachine1[index].Output - parent1.transform.position)) - (Vector3)parentMachine2[index+1].Input;

        //copy auto start of first parent into empty machine
        copy = Instantiate(parent1.GetComponent<CoroutineMG>().AutoStart, (parent1.GetComponent<CoroutineMG>().AutoStart.transform.position + offset1), Quaternion.identity);
        copy.transform.parent = empty.transform;

        Transform children;

        //copy first parents segments up until index, then second parents segments
        for(int i = 0; i < parentMachine1.Count; i++)
        {
            GameObject segment = new GameObject("Segment " + i);
            segment.transform.parent = empty.transform;
            if(i <= index)
            {
                children = parentMachine1[i].gameObject.transform;
                foreach(Transform child in children)
                {
                    copy = Instantiate(child.gameObject, (child.position + offset1), child.rotation);
                    copy.transform.parent = segment.transform;
                }
            } else
            {
                children = parentMachine2[i].gameObject.transform;
                foreach (Transform child in children)
                {
                    copy = Instantiate(child.gameObject, (child.position + offset2), child.rotation);
                    copy.transform.parent = segment.transform;
                }
            }
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
}
