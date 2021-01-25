using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoStart : MonoBehaviour
{

    public Vector2 PistonDirection { get; set; }

    public void GenerateAutoStart(Vector2 startDir, Transform parent)
    {
        Vector2 spawnPos = Vector2.zero;

        if (startDir.x > 0) //place to the left of start
        {
            spawnPos = new Vector2(parent.position.x - 0.75f, parent.position.y);
        }
        else //place to the right of start
        {
            spawnPos = new Vector2(parent.position.x + 0.75f, parent.position.y);
        }

        //instantiate on a new GameObject and transfer values
        GameObject autoStart = Instantiate(Resources.Load("Prefabs/AutoStart"), spawnPos, Quaternion.identity, parent) as GameObject;
        autoStart.name = "AutoStart";
        autoStart.GetComponent<AutoStart>().PistonDirection = startDir;
        parent.GetComponent<Machine>().AutoStart = autoStart;
    }

    void OnMouseDown()
    {
        StartCoroutine(MovePiston(transform.GetChild(0).transform));
    }

    //move piston half a unit towards dir
    public IEnumerator MovePiston(Transform piston)
    {
        while(piston.gameObject.activeSelf)
        {
            piston.position = new Vector3(piston.position.x + 0.1f * PistonDirection.x, piston.position.y, piston.position.z);
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ResetAutoStart()
    {
        transform.GetChild(0).position = transform.position;
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
