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
        int shape = SettingsReader.Instance.MachineSettings.AreaShape;

        Vector2[] points;

        if (shape == 1)
        {

            //calculate 5 points (square) based on area
            points = new Vector2[5];
            for (int i = 0; i < 5; i++)
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

            //add edge collider and set points
            gameObject.AddComponent<EdgeCollider2D>().points = points;

        }
        else if (shape == 2)
        {
            //cirlce
            points = new Vector2[121];

            for (int i = 0; i < 121; i++)
            {
                float angle = 2 * Mathf.PI * i / 120;
                float x = pDFO * Mathf.Cos(angle);
                float y = pDFO * Mathf.Sin(angle);

                points[i] = new Vector2(x, y);
            }

            points[120] = points[0];

            //add edge collider and set points
            gameObject.AddComponent<EdgeCollider2D>().points = points;
        }
        else
        {
            //triangle
            points = new Vector2[4];

            points[0] = new Vector2(0, pDFO);

            for (int i = 1; i < 3; i++)
            {
                float angle = 120 * i * Mathf.Deg2Rad;

                float x = points[0].x * Mathf.Cos(angle) - points[0].y * Mathf.Sin(angle);
                float y = points[0].x * Mathf.Sin(angle) + points[0].y * Mathf.Cos(angle);

                points[i] = new Vector2(x, y);
            }

            points[3] = points[0];

            //add edge collider and set points
            gameObject.AddComponent<EdgeCollider2D>().points = points;
        }

        //set edge collider width
        gameObject.GetComponent<EdgeCollider2D>().edgeRadius = 1f;
    }
}
