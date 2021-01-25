using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//generates edgeCollider2D around machines as bounding box
public class RestrictionArea : MonoBehaviour
{
    public void GenerateRestrictionArea(float machineArea)
    {
        //point distance from origin
        float pDFO = machineArea / 2;

        //calculate 5 points (rectangle) based on area
        Vector2[] points = new Vector2[5];
        for(int i = 0; i < 5; i++)
        {
            switch (i)
            {
                case 1:
                    //point 1 bottom right corner
                    points[i] = new Vector2(pDFO, -pDFO);
                    break;
                case 2:
                    //point 2 top right corner
                    points[i] = new Vector2(pDFO, pDFO);
                    break;
                case 3:
                    //point 3 top left corner
                    points[i] = new Vector2(-pDFO, pDFO);
                    break;
                default:
                    //point 0 & 4 bottom left corner
                    points[i] = new Vector2(-pDFO, -pDFO);
                    break;
            }
        }

        //add edge collider and set edge collider points
        gameObject.AddComponent<EdgeCollider2D>().points = points;

        gameObject.GetComponent<EdgeCollider2D>().edgeRadius = 1f;
    }
}
