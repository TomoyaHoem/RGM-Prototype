using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentPiece : MonoBehaviour
{
    //Event triggers when SP collides with another
    public event Action<GameObject> SegmentPieceCollisionEvent;

    public bool Active { get; set; }
    public bool WasActivatedAndPassed { get; set; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(Active && collision.gameObject.tag == "SegmentPiece" && !collision.gameObject.GetComponent<SegmentPiece>().WasActivatedAndPassed)
        {
            WasActivatedAndPassed = true;
            Active = false;
            gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            collision.gameObject.GetComponent<SegmentPiece>().Activate();
        }
    }

    public void Activate()
    {
        if (!WasActivatedAndPassed)
        {
            Active = true;
            gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            SegmentPieceCollisionEvent?.Invoke(gameObject);
        }
    }

    public void ResetTest()
    {
        Active = false;
        WasActivatedAndPassed = false;
    }
}
