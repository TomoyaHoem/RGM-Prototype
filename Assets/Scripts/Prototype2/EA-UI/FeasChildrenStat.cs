using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeasChildrenStat : UIPart
{
    public TextMeshProUGUI ChildCount;
    public TextMeshProUGUI StaySize;
    public TextMeshProUGUI TransferSize;
    public TextMeshProUGUI DiscardSize;

    public Image StayFill;
    public Image TransferFill;

    private void Start()
    {
        UIStatistics.Instance.UI.Add(gameObject); 
    }

    public override void UpdateStatistics()
    {
        if(UIStatistics.Instance.FeasChildCount > 0)
        {
            ChildCount.SetText(UIStatistics.Instance.FeasChildCount.ToString());
            StaySize.SetText(UIStatistics.Instance.FeasStay.ToString());
            TransferSize.SetText(UIStatistics.Instance.FeasTransfer.ToString());

            DiscardSize.SetText((UIStatistics.Instance.FeasChildCount - (UIStatistics.Instance.FeasStay + UIStatistics.Instance.FeasTransfer)).ToString());

            StayFill.fillAmount = (float)UIStatistics.Instance.FeasStay / UIStatistics.Instance.FeasChildCount;
            TransferFill.fillAmount = (float)UIStatistics.Instance.FeasTransfer / UIStatistics.Instance.FeasChildCount;
        } else
        {
            ChildCount.SetText(UIStatistics.Instance.FeasChildCount.ToString());
            StaySize.SetText(UIStatistics.Instance.FeasStay.ToString());
            TransferSize.SetText(UIStatistics.Instance.FeasTransfer.ToString());

            DiscardSize.SetText("0");
            StayFill.fillAmount = 0;
            TransferFill.fillAmount = 0;
        }
    }
}
