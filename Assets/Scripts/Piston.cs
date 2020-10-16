using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piston : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "SegmentPiece")
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(new Vector2(100 * GetComponentInParent<AutoStart>().PistonDirection.x, 0));
        }
    }
}
