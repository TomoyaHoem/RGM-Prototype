using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableButton : MonoBehaviour
{
    public CanvasGroup toggleableUI;

    public void ToggleUI()
    {
        toggleableUI.interactable = !toggleableUI.interactable;
        toggleableUI.alpha = toggleableUI.alpha == 0 ? 1 : 0;
    }
}
