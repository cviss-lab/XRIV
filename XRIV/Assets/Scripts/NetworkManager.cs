using JsonConversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
//using static VisionNameSpace;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public string ipAddress;
    public string port;
    [HideInInspector]
    public string visionAnalysisEndpoint;
    internal byte[] imageBytes;

    //internal string imagePath;
    private void Awake()
    {
        instance = this;
    }

    public IEnumerator AnalyseLastImageCaptured()
    {

        visionAnalysisEndpoint = "http://" + ipAddress + ":" + port;

        WWWForm webForm = new WWWForm();
        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(visionAnalysisEndpoint, webForm))
        {
            //yield return new WaitForSeconds(2);
            System.DateTime t1 = System.DateTime.Now;

            //imageBytes = GetImageAsByteArray(imagePath);
            unityWebRequest.SetRequestHeader("Content-Type", "application/octet-stream");
            unityWebRequest.SetRequestHeader("tapcount", ClickGlobal.tapsCount.ToString());
            unityWebRequest.SetRequestHeader("cameraToWorldMatrix", CameraProjection.instance.cameraToWorldMatrix.ToString().Replace("\n", ",").Replace("\t", ","));
            unityWebRequest.SetRequestHeader("intrinsicMatrix", CameraProjection.instance.OpenCVMatrix().ToString().Replace("\n", ",").Replace("\t", ","));

            // segmentation analysis
            if (ClickGlobal.instance.analysisType == 0)
            {
                unityWebRequest.SetRequestHeader("Request-Type", "InteractSegment");
                unityWebRequest.SetRequestHeader("posList", findCoordinates3D(DrawLines.instance.posList));
                unityWebRequest.SetRequestHeader("negList", findCoordinates3D(DrawLines.instance.negList));
            }
            // marker analysis
            if (ClickGlobal.instance.analysisType == 1)
            {
                unityWebRequest.SetRequestHeader("Request-Type", "DetectMarker");
            }

            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            unityWebRequest.uploadHandler = new UploadHandlerRaw(imageBytes);
            unityWebRequest.uploadHandler.contentType = "application/octet-stream";
            yield return unityWebRequest.SendWebRequest();

            if (unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
            {
                Debug.Log(unityWebRequest.error);
                ClickGlobal.instance.finishedCapturing();
                yield break;
            }
            else 
            {
                long responseCode = unityWebRequest.responseCode;
                string jsonResponse = unityWebRequest.downloadHandler.text;                
                AnalysedObject analysedObject = JsonUtility.FromJson<AnalysedObject>(jsonResponse);
                if (analysedObject == null)
                {
                    Debug.Log("No Objects Detected!");
                }

                ResultsLabel.instance.SetObjectLabels(analysedObject);

                yield break;
            }            
        }
    }


    public IEnumerator CalculateArea(CurveList curveList)
    {

        WWWForm webForm = new WWWForm();

        using (UnityWebRequest unityWebRequest = UnityWebRequest.Post(NetworkManager.instance.visionAnalysisEndpoint, webForm))
        {
            unityWebRequest.SetRequestHeader("Content-Type", "application/json");
            unityWebRequest.SetRequestHeader("Request-Type", "CalculateArea");
            unityWebRequest.SetRequestHeader("cameraToWorldMatrix", CameraProjection.instance.cameraToWorldMatrix.ToString().Replace("\n", ",").Replace("\t", ","));
            unityWebRequest.SetRequestHeader("tapcount", ClickGlobal.tapsCount.ToString());

            unityWebRequest.downloadHandler = new DownloadHandlerBuffer();
            byte[] bodyRaw = Encoding.UTF8.GetBytes(JsonUtility.ToJson(curveList));
            unityWebRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);

            unityWebRequest.uploadHandler.contentType = "application/string";
            yield return unityWebRequest.SendWebRequest();

            if(unityWebRequest.isNetworkError || unityWebRequest.isHttpError)
            {
                Debug.Log(unityWebRequest.error);
                curveList = null;
                ClickGlobal.instance.finishedCapturing();
                yield break;
            }
            else
            {
                long responseCode = unityWebRequest.responseCode;
                string jsonResponse = unityWebRequest.downloadHandler.text;
                Area areaObject = JsonUtility.FromJson<Area>(jsonResponse);

                if (responseCode == 500)
                {
                    Debug.Log("Area Could not be Calculated!");
                }
                ResultsLabel.instance.placeAreaLabel(areaObject);
                ClickGlobal.instance.finishedCapturing();
                yield break;
            }       
        }
    }

    public static byte[] GetImageAsByteArray(string imageFilePath)
    {
        FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new BinaryReader(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }
    private string findCoordinates3D(List<LineRenderer> pointList)
    {
        string outString = "";
        foreach (LineRenderer p in pointList)
        {
            if (p != null)
            {
                Vector3 pos = p.GetPosition(1);
                outString += string.Format(@"{0},{1},{2}", pos.x, pos.y, pos.z) + ",";
            }
        }
        return outString;
    }
    private string findCoordinates2D(List<LineRenderer> pointList)
    {
        Camera.main.worldToCameraMatrix = CameraProjection.instance.cameraToWorldMatrix.inverse;
        //Matrix4x4 matrix = Camera.main.projectionMatrix * CameraProjection.instance.cameraToWorldMatrix.inverse;
        string outString = "";
        foreach(LineRenderer p in pointList)
        {
            if (p != null)
            {
                Vector3 pos = p.GetPosition(1);
                //Vector3 screenPos = matrix.MultiplyPoint(pos);
                //// convert from clip space (-1, 1) to opencv image coordinates
                //screenPos.x = (1f - screenPos.x) * .5f * CameraProjection.instance.cameraWidth;
                //screenPos.y = (1f + screenPos.y) * .5f * CameraProjection.instance.cameraHeight;
                Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
                screenPos.x = CameraProjection.instance.cameraWidth - screenPos.x * CameraProjection.instance.cameraWidth / Camera.main.pixelWidth;
                screenPos.y = screenPos.y * CameraProjection.instance.cameraHeight / Camera.main.pixelHeight;
                outString += string.Format(@"{0},{1}", screenPos.x, screenPos.y) + ",";
            }
        }
        Camera.main.ResetWorldToCameraMatrix();
        return outString;
    }
}

