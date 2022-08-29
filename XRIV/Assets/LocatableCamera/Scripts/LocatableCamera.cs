// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.WebCam;

namespace Microsoft.MixedReality.OpenXR.BasicSample
{
    public class LocatableCamera : MonoBehaviour
    {

        [SerializeField]
        private Shader textureShader = null;

        [SerializeField]
        private TextMesh text = null;

        public int TimeOut = 1;
        public bool autoCapture = false;
        public bool displayPhoto = false;
        public bool loadToTexture = true;
        public string debugImagePath = "";

        [HideInInspector]
        public Vector3 position;

        [HideInInspector]
        public Quaternion rotation;

        [HideInInspector]
        public Quaternion rotation2;

        [HideInInspector]
        GameObject quad;

        [HideInInspector]
        public bool readyToCapture = false;

        [HideInInspector]
        public uint numPhotos = 0;

        [HideInInspector]
        public Texture2D targetTexture;

        [HideInInspector]
        public UnityEvent imageCapturedEvent;

        [HideInInspector]
        public Matrix4x4 cameraToWorldMatrix;

        private PhotoCapture photoCaptureObject = null;
        private Resolution cameraResolution = default(Resolution);
        private Camera cam;

        public static LocatableCamera instance;

        private async void Awake()
        {
            instance = this;

        }

        private void Start()
        {
            cam = Camera.main;

            imageCapturedEvent = new UnityEvent();
            if (displayPhoto)
            {
                quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

            }
            else
            {
                quad = new GameObject();
            }
            quad.transform.parent = transform;

#if !UNITY_EDITOR // INITIALIZE CAMERA ON MR DEVICE
            initializePhotoCaptureCamera();
#endif

        }



        public void initializePhotoCaptureCamera()
        {

            var resolutions = PhotoCapture.SupportedResolutions;
            if (resolutions == null || resolutions.Count() == 0)
            {
                if (text != null)
                {
                    text.text = "Resolutions not available. Did you provide web cam access?";
                }
                return;
            }
            cameraResolution = resolutions.OrderByDescending((res) => res.width * res.height).First();

            PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);

            if (text != null)
            {
                text.text = "Starting camera...";
            }
            targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);

            readyToCapture = true;
        }

        private void OnDestroy()
        {

            if (photoCaptureObject != null)
            {
                photoCaptureObject.StopPhotoModeAsync(OnPhotoCaptureStopped);

                if (text != null)
                {
                    text.text = "Stopping camera...";
                }
            }
        }

        private void OnPhotoCaptureCreated(PhotoCapture captureObject)
        {
            if (text != null)
            {
                text.text += "\nPhotoCapture created...";
            }

            photoCaptureObject = captureObject;

            CameraParameters cameraParameters = new CameraParameters(WebCamMode.PhotoMode)
            {
                hologramOpacity = 0.0f,
                cameraResolutionWidth = cameraResolution.width,
                cameraResolutionHeight = cameraResolution.height,
                pixelFormat = CapturePixelFormat.BGRA32
            };

            captureObject.StartPhotoModeAsync(cameraParameters, OnPhotoModeStarted);
        }

        private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
        {
            if (result.success)
            {

                if (text != null)
                {
                    text.text = "Ready!\nPress the above button to take a picture.";
                }
            }
            else
            {

                if (text != null)
                {
                    text.text = "Unable to start photo mode!";
                }
            }
        }

        /// <summary>
        /// Takes a photo and attempts to load it into the scene using its location data.
        /// </summary>
        public void TakePhoto()
        {

#if UNITY_EDITOR // PHOTOCAPTURE IN EDITOR
            targetTexture = new Texture2D(1, 1);

            if (debugImagePath.Length > 0)
            {
                targetTexture.LoadImage(LoadPNG(debugImagePath));
            }
            else
            {
                virtualPhoto();
            }
            virtualLocatableCamera();

#else // CAPTURE IMAGE FROM MR DEVICE
        if (!readyToCapture)
        {
            return;
        }

        readyToCapture = false;

        if (text != null)
        {
            if (autoCapture)
            {
                text.text = "Auto Capture Enabled!...";
                text.text += "\nTaking picture...";
            }
            else
            {
                text.text = "Taking picture...";
            }
        }

        photoCaptureObject.TakePhotoAsync(OnPhotoCaptured);

#endif
        }


        private void OnPhotoCaptured(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
        {

            if (result.success)
            {
                if (text != null)
                {
                    text.text = "\nTook picture!";
                }


                float ratio = cameraResolution.height / (float)cameraResolution.width;
                quad.transform.localScale = new Vector3(1.0f, ratio, 1.0f) / 5.0f;

                if (loadToTexture)
                {
                    photoCaptureFrame.UploadImageDataToTexture(targetTexture);
                }


                if (photoCaptureFrame.hasLocationData)
                {
                    photoCaptureFrame.TryGetCameraToWorldMatrix(out cameraToWorldMatrix);
                    Quaternion base2cam = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 90, 0);

                    position = cameraToWorldMatrix.MultiplyPoint(Vector3.zero);
                    rotation2 = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));
                    rotation = rotation2 * base2cam;

                    transform.position = position;
                    //transform.rotation = rotation;
                    transform.rotation = rotation2;

                    photoCaptureFrame.TryGetProjectionMatrix(cam.nearClipPlane, cam.farClipPlane, out Matrix4x4 projectionMatrix);

                    targetTexture.wrapMode = TextureWrapMode.Clamp;

                    if (displayPhoto)
                    {
                        Renderer quadRenderer = quad.GetComponent<Renderer>();
                        quadRenderer.material = new Material(textureShader);
                        quadRenderer.sharedMaterial.SetTexture("_MainTex", targetTexture);
                        quadRenderer.sharedMaterial.SetMatrix("_WorldToCameraMatrix", cameraToWorldMatrix.inverse);
                        quadRenderer.sharedMaterial.SetMatrix("_CameraProjectionMatrix", projectionMatrix);
                        //quadRenderer.material.color = new Color(1f, 1f, 1f, 0.5f);                        
                        quad.transform.localPosition = new Vector3(0.25f, -0.25f, 0.5f);
                    }

                    imageCapturedEvent.Invoke();

                    if (text != null)
                    {
                        text.text += $"\nPosition: ({position.x}, {position.y}, {position.z})";
                        text.text += $"\nRotation: ({rotation.x}, {rotation.y}, {rotation.z}, {rotation.w})";
                    }

                    if (ClickGlobal.currentlyAnalyzing)
                    {
                        return; //ignore frame, still analyzing
                    }

                    List<byte> byteBuffer = new List<byte>();
                    byteBuffer.Clear();
                    photoCaptureFrame.CopyRawImageDataIntoBuffer(byteBuffer);
                    CameraProjection.instance.cameraToWorldMatrix = transform.localToWorldMatrix;
                    NetworkManager.instance.imageBytes = byteBuffer.ToArray();
                    ClickGlobal.startImageAnalysis = true;

                }
                else
                {
                    if (text != null)
                    {
                        text.text += "\nNo location data :(";
                    }
                }
            }
            else
            {
                if (text != null)
                {
                    text.text = "Picture taking failed: " + result.hResult;
                }
            }

            readyToCapture = true;
            if (autoCapture)
            {
                StartCoroutine(Sleeper((float)TimeOut));
                TakePhoto();
            }

        }

        public void StartAutoCapture()
        {
            if (!autoCapture)
            {
                autoCapture = true;
                TakePhoto();
            }
            else
            {
                autoCapture = false;
                text.text += "\nAuto Capture Disabled!";
            }
        }


        private void OnPhotoCaptureStopped(PhotoCapture.PhotoCaptureResult result)
        {
            if (text != null)
            {
                text.text = result.success ? "Photo mode stopped." : "Unable to stop photo mode.";
            }

            photoCaptureObject.Dispose();
            photoCaptureObject = null;
        }

        private IEnumerator Sleeper(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
        public static Byte[] Compress(Byte[] buffer)
        {
            Byte[] compressedByte;
            using (MemoryStream ms = new MemoryStream())
            {
                using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
                {
                    ds.Write(buffer, 0, buffer.Length);
                }

                compressedByte = ms.ToArray();
            }

            return compressedByte;
        }

        public void virtualPhoto()
        {

            Rect rect = new Rect(0, 0, cam.pixelWidth, cam.pixelHeight);
            RenderTexture renderTexture = new RenderTexture(cam.pixelWidth, cam.pixelHeight, 24);
            targetTexture = new Texture2D(cam.pixelWidth, cam.pixelHeight, TextureFormat.RGBA32, false);

            cam.targetTexture = renderTexture;
            cam.nearClipPlane = 1000;
            cam.Render();
            cam.nearClipPlane = 0.1f;

            RenderTexture.active = renderTexture;

            targetTexture.ReadPixels(rect, 0, 0);
            targetTexture.Apply();

            //targetTexture = GetSquareCenteredTexture(targetTexture, (int)CameraProjection.instance.cameraWidth, (int)CameraProjection.instance.cameraHeight);


            cam.targetTexture = null;
            RenderTexture.active = null;

        }

        void virtualLocatableCamera()
        {

            Quaternion base2cam = Quaternion.Euler(90, 0, 0) * Quaternion.Euler(0, 90, 0);
            cameraToWorldMatrix = cam.cameraToWorldMatrix;
            position = cameraToWorldMatrix.MultiplyPoint(Vector3.zero);
            rotation2 = Quaternion.LookRotation(-cameraToWorldMatrix.GetColumn(2), cameraToWorldMatrix.GetColumn(1));
            rotation = rotation2 * base2cam;
            transform.position = position;
            //transform.rotation = rotation;
            transform.rotation = rotation2;
            targetTexture.wrapMode = TextureWrapMode.Clamp;

            if (displayPhoto)
            {
                Renderer quadRenderer = quad.GetComponent<Renderer>();
                quadRenderer.material = new Material(textureShader);
                quadRenderer.sharedMaterial.SetTexture("_MainTex", targetTexture);
                quad.transform.localPosition = new Vector3(0.25f, -0.25f, 0.5f);
            }


            if (text != null)
            {
                text.text += $"\nPosition: ({position.x}, {position.y}, {position.z})";
                text.text += $"\nRotation: ({rotation.x}, {rotation.y}, {rotation.z}, {rotation.w})";
            }

            CameraProjection.instance.cameraToWorldMatrix = transform.localToWorldMatrix;

            if (debugImagePath.Length > 0)
            {
                NetworkManager.instance.imageBytes = LoadPNG(debugImagePath);
            }
            ClickGlobal.startImageAnalysis = true;
            imageCapturedEvent.Invoke();

            if (autoCapture)
            {
                StartCoroutine(Sleeper((float)TimeOut));
                TakePhoto();
            }
        }


        public static byte[] LoadPNG(string filePath)
        {

            byte[] fileData = null;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
            }
            return fileData;
        }

        private Texture2D GetSquareCenteredTexture(Texture2D sourceTexture, int newWidth, int newHeight)
        {
            int xPos = (sourceTexture.width - newWidth) / 2;
            int yPos = (sourceTexture.height - newHeight) / 2;

            Color[] c = ((Texture2D)sourceTexture).GetPixels(xPos, yPos, newWidth, newHeight);
            Texture2D croppedTexture = new Texture2D(newWidth, newHeight);
            croppedTexture.SetPixels(c);
            croppedTexture.Apply();
            return croppedTexture;
        }
    }

}
