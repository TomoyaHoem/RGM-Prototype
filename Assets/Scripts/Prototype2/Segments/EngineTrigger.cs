﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineTrigger : MonoBehaviour
{
    public GameObject Engine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log(gameObject.name + " , " + collision.gameObject.name);
        Engine.GetComponent<CarEngine>().SwitchEngineState(collision);
    }
}
