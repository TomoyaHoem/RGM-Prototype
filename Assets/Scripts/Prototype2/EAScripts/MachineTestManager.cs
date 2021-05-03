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
    //num of finished tasks, #machines tested
    private int finishedTasks;

    //simulate max 100 segments at a time
    private int maxParallelTasks;

    private void Awake()
    {
        mainScene = SceneManager.GetActiveScene();

        maxParallelTasks = 40 / SettingsReader.Instance.MachineSettings.NumSegments > 0 ? 40 / SettingsReader.Instance.MachineSettings.NumSegments : 1;

        CreateSceneParameters sceneParam = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        simulationScene = SceneManager.CreateScene("Machine Simulation", sceneParam);
        simulationScenePhysics = simulationScene.GetPhysicsScene2D();
    }

    public IEnumerator TestMachinePopulation(List<GameObject> population)
    {
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

        while(finishedTasks < numTasks)
        {
            if (curTasks >= maxParallelTasks || taskID >= numTasks)
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
                MachineTester cur = (machine.GetComponent<MachineTester>() == null) ? machine.AddComponent<MachineTester>() : machine.GetComponent<MachineTester>();

                t = new Task(cur.TestMachine(populationHolder));
                t.Finished += OnMachineTestFinished;
                taskList.Add(t);

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
            Time.timeScale = 100;
            simulationScenePhysics.Simulate(Time.fixedDeltaTime);
        } else
        {
            Time.timeScale = 1;
        }
    }
}
