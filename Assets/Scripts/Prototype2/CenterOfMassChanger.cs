using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CenterOfMassChanger : MonoBehaviour
{
    public Vector2 CenterOfMassNew;
    public bool Awake;
    Rigidbody2D r;

    private void Start()
    {
        r = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        r.centerOfMass = CenterOfMassNew;
        r.WakeUp();
        Awake = !r.IsSleeping();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + transform.rotation * CenterOfMassNew, 0.01f);
    }
}
