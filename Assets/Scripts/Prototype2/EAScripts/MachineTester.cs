using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MachineTester : MonoBehaviour
{
    public List<GameObject> TestSequence { get; set; }

    Scene mainScene;

    MachineSettings settings;

    private void Awake()
    {
        mainScene = SceneManager.GetActiveScene();
    }

    private void Start()
    {
        settings = SettingsReader.Instance.MachineSettings;
    }

    public IEnumerator TestMachine(GameObject populationHolder)
    {
        //subscribe all
        foreach(GameObject segment in gameObject.GetComponent<Machine>().Segments)
        {
            foreach(Transform child in segment.transform)
            {
                if(child.tag == "SegmentPiece")
                {
                    child.GetComponent<SegmentPiece>().SegmentPieceCollisionEvent += OnSegmentPieceCollision;
                }
                foreach (Transform c in child.transform)
                {
                    if (c.tag == "SegmentPiece")
                    {
                        c.GetComponent<SegmentPiece>().SegmentPieceCollisionEvent += OnSegmentPieceCollision;
                    }
                }
            }
        }

        //start auto start
        GameObject autoStart = gameObject.GetComponent<Machine>().AutoStart;
        Task t = new Task(autoStart.GetComponent<AutoStart>().MovePiston(autoStart.transform.GetChild(0).transform));

        //loop test timer or testsequence complete
        TestSequence = new List<GameObject>();
        TestSequence.Clear();
        float timer = 0f;
        while (timer < SettingsReader.Instance.MachineSettings.Limit)
        {
            //if TestSequence has same Length as original Sequence break
            if (TestSequence.Count == gameObject.GetComponent<Machine>().SegmentPieces.Count)
            {
                if (settings.MaxTime < timer) settings.MaxTime = timer;
                break;
            }
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        while (t.Running) yield return null;

        //reset
        autoStart.GetComponent<AutoStart>().ResetAutoStart();
        gameObject.GetComponent<Machine>().ResetMachine();

        //Debug.Log(timer);

        //unsubscribe all
        foreach (GameObject segment in gameObject.GetComponent<Machine>().Segments)
        {
            foreach (Transform child in segment.transform)
            {
                if (child.tag == "SegmentPiece")
                {
                    child.GetComponent<SegmentPiece>().SegmentPieceCollisionEvent -= OnSegmentPieceCollision;
                }
                foreach (Transform c in child.transform)
                {
                    if (c.tag == "SegmentPiece")
                    {
                        c.GetComponent<SegmentPiece>().SegmentPieceCollisionEvent -= OnSegmentPieceCollision;
                    }
                }
            }
        }

        SceneManager.MoveGameObjectToScene(gameObject, mainScene);
        gameObject.transform.parent = populationHolder.transform;
        gameObject.SetActive(false);
    }

    private void OnSegmentPieceCollision(GameObject current)
    {
        TestSequence.Add(current);
    }
}
