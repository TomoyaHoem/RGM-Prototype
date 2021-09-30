using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MachineTestManager : MonoBehaviour
{

    private bool isActive;

    private Scene mainScene;

    private Scene simulationScene;
    private PhysicsScene2D simulationScenePhysics;

    //number of tasks, #machines to be tested
    private int numTasks;
    //num of concurrently running tasks : machine tests
    private int curTasks;
    //num of concurrently segments being tested
    public int ConcurrentSegments { get; set; }
    //num of finished tasks, #machines tested
    private int finishedTasks;

    //simulate max 100 segments at a time
    private int maxParallelTasks;

    private void Awake()
    {
        System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

        mainScene = SceneManager.GetActiveScene();
        
        CreateSceneParameters sceneParam = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        simulationScene = SceneManager.CreateScene("Machine Simulation", sceneParam);
        simulationScenePhysics = simulationScene.GetPhysicsScene2D();
    }

    public IEnumerator TestMachinePopulation(List<GameObject> population)
    {
        maxParallelTasks = SettingsReader.Instance.MachineSettings.ParallelMachines;//40 / SettingsReader.Instance.MachineSettings.NumSegments > 0 ? 40 / SettingsReader.Instance.MachineSettings.NumSegments : 1;

        List<GameObject> testMachines = new List<GameObject>();
        GameObject populationHolder = population[0].transform.parent.gameObject;

        //seperate test machines from population
        foreach(GameObject machine in population)
        {
            machine.SetActive(false);
            if (machine.GetComponent<Machine>().Fitness == 0)
            {
                //add to list
                testMachines.Add(machine);
            }
            yield return null;
        }

        isActive = true;

        //serialize machine testing
        List<Task> taskList = new List<Task>();
        Task t;

        numTasks = testMachines.Count;
        finishedTasks = 0;
        curTasks = 0;
        //taskID to identify machine to test
        int taskID = 0;
        //count segments to limit 80 segments parallel
        ConcurrentSegments = 0;

        while(finishedTasks < numTasks)
        {
            if (curTasks >= maxParallelTasks || taskID >= numTasks || ConcurrentSegments > 80)
            {
                yield return null;
            } else
            {
                GameObject machine = testMachines[taskID];
                machine.SetActive(true);
                //remove parent to allow for move-to other scene
                machine.transform.parent = null;

                //move to other scene
                SceneManager.MoveGameObjectToScene(machine, simulationScene);

                machine.GetComponent<Machine>().InitSegPieces();
                MachineTester cur;
                if(machine.GetComponent<MachineTester>() != null)
                {
                    cur = machine.GetComponent<MachineTester>();
                } else
                {
                    cur = machine.AddComponent<MachineTester>();
                    cur.Manager = this;
                }

                t = new Task(cur.TestMachine(populationHolder));
                t.Finished += OnMachineTestFinished;
                taskList.Add(t);

                ConcurrentSegments += machine.GetComponent<Machine>().Segments.Count;
                curTasks++;
                taskID++;
            }
        }

        foreach(Task task in taskList)
        {
            task.Finished -= OnMachineTestFinished;
        }

        isActive = false;

        foreach (GameObject machine in population)
        {
            machine.SetActive(true);
            yield return null;
        }
    }

    private void OnMachineTestFinished(bool manual)
    {
        finishedTasks++;
        curTasks--;
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            Time.timeScale = SettingsReader.Instance.MachineSettings.SpeedUp;
            simulationScenePhysics.Simulate(Time.fixedDeltaTime);
        } else
        {
            Time.timeScale = 1;
        }
    }
}
