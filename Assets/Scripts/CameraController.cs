using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Range(10, 30)]
    public float panSpeed = 20f;
    [Range(5, 15)]
    public float dragSpeed = 10f;
    public Vector2 panLimit;
    public float scrollSpeed = 200;
    [Range(2,200)]
    public float sizeLimit;

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;

        //wasd camera control
        if(Input.GetKey("w"))
        {
            pos.y += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a"))
        {
            pos.x -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s"))
        {
            pos.y -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("d"))
        {
            pos.x += panSpeed * Time.deltaTime;
        }

        //middlemouse drag
        if (Input.GetMouseButton(2))
        {
            float zoom = Camera.main.orthographicSize * dragSpeed;
            pos.x -= Input.GetAxis("Mouse X") * zoom * Time.deltaTime;
            pos.y -= Input.GetAxis("Mouse Y") * zoom * Time.deltaTime;
        }

        //scroll zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize -= scroll * scrollSpeed * Time.deltaTime;

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 1, sizeLimit);
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);

        transform.position = pos;
    }
}
