using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouse : MonoBehaviour
{

    [SerializeField]
    float movSpeed = 1.0f;

    void Update()
    {
        RotateAndMoveTowardsMouse();
    }

    void RotateAndMoveTowardsMouse()
    {
        //get mouse position and translate to world position
        Vector2 cursorPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        //objects direction towards mouse
        Vector2 direction = new Vector2(
            cursorPos.x - transform.position.x,
            cursorPos.y - transform.position.y
            );

        //rotate towards mouse
       
        float angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
        

        //move towards mouse
        transform.position = Vector2.Lerp(transform.position, cursorPos, movSpeed);
    }
}
