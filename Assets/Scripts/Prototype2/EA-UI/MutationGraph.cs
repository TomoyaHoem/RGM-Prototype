using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MutationGraph : UIPart
{
    [SerializeField] private Sprite circleSprite;
    [SerializeField] private List<TextMeshProUGUI> xAxis;

    List<GameObject> points, connections;

    private void Start()
    {
        points = new List<GameObject>();
        connections = new List<GameObject>();
        UIStatistics.Instance.UI.Add(gameObject);
    }

    public override void UpdateStatistics()
    {
        int iteration = UIStatistics.Instance.Iteration;
        float mut = UIStatistics.Instance.MutationChance;

        float yPos = -120 + mut * 200;

        if (points.Count == 0)
        {
            //create first data point at min x and corresponding y
            //min x -250 and max y 200
            points.Add(SetPoint(new Vector2(-250, yPos)));
        }
        else if (points.Count < 9)
        {
            //else x value = -250 + iteration * distance, distance = 95/2
            points.Add(SetPoint(new Vector2(-250 + iteration * 47.5f, yPos)));
            connections.Add(CreateConnection(points[points.Count - 2].GetComponent<RectTransform>().anchoredPosition, points[points.Count - 1].GetComponent<RectTransform>().anchoredPosition));
        }
        else
        {
            //if more than 8 iterations shift graph and x axis
            //delete first point and place at 8
            GameObject first = points[0];
            points.RemoveAt(0);
            Destroy(first);
            //delete first connection and connect new to previous
            first = connections[0];
            connections.RemoveAt(0);
            Destroy(first);
            //shift other points
            foreach (GameObject g in points)
            {
                RectTransform r = g.GetComponent<RectTransform>();
                r.anchoredPosition = new Vector2(r.anchoredPosition.x - 47.5f, r.anchoredPosition.y);
            }
            //shift connections
            foreach (GameObject c in connections)
            {
                RectTransform r = c.GetComponent<RectTransform>();
                r.anchoredPosition = new Vector2(r.anchoredPosition.x - 47.5f, r.anchoredPosition.y);
            }
            //add new point and connection
            points.Add(SetPoint(new Vector2(-250 + 8 * 47.5f, yPos)));
            connections.Add(CreateConnection(points[points.Count - 2].GetComponent<RectTransform>().anchoredPosition, points[points.Count - 1].GetComponent<RectTransform>().anchoredPosition));
            //shift xAxis
            foreach (TextMeshProUGUI text in xAxis)
            {
                text.SetText((int.Parse(text.text) + 1).ToString());
            }
        }

    }

    private GameObject SetPoint(Vector2 position)
    {
        GameObject circle = new GameObject("circle", typeof(Image));
        circle.transform.SetParent(gameObject.transform, false);
        circle.GetComponent<Image>().sprite = circleSprite;
        RectTransform rectTransform = circle.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(5, 5);
        return circle;
    }

    private GameObject CreateConnection(Vector2 posA, Vector2 posB)
    {
        GameObject connect = new GameObject("connect", typeof(Image));
        connect.transform.SetParent(gameObject.transform, false);
        connect.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        RectTransform rectTransform = connect.GetComponent<RectTransform>();

        Vector2 dir = (posB - posA).normalized;
        float distance = Vector2.Distance(posA, posB);

        rectTransform.sizeDelta = new Vector2(distance, 1.5f);
        rectTransform.anchoredPosition = posA + dir * distance * 0.5f;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        rectTransform.localEulerAngles = new Vector3(0, 0, angle);

        return connect;
    }
}
