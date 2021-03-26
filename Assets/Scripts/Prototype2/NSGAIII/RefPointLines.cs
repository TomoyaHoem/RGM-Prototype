using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefPointLines : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        Color color = Color.yellow; color.a = 0.1f;
        Debug.DrawLine(Vector3.zero, gameObject.transform.position.normalized*1.5f, color, 0.1f);
    }
}
