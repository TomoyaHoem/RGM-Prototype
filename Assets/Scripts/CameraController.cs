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
    public float scrollSpeed = 1000f;
    [Range(2, 200)]
    public float sizeLimit = 50f;

    private bool isSelected = true;
    private Color defaultColor;

    private void Awake()
    {
        //panLimit = new Vector2(transform.position.x + 100f, transform.position.y + 100f);
        defaultColor = GetComponent<Camera>().backgroundColor;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (GetComponent<Camera>().pixelRect.Contains(Input.mousePosition) && Input.GetMouseButtonDown(0))
        {
            isSelected = true;
            //set active camera color to light blue
            GetComponent<Camera>().backgroundColor = new Color32(52, 92, 164, 255);
        }
        if (GetComponent<Camera>().pixelRect.Contains(Input.mousePosition) && Input.GetMouseButtonDown(1))
        {
            isSelected = false;
            //set active camera color back
            GetComponent<Camera>().backgroundColor = defaultColor;
        }
        */

        if (isSelected)
        {
            Vector3 pos = transform.position;

            //wasd camera control
            if (Input.GetKey("w"))
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
                float zoom = GetComponent<Camera>().orthographicSize * dragSpeed;
                pos.x -= Input.GetAxis("Mouse X") * zoom * Time.deltaTime;
                pos.y -= Input.GetAxis("Mouse Y") * zoom * Time.deltaTime;
            }

            //scroll zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            GetComponent<Camera>().orthographicSize -= scroll * scrollSpeed * Time.deltaTime;

            GetComponent<Camera>().orthographicSize = Mathf.Clamp(GetComponent<Camera>().orthographicSize, 1, sizeLimit);
            pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
            pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);

            transform.position = pos;
        }
    }
}
