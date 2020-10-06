using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoStart : MonoBehaviour
{

    public Vector2 PistonDirection { get; set; }

    void OnMouseDown()
    {
        StartCoroutine(MovePiston(transform.GetChild(0).transform));
    }

    //move piston half a unit towards dir
    IEnumerator MovePiston(Transform piston)
    {
        piston.position = new Vector3(piston.position.x + 0.2f * PistonDirection.x, 0, 0);
        yield return new WaitForSeconds(.1f);

    }
}
