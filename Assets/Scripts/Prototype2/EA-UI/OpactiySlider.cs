using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpactiySlider : MonoBehaviour
{
    public CanvasGroup togglealbeUI;

    public void ChangeOpactiy(float value)
    {
        if(value > 0.1f)
        {
            togglealbeUI.alpha = value;
        }
    }

}
