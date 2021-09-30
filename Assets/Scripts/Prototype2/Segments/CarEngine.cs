using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarEngine : MonoBehaviour
{
    public GameObject Carosserie;
    public WheelJoint2D Tire1;
    public WheelJoint2D Tire2;

    public bool active;

    private void Awake()
    {
        Tire1 = Carosserie.GetComponents<WheelJoint2D>()[0];
        Tire2 = Carosserie.GetComponents<WheelJoint2D>()[1];
    }

    public void SwitchEngineState(Collider2D collision)
    {
        //switch on
        if (!active && collision.gameObject.name.Contains("Ball") || collision.gameObject.name.Contains("Car"))
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            Tire1.useMotor = true;
            Tire2.useMotor = true;
            active = true;
        }  //switch off
        else if (active && collision.gameObject.tag == "SegmentPiece")
        {
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            Tire1.useMotor = false;
            Tire2.useMotor = false;
            active = false;
        }
    }

    public void ResetEngine()
    {
        gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        Tire1.useMotor = false;
        Tire2.useMotor = false;
        active = false;
    }
}
