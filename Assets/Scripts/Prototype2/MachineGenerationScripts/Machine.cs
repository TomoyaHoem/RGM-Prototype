using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine : MonoBehaviour
{
    //store machine data
    //machine segments
    [SerializeField]
    public List<GameObject> Segments { get; set; }
    public List<GameObject> SegmentPieces { get; set; }
    public GameObject AutoStart { get; set; }
    public GameObject Canvas { get; set; }

    public List<float> FitnessVals { get; set; }
    public float Fitness { get; set; }

    public bool IsSelected { get; set; }

    //Event triggers when Machine is selected
    public event Action<GameObject> MachineSelectedEvent;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        Segments = new List<GameObject>();
        SegmentPieces = new List<GameObject>();
    }

    private void OnMouseDown()
    {
        SwitchSelect();
        //notify Machine selected
        MachineSelectedEvent?.Invoke(gameObject);
    }

    //switch IsSelected state
    public void SwitchSelect()
    {
        if (IsSelected)
        {
            IsSelected = false;
            spriteRenderer.enabled = false;
        } else
        {
            IsSelected = true;
            spriteRenderer.enabled = true;
        }
    }

    //Adds a BoxCollider2D to allow selection of Machines through clicks
    public void AddSelectionArea(float machineArea)
    {
        BoxCollider2D machineCollider = gameObject.AddComponent<BoxCollider2D>();
        machineCollider.enabled = false;
        gameObject.layer = 8;
        machineCollider.size = new Vector2(machineArea, machineArea);

        AddSelectionSprite(machineArea);
    }

    //Selected Machines will be surrounded by a Red&Black Sprite
    private void AddSelectionSprite(float machineArea)
    {
        GameObject sprite = new GameObject("Sprite");
        sprite.transform.position = gameObject.transform.position;
        sprite.transform.parent = gameObject.transform;

        spriteRenderer = sprite.AddComponent<SpriteRenderer>();
        spriteRenderer.enabled = false;

        spriteRenderer.sprite = Resources.Load<Sprite>("Sprites/select");
        spriteRenderer.color = new Color(1, 0, 0, 0.4f);
        spriteRenderer.sortingOrder = -1;

        spriteRenderer.transform.localScale = new Vector2(machineArea, machineArea);
    }

    public void AddFitnessBarChart(float machineArea)
    {
        Dictionary<string, float> fit = SettingsReader.Instance.EASettings.FitFunc;
        if(fit.Count > 0)
        {
            Canvas = Instantiate(Resources.Load("Prefabs/BarChart/BarChartCanvas"), gameObject.transform) as GameObject;
            float scale = machineArea / 100;
            Canvas.GetComponent<RectTransform>().transform.localScale = Canvas.GetComponent<RectTransform>().transform.localScale * scale;
            Canvas.transform.parent = gameObject.transform;
        }
    }

    public void ResetMachine()
    {
        foreach(GameObject segment in Segments)
        {
            segment.GetComponent<SegmentPart>().ResetSegment();
        }
    }

    public void InitSegPieces()
    {
        foreach(GameObject seg in Segments)
        {
            foreach(Transform child in seg.transform)
            {
                if(child.tag == "SegmentPiece") SegmentPieces.Add(child.gameObject);
            }
        }
    }
}
