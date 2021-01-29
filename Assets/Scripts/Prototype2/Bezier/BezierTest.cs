using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierTest : MonoBehaviour
{
    [Range(0.05f, 1.5f)]
    public float spacing = 1;
    public float resolution = 1;
    public float meshWidth = 1;
    public float tiling = 1;

    Vector2 input;
    Vector2 output;

    Vector2 direction = Vector2.right;

    public GameObject curve;

    // Start is called before the first frame update
    void Start()
    {
        MeshFilter bezierMesh = curve.AddComponent<MeshFilter>();
        GameObject pathHolder = new GameObject("PathHolder");
        pathHolder.transform.parent = curve.transform;

        input = transform.position;

        float ranH = Random.Range(-5, .5f);
        int ranL = Random.Range(4, 16) * (int)direction.x;

        //Debug.Log("rampH: " + ranH + " , rampL: " + ranL);

        output = new Vector2(ranL, ranH);

        int numPoints = Random.Range(1, (Mathf.Abs(ranL) / 4) + 1);

        //Debug.Log("nump: " + numPoints);

        List<Vector2> midPoints = CalculateMidPoints(numPoints);

        Path path = new Path(input + direction * 0.5f, output - direction, direction, midPoints);

        for (int i = 0; i < path.NumPoints; i++)
        {
            //Debug.Log(path[i]);
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = path[i];
            if (i % 3 == 0)
            {
                g.transform.localScale = Vector3.one * 0.3f;
                g.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            } else
            {
                g.transform.localScale = Vector3.one * 0.2f;
                g.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1, 125));
            }
            g.transform.parent = pathHolder.transform;
        }

        Vector2[] evenPoints = path.CalculateEvenlySpacedPoints(spacing, resolution);
        foreach(Vector2 p in evenPoints)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = p;
            g.transform.localScale = Vector3.one * .1f * .5f;
            g.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
            g.transform.parent = pathHolder.transform;
        }

        bezierMesh.mesh = BezierMeshCreator.CreateBezierMesh(evenPoints, meshWidth);

        int textureRepeat = Mathf.RoundToInt(tiling * evenPoints.Length * spacing * 0.5f);
        curve.GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
    }

    List<Vector2> CalculateMidPoints(int numPoints)
    {
        List<Vector2> points = new List<Vector2>();

        float start = input.x + direction.x * 0.5f;
        float end = output.x - direction.x;
        float dst = end - start;
        float step = dst / (numPoints + 1);

        for (int i = 0; i < numPoints; i++)
        {
            float pX = start + step + i * step;
            float hOffset = Random.Range(0, 2) * 2 - 1;
            float pY = Random.Range(input.y + hOffset, output.y + hOffset);

            points.Add(new Vector2(pX, pY));
        }

        return points;
    }
}
