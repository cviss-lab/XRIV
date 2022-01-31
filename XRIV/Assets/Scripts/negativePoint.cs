using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class negativePoint : MonoBehaviour
{
    [HideInInspector]
    public bool on;
    public static negativePoint instance;
    private void Awake()
    {
        instance = this;
        on = false;
    }

    public void NegativeClick()
    {
        DrawLines.ClickPhoto = false;

        if (on & DrawLines.instance.DrawPoint)
        {
            DrawLines.instance.DrawPoint = false;
        }
        else
        {
            DrawLines.instance.DrawPoint = true;
        }
        on = true;
        positivePoint.instance.on = false;
    }
}
