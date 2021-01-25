using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piston : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "SegmentPiece")
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForceAtPosition(new Vector2(500 * GetComponentInParent<AutoStart>().PistonDirection.x, 0), collision.GetContact(0).point);
            collision.gameObject.GetComponent<SegmentPiece>().Activate();
            gameObject.SetActive(false);
        }
    }
}
