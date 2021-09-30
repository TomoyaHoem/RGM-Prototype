using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NSGA3Test : MonoBehaviour
{
    public int numPoints;
    public int numObjectives;

    // Start is called before the first frame update
    void Start()
    {
        List<ReferencePoint> refPoints = ReferencePointCalculator.CalculateReferencePoints(numPoints, numObjectives);
        Debug.Log(refPoints.Count);

        List<GameObject> testPopulation = new List<GameObject>();

        Color color = Color.yellow; color.a = 0.1f;

        foreach (ReferencePoint p in refPoints)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = new Vector3(p.Position[0], p.Position[1], p.Position[2]);
            g.transform.localScale = Vector3.one * .1f * .1f;
            g.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            g.transform.parent = gameObject.transform;
            g.AddComponent<RefPointLines>();
        }


        GameObject individuals = new GameObject("Population");

        for (int i = 0; i < 100; i++)
        {
            GameObject indv = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indv.name = i.ToString();
            indv.AddComponent<Machine>().FitnessVals = new List<float> { Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f) };
            indv.transform.position = new Vector3(indv.GetComponent<Machine>().FitnessVals[0], indv.GetComponent<Machine>().FitnessVals[1], indv.GetComponent<Machine>().FitnessVals[2]);
            indv.transform.localScale = Vector3.one * .1f * .25f;
            indv.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            testPopulation.Add(indv);
            indv.transform.parent = individuals.transform;
            indv.AddComponent<RefPointLines>();
        }

        List<List<GameObject>> fronts = FastNonDominatedSort.CalculateFronts(testPopulation);

        int count = 0;

        foreach(List<GameObject> front in fronts)
        {
            //Debug.Log("front" + count + ": " + front.Count);
            foreach(GameObject g in front)
            {
                //Debug.Log(g);
            }
            count++;
        }

        //List<GameObject> nextPop = NSGA3.NSGAIII(fronts, new List<ReferencePoint>(refPoints), 50);
        List<GameObject> nextPop = NSGA2.NSGAII(fronts, 50);

        foreach(GameObject machine in nextPop)
        {
            machine.GetComponent<Renderer>().material.SetColor("_Color", Color.blue);
        }

        Debug.Log("NSGA FINISHED");
    }
}
