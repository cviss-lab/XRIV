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
using NumericsConversion;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine.XR.WSA;
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
    public string UnityTestImage;
    public static bool startImageAnalysis = false;
    public static bool currentlyAnalyzing = false;
#if UNITY_WSA && !UNITY_EDITOR
    public VideoFrameReader CameraVideoFrameReader = new VideoFrameReader();

#endif

    private async void Awake()
    {
        instance = this;

#if UNITY_WSA && !UNITY_EDITOR // RUNNING ON WINDOWS
        await CameraVideoFrameReader.Inititalize();
# endif
    }



    public void ClickPhoto()
    {
#if UNITY_WSA && !UNITY_EDITOR  // RUNNING ON WINDOWS
        CameraVideoFrameReader.currentlyCapturing = true;
#elif UNITY_EDITOR              // RUNNING IN UNITY
        startImageAnalysis = true;
#endif
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

#if UNITY_EDITOR // RUNNING ON UNITY        
        NetworkManager.instance.imageBytes = LoadJPG(UnityTestImage);
        CameraProjection.instance.cameraToWorldMatrix = Camera.main.transform.localToWorldMatrix;
#endif
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

