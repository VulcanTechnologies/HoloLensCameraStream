//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using UnityEngine;


using System;

using HoloLensCameraStream;

/// <summary>
/// This example gets the video frames at 30 fps and displays them on a Unity texture,
/// which is locked the User's gaze.
/// </summary>
public class VideoPanelApp : MonoBehaviour
{
    byte[] _latestImageBytes;
    HoloLensCameraStream.Resolution _resolution;

    //"Injected" objects.
    VideoPanel _videoPanelUI;
    VideoCapture _videoCapture;

    IntPtr _spatialCoordinateSystemPtr; 

    void Start()
    {
        //Fetch a pointer to Unity's spatial coordinate system if you need pixel mapping
        _spatialCoordinateSystemPtr = UnityEngine.VR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();

        //Call this in Start() to ensure that the CameraStreamHelper is already "Awake".
        CameraStreamHelper.Instance.GetVideoCaptureAsync(OnVideoCaptureCreated);
        //You could also do this "shortcut":
        //CameraStreamManager.Instance.GetVideoCaptureAsync(v => videoCapture = v);

        _videoPanelUI = GameObject.FindObjectOfType<VideoPanel>();
    }

    private void OnDestroy()
    {
        if (_videoCapture != null)
        {
            _videoCapture.FrameSampleAcquired -= OnFrameSampleAcquired;
            _videoCapture.Dispose();
        }
    }

    void OnVideoCaptureCreated(VideoCapture videoCapture)
    {
        if (videoCapture == null)
        {
            Debug.LogError("Did not find a video capture object. You may not be using the HoloLens.");
            return;
        }
        
        this._videoCapture = videoCapture;

        //Request the spatial coordinate ptr if you want fetch the camera and set it if you need to 
        CameraStreamHelper.Instance.SetNativeISpatialCoordinateSystemPtr(_spatialCoordinateSystemPtr);

        _resolution = CameraStreamHelper.Instance.GetLowestResolution();
        float frameRate = CameraStreamHelper.Instance.GetHighestFrameRate(_resolution);
        videoCapture.FrameSampleAcquired += OnFrameSampleAcquired;

        //You don't need to set all of these params.
        //I'm just adding them to show you that they exist.
        CameraParameters cameraParams = new CameraParameters();
        cameraParams.cameraResolutionHeight = _resolution.height;
        cameraParams.cameraResolutionWidth = _resolution.width;
        cameraParams.frameRate = Mathf.RoundToInt(frameRate);
        cameraParams.pixelFormat = CapturePixelFormat.BGRA32;
        cameraParams.rotateImage180Degrees = true; //If your image is upside down, remove this line.
        cameraParams.enableHolograms = false;

        UnityEngine.WSA.Application.InvokeOnAppThread(() => { _videoPanelUI.SetResolution(_resolution.width, _resolution.height); }, false);

        videoCapture.StartVideoModeAsync(cameraParams, OnVideoModeStarted);
    }

    void OnVideoModeStarted(VideoCaptureResult result)
    {
        if (result.success == false)
        {
            Debug.LogWarning("Could not start video mode.");
            return;
        }

        Debug.Log("Video capture started.");
    }

    void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }

    void OnFrameSampleAcquired(VideoCaptureSample sample)
    {
        //When copying the bytes out of the buffer, you must supply a byte[] that is appropriately sized.
        //You can reuse this byte[] until you need to resize it (for whatever reason).
        if (_latestImageBytes == null || _latestImageBytes.Length < sample.dataLength)
        {
            _latestImageBytes = new byte[sample.dataLength];
        }

        // edit the color of each frame
        sample.CopyRawImageDataIntoBuffer(_latestImageBytes);

        //If you need to get the cameraToWorld matrix for purposes of compositing you can do it like this
        float[] cameraToWorldMatrix;
        if (sample.TryGetCameraToWorldMatrix(out cameraToWorldMatrix) == false)
        {
            return;
        }

        //If you need to get the projection matrix for purposes of compositing you can do it like this
        float[] projectionMatrix;
        if (sample.TryGetProjectionMatrix(out projectionMatrix) == false)
        {
            return;

        }

        sample.Dispose();

        //This is where we actually use the image data
        UnityEngine.WSA.Application.InvokeOnAppThread(() =>
        {
            int f = 10000;
            int PixelSize = 4;
            int width = _resolution.width;
            int height = _resolution.height;
            int Line = width * PixelSize;

            // get normal image from mirror image
            for (int i = 0; i < height; ++i)
                for (int j = 0; j + 4 < Line / 2; j += 4)
                {
                    Swap<byte>(ref _latestImageBytes[Line * i + j], ref _latestImageBytes[Line * i + Line - j - 4]);
                    Swap<byte>(ref _latestImageBytes[Line * i + j + 1], ref _latestImageBytes[Line * i + Line - j - 3]);
                    Swap<byte>(ref _latestImageBytes[Line * i + j + 2], ref _latestImageBytes[Line * i + Line - j - 2]);
                    Swap<byte>(ref _latestImageBytes[Line * i + j + 3], ref _latestImageBytes[Line * i + Line - j - 1]);

                    // reduce the alpha
                    //_latestImageBytes[Line * i + j + 3] = _latestImageBytes[Line * i + Line - j - 1] = 100;
                }


            _videoPanelUI.SetBytes(_latestImageBytes);
        }, true);
    }
}
