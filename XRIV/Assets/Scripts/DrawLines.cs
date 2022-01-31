using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine.Rendering;
using Microsoft.MixedReality.Toolkit;


public class DrawLines : BaseInputHandler, IMixedRealityPointerHandler
{
    public static DrawLines instance;
    public static LineRenderer line;
    public static bool ClickPhoto;
    private Vector3 mousePos;
    public Material posMaterial;
    public Material negMaterial;
    public bool DrawLine;
    public bool DrawPoint;
    public float LineWidth;
    private int currLines = 0;

    [HideInInspector]
    public List<LineRenderer> posList = new List<LineRenderer>();
    [HideInInspector]
    public List<LineRenderer> negList = new List<LineRenderer>();
    [HideInInspector]
    public List<LineRenderer> allList = new List<LineRenderer>();

    private void Awake()
    {
        instance = this;
    }

    protected override void RegisterHandlers()
    {
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    protected override void UnregisterHandlers()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
    }

    void IMixedRealityPointerHandler.OnPointerUp(
     MixedRealityPointerEventData eventData)
    {
        if (DrawLine)
        {
            var result = eventData.Pointer.Result;
            mousePos = result.Details.Point;
            if (result.CurrentPointerTarget == null)
            {
                // ray pointer did not hit any objects/spatial mesh
                Debug.Log("No Walls Detected!");
            }

            line.SetPosition(1, mousePos);
            line = null;
            currLines++;
        }
    }

    void IMixedRealityPointerHandler.OnPointerDown(
    MixedRealityPointerEventData eventData)
    {
        if (DrawLine)
        {
            if (line == null)
            {
                createLine();
            }

            var result = eventData.Pointer.Result;
            mousePos = result.Details.Point;
            if (result.CurrentPointerTarget == null)
            {
                // ray pointer did not hit any objects/spatial mesh
                Debug.Log("No Walls Detected!");
            }

            line.SetPosition(0, mousePos);
            line.SetPosition(1, mousePos);
        }
    }

    void IMixedRealityPointerHandler.OnPointerDragged(
         MixedRealityPointerEventData eventData)
    {
        if (DrawLine)
        {
            var result = eventData.Pointer.Result;
            mousePos = result.Details.Point;
            if (result.CurrentPointerTarget == null)
            {
                // ray pointer did not hit any objects/spatial mesh
                Debug.Log("No Walls Detected!");
            }

            line.SetPosition(1, mousePos);
        }
    }

    void IMixedRealityPointerHandler.OnPointerClicked(
        MixedRealityPointerEventData eventData)
    {
        if (DrawPoint)
        {
            
            var result = eventData.Pointer.Result;
            mousePos = result.Details.Point;
            if (result.CurrentPointerTarget == null)
            {
                // ray pointer did not hit any objects/spatial mesh
                Debug.Log("No Walls Detected!");
                createLine();
                line.SetPosition(0, mousePos);
                line.SetPosition(1, mousePos);
            }
            else if (ResultsLabel.instance.m_raycastLayer == (ResultsLabel.instance.m_raycastLayer | (1 << result.Details.Object.layer)))
            {
                createLine();
                line.SetPosition(0, mousePos);
                line.SetPosition(1, mousePos);
            }
        }

        else if (ClickPhoto)
        {
            ClickGlobal.instance.ClickPhoto();
            if (!ClickGlobal.instance.continousClicking)
            {
                ClickPhoto = false;
            }
        }
    }


    public void drawLine(Vector3 mousePos)
    {
        positivePoint.instance.on = false;
        createLine();
        line.SetPosition(0, mousePos);
        line.SetPosition(1, mousePos);
        line.material.SetColor("_Color", Color.green);
        line.startWidth = LineWidth*1.2f;
        line.endWidth = LineWidth*1.2f;
        positivePoint.instance.on = true;
    }
    void createLine()
    {
        GameObject lineObject = new GameObject("Line" + currLines);
        line = lineObject.AddComponent<LineRenderer>();
        
        line.positionCount = 2;
        line.startWidth = LineWidth;
        line.endWidth = LineWidth;
        line.useWorldSpace = false;
        line.numCapVertices = 50;
        if (positivePoint.instance.on)
        {
            line.material = posMaterial;            
            posList.Add(line);
            allList.Add(line);
        }
        else if (negativePoint.instance.on)
        {
            line.material = negMaterial;
            negList.Add(line);
            allList.Add(line);
        }

    }

}

