using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BezierMeshCreator
{

    public static Mesh CreateBezierMesh(Vector2[] points, float meshWidth)
    {
        //Debug.Log(points.Length);
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[2 * (points.Length - 1) * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            //direction to next point
            Vector2 forward = Vector2.zero;
            if(i < points.Length - 1)
            {
                forward += points[i + 1] - points[i];
            }
            if(i > 0)
            {
                forward += points[i] - points[i - 1];
            }
            forward.Normalize();
            //perpendicular vectors
            Vector2 left = new Vector2(-forward.y, forward.x);

            verts[vertIndex] = points[i] + left * meshWidth * 0.5f;
            verts[vertIndex + 1] = points[i] - left * meshWidth * 0.5f;

            float completionPercent = i / (float)(points.Length - 1);
            uvs[vertIndex] = new Vector2(0, completionPercent);
            uvs[vertIndex + 1] = new Vector2(1, completionPercent);

            if (i < points.Length - 1)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = vertIndex + 2;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = vertIndex + 2;
                tris[triIndex + 5] = vertIndex + 3;
            }

            vertIndex += 2;
            triIndex += 6;

        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        return mesh;
    }
}
