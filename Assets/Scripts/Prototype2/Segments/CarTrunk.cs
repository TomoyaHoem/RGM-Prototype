using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTrunk : MonoBehaviour
{
    public WheelJoint2D Tire1;
    public WheelJoint2D Tire2;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Tire1.useMotor = true;
        Tire2.useMotor = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Tire1.useMotor = false;
        Tire2.useMotor = false;
    }
}
