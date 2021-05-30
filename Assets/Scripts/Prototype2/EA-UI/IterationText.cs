using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IterationText : UIPart
{
    [SerializeField] private TextMeshProUGUI it;

    private void Start()
    {
        UIStatistics.Instance.UI.Add(gameObject);
    }

    public override void UpdateStatistics()
    {
        int iteration = UIStatistics.Instance.Iteration;
        it.SetText("Iteration i: " + iteration);
    }

}
