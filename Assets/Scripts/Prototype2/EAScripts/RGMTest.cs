using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RGMTest : MonoBehaviour
{
    //starts process

    // Start is called before the first frame update
    void Start()
    {
        //init reference to Evolution scirpt
        RGMEA rg = FindObjectOfType<RGMEA>();

        //Call Evolution
        StartCoroutine(rg.EvolveMachines());
    }

    //custom sign function to return 0 when x is 0
    public static int Sign(float x)
    {
        return x < 0 ? -1 : (x > 0 ? 1 : 0);
    }

    public static float LN(float x)
    {
        if(x == 0)
        {
            return 0;
        } else
        {
            return Mathf.Log(x);
        }
    }
}
