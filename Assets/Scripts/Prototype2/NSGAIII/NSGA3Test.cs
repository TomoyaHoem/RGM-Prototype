﻿using System.Collections;
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

        List<GameObject> testPopulation = new List<GameObject>();

        foreach (ReferencePoint p in refPoints)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = new Vector3(p.Position[0], p.Position[1], p.Position[2]);
            g.transform.localScale = Vector3.one * .1f * .5f;
            g.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            g.transform.parent = gameObject.transform;
        }

        GameObject x = new GameObject("x");
        x.AddComponent<Machine>().FitnessVals = new List<float> { 0, 0, 0};
        testPopulation.Add(x);

        GameObject y = new GameObject("y");
        y.AddComponent<Machine>().FitnessVals = new List<float> { 0, 1, 0};
        testPopulation.Add(y);

        GameObject z = new GameObject("z");
        z.AddComponent<Machine>().FitnessVals = new List<float> { 1, 0, 1};
        testPopulation.Add(z);

        GameObject a = new GameObject("a");
        a.AddComponent<Machine>().FitnessVals = new List<float> { 2, 2, 3};
        testPopulation.Add(a);

        GameObject b = new GameObject("b");
        b.AddComponent<Machine>().FitnessVals = new List<float> { 2, 2, 4};
        testPopulation.Add(b);

        List<List<GameObject>> fronts = FastNonDominatedSort.CalculateFronts(testPopulation);

        int count = 0;

        foreach(List<GameObject> front in fronts)
        {
            Debug.Log("front" + count + ": " + front.Count);
            foreach(GameObject g in front)
            {
                Debug.Log(g);
            }
            count++;
        }
    }
}
