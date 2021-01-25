using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarChart : MonoBehaviour
{

    private List<GameObject> bars = new List<GameObject>();

    void Start()
    {
        Dictionary<string, float> fit = SettingsReader.Instance.EASettings.FitFunc;

        //scale BarChart width 6 + 4 * Count-1
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(6 + 4 * (fit.Count - 1), gameObject.GetComponent<RectTransform>().sizeDelta.y);

        //instantiate Bar for each DictEntry
        foreach(var pair in fit)
        {
            GameObject newBar = Instantiate(Resources.Load("Prefabs/BarChart/Bar Holder"), gameObject.transform) as GameObject;
            newBar.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = pair.Key;
            bars.Add(newBar);
        }
    }

    public void UpdateBars(List<float> fit)
    {
        for (int i = 0; i < bars.Count; i++)
        {
            bars[i].transform.GetChild(0).GetComponent<Image>().fillAmount = fit[i];
        }
    }

    public void ResetBars()
    {
        for (int i = 0; i < bars.Count; i++)
        {
            bars[i].transform.GetChild(0).GetComponent<Image>().fillAmount = 0;
        }
    }
}
