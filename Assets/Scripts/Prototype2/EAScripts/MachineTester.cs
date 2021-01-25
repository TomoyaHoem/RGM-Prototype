using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MachineTester : MonoBehaviour
{
    public List<GameObject> TestSequence { get; set; }

    Scene mainScene;

    private void Awake()
    {
        mainScene = SceneManager.GetActiveScene();
    }

    public IEnumerator TestMachine(GameObject populationHolder)
    {
        //start auto start
        GameObject autoStart = gameObject.GetComponent<Machine>().AutoStart;
        Task t = new Task(autoStart.GetComponent<AutoStart>().MovePiston(autoStart.transform.GetChild(0).transform));

        //subscribe all
        foreach(GameObject segment in gameObject.GetComponent<Machine>().Segments)
        {
            foreach(Transform child in segment.transform)
            {
                if(child.tag == "SegmentPiece")
                {
                    child.GetComponent<SegmentPiece>().SegmentPieceCollisionEvent += OnSegmentPieceCollision;
                }
            }
        }

        //loop test timer or testsequence complete
        TestSequence = new List<GameObject>();
        float timer = 0f;
        while (timer < 1f)
        {
            //if TestSequence has same Length as original Sequence break
            if (TestSequence.Count == gameObject.GetComponent<Machine>().SegmentPieces.Count) break;
            timer += Time.deltaTime / Time.timeScale;
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
            }
        }

        SceneManager.MoveGameObjectToScene(gameObject, mainScene);
        gameObject.transform.parent = populationHolder.transform;
    }

    private void OnSegmentPieceCollision(GameObject current)
    {
        TestSequence.Add(current);
    }
}
