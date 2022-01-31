#if UNITY_WSA && !UNITY_EDITOR // RUNNING ON WINDOWS
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Perception.Spatial;
#endif
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using NumericsConversion;
using UnityEngine.XR.WSA;
using UnityEngine.XR;

[StructLayout(LayoutKind.Sequential)]
struct HolographicFrameNativeData
{
    public uint VersionNumber;
    public uint MaxNumberOfCameras;
    public IntPtr ISpatialCoordinateSystemPtr; // Windows::Perception::Spatial::ISpatialCoordinateSystem
    public IntPtr IHolographicFramePtr; // Windows::Graphics::Holographic::IHolographicFrame 
    public IntPtr IHolographicCameraPtr; // // Windows::Graphics::Holographic::IHolographicCamera
}

namespace NumericsConversion
{
    public static class NumericsConversionExtensions
    {
        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 v) => new UnityEngine.Vector3(v.X, v.Y, -v.Z);
        public static UnityEngine.Quaternion ToUnity(this System.Numerics.Quaternion q) => new UnityEngine.Quaternion(-q.X, -q.Y, q.Z, q.W);
        public static UnityEngine.Matrix4x4 ToUnity(this System.Numerics.Matrix4x4 m) => new UnityEngine.Matrix4x4(
            new Vector4(m.M11, m.M12, -m.M13, m.M14),
            new Vector4(m.M21, m.M22, -m.M23, m.M24),
            new Vector4(-m.M31, -m.M32, m.M33, -m.M34),
            new Vector4(m.M41, m.M42, -m.M43, m.M44));

        public static System.Numerics.Vector3 ToSystem(this UnityEngine.Vector3 v) => new System.Numerics.Vector3(v.x, v.y, -v.z);
        public static System.Numerics.Quaternion ToSystem(this UnityEngine.Quaternion q) => new System.Numerics.Quaternion(-q.x, -q.y, q.z, q.w);
        public static System.Numerics.Matrix4x4 ToSystem(this UnityEngine.Matrix4x4 m) => new System.Numerics.Matrix4x4(
            m.m00, m.m10, -m.m20, m.m30,
            m.m01, m.m11, -m.m21, m.m31,
           -m.m02, -m.m12, m.m22, -m.m32,
            m.m03, m.m13, -m.m23, m.m33);
    }
}

#if UNITY_WSA && !UNITY_EDITOR
public class VideoFrameReader    
{

    private MediaCapture CameraCapture;
    private MediaFrameReader CameraFrameReader;
    public MediaFrameReference frameReference;
    public bool currentlyCapturing = false;
    private SpatialCoordinateSystem worldOrigin;

    private Int64 FramesCaptured;
    public int mode = 0;

    public VideoFrameReader()
    {
    }

    public async Task Inititalize() //(IUnityScanScene unityApp)
    {

        currentlyCapturing = false;
        await InitializeCameraCapture();
        await InitializeCameraFrameReader();

        IntPtr nativePtr = XRDevice.GetNativePtr();
        HolographicFrameNativeData hfd = Marshal.PtrToStructure<HolographicFrameNativeData>(nativePtr);
        worldOrigin = Marshal.GetObjectForIUnknown(WorldManager.GetNativeISpatialCoordinateSystemPtr()) as SpatialCoordinateSystem;
    }

    private async Task InitializeCameraCapture()
    {
        CameraCapture = new MediaCapture();
        MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
        
        settings.StreamingCaptureMode = StreamingCaptureMode.Video;
        settings.MemoryPreference = MediaCaptureMemoryPreference.Cpu;
        
        await CameraCapture.InitializeAsync(settings);
    }
       
    private async Task InitializeCameraFrameReader()
    {
        var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync(); 
        MediaFrameSourceGroup selectedGroup = null;
        MediaFrameSourceInfo colorSourceInfo = null;

        foreach (var sourceGroup in frameSourceGroups)
        {
            foreach (var sourceInfo in sourceGroup.SourceInfos)
            {
                if (sourceInfo.MediaStreamType == MediaStreamType.VideoPreview
                    && sourceInfo.SourceKind == MediaFrameSourceKind.Color)
                {
                    colorSourceInfo = sourceInfo;
                    break;
                }
            }
            if (colorSourceInfo != null)
            {
                selectedGroup = sourceGroup;
                break;
            }
        }

        var colorFrameSource = CameraCapture.FrameSources[colorSourceInfo.Id];
        var preferredFormat = colorFrameSource.SupportedFormats.Where(format =>
        {
            return format.Subtype == MediaEncodingSubtypes.Argb32;

        }).FirstOrDefault();

        CameraFrameReader = await CameraCapture.CreateFrameReaderAsync(colorFrameSource);
        await CameraFrameReader.StartAsync();
        CameraFrameReader.FrameArrived += OnFrameArrived;
    }

    public async Task Dispose()
    {
        // clean up unmanaged resources
        await CameraFrameReader.StopAsync();
        CameraFrameReader.FrameArrived -= OnFrameArrived;
        CameraCapture.Dispose();
        CameraCapture = null;
    }

    public async Task StopRecordingAsync()
    {
        // Stop processing frames
        CameraFrameReader.FrameArrived -= OnFrameArrived;
    }

    public async Task StartRecordingAsync()
    {
        // Start processing frames
        currentlyCapturing = false;
        CameraFrameReader.FrameArrived += OnFrameArrived;
    }

    private void OnFrameArrived(MediaFrameReader CameraFrameReader, MediaFrameArrivedEventArgs args)
    {


        if (!currentlyCapturing)
        {
            return; //ignore frame
        }

        if (mode == 0)
        {
            if (ClickGlobal.currentlyAnalyzing)
            {
                return; //ignore frame, still analyzing
            }
        }

        currentlyCapturing = false;

        using (frameReference = CameraFrameReader.TryAcquireLatestFrame())
        {
            if (frameReference == null)
            {
                currentlyCapturing = true;
                return; //ignore frame, get next frame
            }
            using (VideoFrame videoFrame = frameReference.VideoMediaFrame.GetVideoFrame())
            {
                if (videoFrame == null)
                {
                    currentlyCapturing = true;
                    return; //ignore frame, get next frame
                }
                if (videoFrame.SoftwareBitmap == null)
                {
                    currentlyCapturing = true;
                    return; //ignore frame, get next frame
                }
                SpatialCoordinateSystem cameraCoordinateSystem = frameReference.CoordinateSystem;
                System.Numerics.Matrix4x4? sceneToWorld = cameraCoordinateSystem.TryGetTransformTo(worldOrigin);
                CameraProjection.instance.cameraToWorldMatrix = sceneToWorld.Value.ToUnity();
                NetworkManager.instance.imageBytes = Task.Run(() => convertFrameToByteArrayAsync(videoFrame)).GetAwaiter().GetResult();
                ClickGlobal.startImageAnalysis = true;
            };
        }        
    }

    public async Task<byte[]> convertFrameToByteArrayAsync(VideoFrame videoFrame, string filename = null)
    {
  
        using (SoftwareBitmap bitmap = videoFrame.SoftwareBitmap)
        {
            byte[] bytes = await EncodedBytes(bitmap, BitmapEncoder.JpegEncoderId);

            if (bytes == null)
            {
                Debug.Log("Bytes returned are null!");
            }

            if (filename != null)
            {
                await SaveSoftwareBitmapAsync(bitmap, filename);
            }

            return bytes;
        }
    }

    private async Task SaveSoftwareBitmapAsync(SoftwareBitmap bitmap, String filename)
    {
        using (var outputStream = File.Create(filename))
        {
            using (IRandomAccessStream randomAccessStream = outputStream.AsRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, randomAccessStream);
                encoder.SetSoftwareBitmap(SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Bgra8));
                await encoder.FlushAsync();
            }   
        }

    }

    private async Task<byte[]> EncodedBytes(SoftwareBitmap soft, Guid encoderId)
    {                

        using (var ms = new InMemoryRandomAccessStream())
        {
            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(encoderId, ms);
            encoder.SetSoftwareBitmap(SoftwareBitmap.Convert(soft, BitmapPixelFormat.Bgra8));
            await encoder.FlushAsync();
            byte[] array = new byte[ms.Size];
            await ms.ReadAsync(array.AsBuffer(), (uint)ms.Size, InputStreamOptions.None);
            return array;
        }           
    }
        
    public void StartPullCameraFrames()
    {
        Task.Run(async () =>
        {
            for (; ; ) 
            {
                FramesCaptured++;
                using (var frameReference = CameraFrameReader.TryAcquireLatestFrame())
                using (var videoFrame = frameReference?.VideoMediaFrame?.GetVideoFrame())
                {

                    if (frameReference == null)
                    {
                        continue; 
                    }

                    if (videoFrame == null)
                    {
                        continue;
                    }

                    if (videoFrame.Direct3DSurface == null)
                    {
                        videoFrame.Dispose();
                        continue;                     
                    }
                    
                }

            }

        });
    }

}
#endif


