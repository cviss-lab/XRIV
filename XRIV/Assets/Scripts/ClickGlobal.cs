using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.Windows.WebCam;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using JsonConversion;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine.XR.WSA;
using Microsoft.MixedReality.OpenXR.BasicSample;
#if UNITY_WSA && !UNITY_EDITOR // RUNNING ON WINDOWS
using Windows.Perception.Spatial;
using Windows.Media.Capture.Frames;
using Windows.Media.Devices.Core;
using Windows.Media;
#endif
public class ClickGlobal : MonoBehaviour
{
    public static ClickGlobal instance;
    public static bool analysisLabel = true;
    public int analysisType = 0;
    public static int tapsCount = 0;
    public bool continousClicking = false;
    public static bool captureVideo = false;
    public static bool startImageAnalysis = false;
    public static bool currentlyAnalyzing = false;


    private async void Awake()
    {
        instance = this;

    }

    public void ClickPhoto()
    {
       LocatableCamera.instance.TakePhoto();
    }

    void Update()
    {
        if (startImageAnalysis)
        {
            startImageAnalysis = false;
            currentlyAnalyzing = true;
            tapsCount++;
            StartAnalysis();

        }
    }

    public void finishedCapturing()
    {
        currentlyAnalyzing = false;

        if (captureVideo & DrawLines.ClickPhoto)
        {
            ClickPhoto();
        }

    }

    void StartAnalysis()
    {
        if (analysisLabel)
        {
            ResultsLabel.instance.CreateLabel();
        }

        StartCoroutine(NetworkManager.instance.AnalyseLastImageCaptured());

    }

    public void TakePhoto()
    {

        if (!DrawLines.ClickPhoto) 
        {
            DrawLines.ClickPhoto = true;
        } else
        {
            DrawLines.ClickPhoto = false;
        }
        
        DrawLines.instance.DrawPoint = false;
    }

    public void TakeVideo()
    {
        
        if (captureVideo)
        {
            analysisLabel = true;
            captureVideo = false;
        } else
        {
            analysisLabel = false;
            captureVideo = true;
        }

        DrawLines.ClickPhoto = false;
        TakePhoto();
    }



    public void QuitApp()
    {
        Application.Quit();
    }

    public static byte[] LoadJPG(string filePath)
    {

        byte[] fileData = null;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
        }
        return fileData;
    }

}

