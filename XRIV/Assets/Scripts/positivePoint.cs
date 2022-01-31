using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class positivePoint : MonoBehaviour
{
    [HideInInspector]
    public bool on;
    public static positivePoint instance;
    private void Awake()
    {
        instance = this;
        on = false;
    }

    public void PositiveClick()
    {

        DrawLines.ClickPhoto = false;
        if (on & DrawLines.instance.DrawPoint)
        {
            DrawLines.instance.DrawPoint = false;
        } else
        {
            DrawLines.instance.DrawPoint = true;
        }
        on = true;
        negativePoint.instance.on = false;
    }

}

