using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopulationStat : UIPart
{
    public TextMeshProUGUI PopSize;
    public TextMeshProUGUI FeasSize;
    public TextMeshProUGUI InfeasSize;

    public Image FeasFill;
    public Image InfeasFill;

    private void Start()
    {
        UIStatistics.Instance.UI.Add(gameObject);
    }

    public override void UpdateStatistics()
    {
        PopSize.SetText(UIStatistics.Instance.PopulationSize.ToString());
        FeasSize.SetText(UIStatistics.Instance.FeasSize.ToString());
        InfeasSize.SetText(UIStatistics.Instance.InfeasSize.ToString());

        FeasFill.fillAmount = (float)UIStatistics.Instance.FeasSize / UIStatistics.Instance.PopulationSize;
        InfeasFill.fillAmount = (float)UIStatistics.Instance.InfeasSize / UIStatistics.Instance.PopulationSize;
    }
}
