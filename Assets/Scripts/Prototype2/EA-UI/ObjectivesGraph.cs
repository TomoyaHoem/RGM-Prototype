using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObjectivesGraph : UIPart
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private List<TextMeshProUGUI> xAxis;

    List<List<GameObject>> points, connections;

    private void Start()
    {
        points = new List<List<GameObject>>();
        connections = new List<List<GameObject>>();

        for (int i = 0; i < SettingsReader.Instance.EASettings.FitFunc.Count-1; i++)
        {
            points.Add(new List<GameObject>());
            connections.Add(new List<GameObject>());
        }

        UIStatistics.Instance.UI.Add(gameObject);
    }

    public override void UpdateStatistics()
    {
        int iteration = UIStatistics.Instance.Iteration;
        float[] ObjectiveVals = UIStatistics.Instance.AverageObjectives;

        Color color;

        for (int i = 0; i < ObjectiveVals.Length; i++)
        {
            //freq green, lin yellow, compactness purple, coverage orange 
            color = GetColor(i);

            float yPos = -120 + ObjectiveVals[i] * 200;

            if (points[i].Count == 0)
            {
                //create first data point at min x and corresponding y
                //min x -250 and max y 200
                points[i].Add(SetPoint(new Vector2(-250, yPos), color));
            }
            else if (points[i].Count < 9)
            {
                //else x value = -250 + iteration * distance, distance = 95/2
                points[i].Add(SetPoint(new Vector2(-250 + iteration * 47.5f, yPos), color));
                connections[i].Add(CreateConnection(points[i][points[i].Count - 2].GetComponent<RectTransform>().anchoredPosition, points[i][points[i].Count - 1].GetComponent<RectTransform>().anchoredPosition, color));
            }
            else
            {
                //if more than 8 iterations shift graph and x axis
                //delete first point and place at 8
                GameObject first = points[i][0];
                points[i].RemoveAt(0);
                Destroy(first);
                //delete first connection and connect new to previous
                first = connections[i][0];
                connections[i].RemoveAt(0);
                Destroy(first);
                //shift other points
                foreach (GameObject g in points[i])
                {
                    RectTransform r = g.GetComponent<RectTransform>();
                    r.anchoredPosition = new Vector2(r.anchoredPosition.x - 47.5f, r.anchoredPosition.y);
                }
                //shift connections
                foreach (GameObject c in connections[i])
                {
                    RectTransform r = c.GetComponent<RectTransform>();
                    r.anchoredPosition = new Vector2(r.anchoredPosition.x - 47.5f, r.anchoredPosition.y);
                }
                //add new point and connection
                points[i].Add(SetPoint(new Vector2(-250 + 8 * 47.5f, yPos), color));
                connections[i].Add(CreateConnection(points[i][points[i].Count - 2].GetComponent<RectTransform>().anchoredPosition, points[i][points[i].Count - 1].GetComponent<RectTransform>().anchoredPosition, color));
                if (i == 0)
                {
                    //shift xAxis
                    foreach (TextMeshProUGUI text in xAxis)
                    {
                        text.SetText((int.Parse(text.text) + 1).ToString());
                    }
                }
            }
        }
    }

    private GameObject SetPoint(Vector2 position, Color color)
    {
        GameObject circle = new GameObject("circle", typeof(Image));
        circle.transform.SetParent(gameObject.transform, false);
        circle.GetComponent<Image>().color = color;
        circle.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = circle.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(5, 5);
        return circle;
    }

    private GameObject CreateConnection(Vector2 posA, Vector2 posB, Color color)
    {
        GameObject connect = new GameObject("connect", typeof(Image));
        connect.transform.SetParent(gameObject.transform, false);
        connect.GetComponent<Image>().color = color;
        RectTransform rectTransform = connect.GetComponent<RectTransform>();

        Vector2 dir = (posB - posA).normalized;
        float distance = Vector2.Distance(posA, posB);

        rectTransform.sizeDelta = new Vector2(distance, 1.5f);
        rectTransform.anchoredPosition = posA + dir * distance * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        return connect;
    }

    private Color GetColor(int i)
    {
        Color color;

        switch (i)
        {
            case 0:
                color = new Color(0, 1, 0, 0.5f);
                break;
            case 1:
                color = new Color(1, 1, 0, 0.5f);
                break;
            case 2:
                color = new Color(1, 0, 1, 0.5f);
                break;
            default:
                color = new Color(1, 0.5f, 0, 0.5f);
                break;
        }
        
        return color;
    }
}
