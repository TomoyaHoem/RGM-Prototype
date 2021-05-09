using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfeasChildrenStat : UIPart
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
        if (UIStatistics.Instance.InfeasChildCount > 0)
        {
            ChildCount.SetText(UIStatistics.Instance.InfeasChildCount.ToString());
            StaySize.SetText(UIStatistics.Instance.InfeasStay.ToString());
            TransferSize.SetText(UIStatistics.Instance.InfeasTransfer.ToString());

            DiscardSize.SetText((UIStatistics.Instance.InfeasChildCount - (UIStatistics.Instance.InfeasStay + UIStatistics.Instance.InfeasTransfer)).ToString());

            StayFill.fillAmount = (float)UIStatistics.Instance.InfeasStay / UIStatistics.Instance.InfeasChildCount;
            TransferFill.fillAmount = (float)UIStatistics.Instance.InfeasTransfer / UIStatistics.Instance.InfeasChildCount;
        } else
        {
            ChildCount.SetText(UIStatistics.Instance.InfeasChildCount.ToString());
            StaySize.SetText(UIStatistics.Instance.InfeasStay.ToString());
            TransferSize.SetText(UIStatistics.Instance.InfeasTransfer.ToString());

            DiscardSize.SetText("0");
            StayFill.fillAmount = 0;
            TransferFill.fillAmount = 0;
        }
    }
}
