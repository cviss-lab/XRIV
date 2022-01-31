using System;
using UnityEngine;
using UnityEngine.Windows.WebCam;
#if UNITY_WSA && !UNITY_EDITOR // RUNNING ON WINDOWS
using Windows.Media.Capture.Frames;
using Windows.Media.Devices.Core;
#endif
public class CameraProjection : MonoBehaviour

{
    [HideInInspector]
    public bool useMediaCaptureProjection;
    public bool useCalibratedProjection;
    public float fx;
    public float fy;
    public float cx;
    public float cy;
    public int cameraWidth;
    public int cameraHeight;
    public static CameraProjection instance;
    private Camera cam;
    [HideInInspector]
    public Matrix4x4 cameraToWorldMatrix;
    private void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    void Start()
    {
        if (useCalibratedProjection)
        {
            Camera.main.projectionMatrix = OpenCV2OpenGL();
        }
    }

    public Matrix4x4 OpenCV2OpenGL()
    {

#if UNITY_EDITOR // RUNNING ON UNITY
        cx = cameraWidth/2;
        cy = cameraHeight/2;
#endif

        float znear = cam.nearClipPlane; // Near clipping distance
        float zfar = cam.farClipPlane; // Far clipping distance
        Matrix4x4 pM = new Matrix4x4
        {
            m00 = 2.0f * fx / cameraWidth,
            m10 = 0.0f,
            m20 = 0.0f,
            m30 = 0.0f,

            m01 = 0.0f,
            m11 = 2.0f * fy / cameraHeight,
            m21 = 0.0f,
            m31 = 0.0f,

            m02 = 1.0f - 2.0f * cx / cameraWidth,
            m12 = 2.0f * cy / cameraHeight - 1.0f,
            m22 = (zfar + znear) / (znear - zfar),
            m32 = -1.0f,

            m03 = 0.0f,
            m13 = 0.0f,
            m23 = 2.0f * zfar * znear / (znear - zfar),
            m33 = 0.0f
        };

        return pM;
    }

    public Matrix4x4 OpenCVMatrix()
    {

#if UNITY_EDITOR // RUNNING ON UNITY
        cx = cameraWidth/2;
        cy = cameraHeight/2;
#endif

        Matrix4x4 pM = new Matrix4x4
        {
            m00 = fx,
            m10 = 0.0f,
            m20 = 0.0f,
            m30 = 0.0f,

            m01 = 0.0f,
            m11 = fy,
            m21 = 0.0f,
            m31 = 0.0f,

            m02 = cx,
            m12 = cameraHeight-cy,
            m22 = 1.0f,
            m32 = 0.0f,

            m03 = 0.0f,
            m13 = 0.0f,
            m23 = 0.0f,
            m33 = 0.0f
        };

        return pM;
    }

    public Vector3 ScreenToWorldPoint(Vector3 sp2)
    {
        Ray ray = ScreenPointToRay(sp2);
        return ray.GetPoint(sp2.z);
    }

    public Ray ScreenPointToRay(Vector3 sp)
    {
        Ray ray1 = cam.ScreenPointToRay(sp);
        Vector3 position = cameraToWorldMatrix.MultiplyPoint(Vector3.zero);
        Vector3 raydir = cameraToWorldMatrix.rotation * (cam.transform.worldToLocalMatrix.rotation * ray1.direction);
        return new Ray(position, raydir);
    }


    

}


