using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HammerSwitch : MonoBehaviour
{
    public GameObject Hammer;

    public bool IsActive { get; set; }

    private void Awake()
    {
        IsActive = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag != "Ignore" && IsActive)
        {
            if ((collision.gameObject.tag == "SegmentPiece" && !collision.gameObject.GetComponent<SegmentPiece>().WasActivatedAndPassed && collision.gameObject.GetComponent<SegmentPiece>().Active) || collision.gameObject.tag == "Piston")
            {
                if (collision.gameObject.tag == "Piston")
                {
                    collision.gameObject.SetActive(false);
                } else
                {
                    collision.gameObject.GetComponent<SegmentPiece>().WasActivatedAndPassed = true;
                    collision.gameObject.GetComponent<SegmentPiece>().Active = false;
                    if (collision.gameObject.GetComponent<SpriteRenderer>() != null)
                    {
                        collision.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                    }
                    else
                    {
                        foreach (Transform child in collision.gameObject.transform)
                        {
                            child.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                        }
                    }
                }
                Hammer.GetComponent<SegmentPiece>().Activate();
            }
            Hammer.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
            IsActive = false;
        }
    }
}
