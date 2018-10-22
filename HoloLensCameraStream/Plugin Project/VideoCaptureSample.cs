//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Perception.Spatial;
using Windows.Graphics.Imaging;
using Windows.Media.Capture.Frames;


/* Available GUIDS from the acquired frame (refer https://www.magnumdb.com)
 * ?                                                    {C9BA19A9-0F8A-432F-AEDE-BA44D7F38AB7}:System.Byte[]
 * MFSampleExtension_Interlaced                         {B1D5830A-DEB8-40E3-90FA-389943716461}:1
 * MFSampleExtension_BottomFieldFirst                   {941CE0A3-6AE3-4DDA-9A08-A64298340617}:1
 * ?                                                    {1D120EF0-CFC4-4E49-B64C-DDC371776022}:System.Byte[]
 * MFSampleExtension_Spatial_CameraCoordinateSystem     {9D13C82F-2199-4E67-91CD-D1A4181F2534}:Windows.Perception.Spatial.SpatialCoordinateSystem
 * ?                                                    {0B404D45-3042-4F52-9BA5-B1DAB2D02508}:25586972113
 * MFSampleExtension_CameraExtrinsics                   {6B761658-B7EC-4C3B-8225-8623CABEC31D}:System.Byte[]
 * MFSampleExtension_CleanPoint                         {9CDF01D8-A0F0-43BA-B077-EAA06CBD728A}:1
 * MFSampleExtension_DeviceReferenceSystemTime          {6523775A-BA2D-405F-B2C5-01FF88E2E8F6}:251941698121
 * MFSampleExtension_Spatial_CameraViewTransform        {4E251FA4-830F-4770-859A-4B8D99AA809B}:System.Byte[]
 * MFSampleExtension_PinholeCameraIntrinsics            {4EE3B6C5-6A15-4E72-9761-70C1DB8B9FE3}:System.Byte[]
 * MFSampleExtension_Spatial_CameraProjectionTransform  {47F9FCB5-2A02-4F26-A477-792FDF95886A}:System.Byte[]
 * ?                                                    {66A3D7D5-F91B-42BB-AE55-B4DB6F98FCD6}:System.Byte[]
 * ?                                                    {137E6B95-4CAD-4CB9-9B7F-65CEF2068A41}:0
 * ?                                                    {C4139297-2CEC-47C6-9CDF-6DB62EE6DF72}:2
 * ?                                                    {429F001F-BC30-4BAC-AF7F-91C024D1D974}:System.Byte[]
 */

namespace HoloLensCameraStream
{
    public class VideoCaptureSample
    {
        /// <summary>
        /// The guid for getting the view transform from the frame sample.
        /// See https://developer.microsoft.com/en-us/windows/mixed-reality/locatable_camera#locating_the_device_camera_in_the_world
        /// </summary>
        static Guid viewTransformGuid = new Guid("4E251FA4-830F-4770-859A-4B8D99AA809B");

        /// <summary>
        /// The guid for getting the projection transform from the frame sample.
        /// See https://developer.microsoft.com/en-us/windows/mixed-reality/locatable_camera#locating_the_device_camera_in_the_world
        /// </summary>
        static Guid projectionTransformGuid = new Guid("47F9FCB5-2A02-4F26-A477-792FDF95886A");

        /// <summary>
        /// The guid for getting the camera coordinate system for the frame sample.
        /// See https://developer.microsoft.com/en-us/windows/mixed-reality/locatable_camera#locating_the_device_camera_in_the_world
        /// </summary>
        static Guid cameraCoordinateSystemGuid = new Guid("9D13C82F-2199-4E67-91CD-D1A4181F2534");

        /// <summary>
        /// The guid for getting the camera intrinsics for the frame sample (see https://www.magnumdb.com)
        /// </summary>
        static Guid cameraIntrinsicsGuid = new Guid("4EE3B6C5-6A15-4E72-9761-70C1DB8B9FE3");


        /// <summary>
        /// How many bytes are in the frame.
        /// There are four bytes per pixel, times the width and height of the bitmap.
        /// </summary>
        public int dataLength
        {
            get
            {
                return 4 * bitmap.PixelHeight * bitmap.PixelWidth;
            }
        }

        /// <summary>
        /// Note: This method has not been written. Help us out on GitHub!
        /// Will be true if the HoloLens knows where it is and is tracking.
        /// Indicates that obtaining the matrices will be successful.
        /// </summary>
        public bool hasLocationData
        {
            get
            {
                //TODO: Return if location data exists.
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The format of the frames that the bitmap stream is sending.
        /// </summary>
        public CapturePixelFormat pixelFormat { get; private set; }

        /// <summary>
        /// The camera intrinsics
        /// </summary>
        public CameraIntrinsics cameraIntrinsics { get; private set; }

        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }

        //Internal members

        internal SpatialCoordinateSystem worldOrigin { get; private set; }

        internal SoftwareBitmap bitmap { get; private set; }

        internal bool isBitmapCopied { get; private set; }

        //Private members

        MediaFrameReference frameReference;

        internal VideoCaptureSample(MediaFrameReference frameReference, SpatialCoordinateSystem worldOrigin)
        {
            if (frameReference == null)
            {
                throw new ArgumentNullException("frameReference.");
            }

            this.frameReference = frameReference;
            this.worldOrigin = worldOrigin;

            // When Windows.Media.Devices.Core.CameraIntrinsics is out of prerelease, use this instead
            //cameraIntrinsics = new CameraIntrinsics(frameReference.VideoMediaFrame.CameraIntrinsics);

            byte[] rawIntrinsics = frameReference.Properties[cameraIntrinsicsGuid] as byte[];
            float[] intrinsicArray = ConvertByteArrayToFloatArray(rawIntrinsics);
            cameraIntrinsics = new CameraIntrinsics(intrinsicArray);

            bitmap = frameReference.VideoMediaFrame.SoftwareBitmap;
            FrameWidth = bitmap.PixelWidth;
            FrameHeight = bitmap.PixelHeight;
        }

        /// <summary>
        /// If you need safe, long term control over the image bytes in this frame, they will need to be
        /// copied. You need to supply a byte[] to copy them into. It is best to pre-allocate and reuse
        /// this byte array to minimize unecessarily high memory ceiling or unnecessary garbage collections.
        /// </summary>
        /// <param name="byteBuffer">A byte array with a length the size of VideoCaptureSample.dataLength</param>
        public void CopyRawImageDataIntoBuffer(byte[] byteBuffer)
        {
            //Here is a potential way to get direct access to the buffer:
            //http://stackoverflow.com/questions/25481840/how-to-change-mediacapture-to-byte

            if (byteBuffer == null)
            {
                throw new ArgumentNullException("byteBuffer");
            }

            if (byteBuffer.Length < dataLength)
            {
                throw new IndexOutOfRangeException("Your byteBuffer is not big enough." +
                    " Please use the VideoCaptureSample.dataLength property to allocate a large enough array.");
            }

            bitmap.CopyToBuffer(byteBuffer.AsBuffer());
            isBitmapCopied = true;
        }


        public void CopyRawImageDataIntoBuffer(List<byte> byteBuffer)
        {
            throw new NotSupportedException("This method is not yet supported with a List<byte>. Please provide a byte[] instead.");
        }

        /// <summary>
        /// This returns the transform matrix at the time the photo was captured, if location data if available.
        /// If it's not, that is probably an indication that the HoloLens is not tracking and its location is not known.
        /// It could also mean the VideoCapture stream is not running.
        /// If location data is unavailable then the camera to world matrix will be set to the identity matrix.
        /// </summary>
        /// <param name="matrix">The transform matrix used to convert between coordinate spaces.
        /// The matrix will have to be converted to a Unity matrix before it can be used by methods in the UnityEngine namespace.
        /// See https://forum.unity3d.com/threads/locatable-camera-in-unity.398803/ for details.</param>
        public bool TryGetCameraToWorldMatrix(out float[] outMatrix)
        {
            if (frameReference.Properties.ContainsKey(viewTransformGuid) == false)
            {
                outMatrix = GetIdentityMatrixFloatArray();
                return false;
            }

            if (worldOrigin == null)
            {
                outMatrix = GetIdentityMatrixFloatArray();
                return false;
            }

            Matrix4x4 cameraViewTransform = ConvertByteArrayToMatrix4x4(frameReference.Properties[viewTransformGuid] as byte[]);
            if (cameraViewTransform == null)
            {
                outMatrix = GetIdentityMatrixFloatArray();
                return false;
            }

            SpatialCoordinateSystem cameraCoordinateSystem = frameReference.Properties[cameraCoordinateSystemGuid] as SpatialCoordinateSystem;
            if (cameraCoordinateSystem == null)
            {
                outMatrix = GetIdentityMatrixFloatArray();
                return false;
            }

            Matrix4x4? cameraCoordsToUnityCoordsMatrix = cameraCoordinateSystem.TryGetTransformTo(worldOrigin);
            if (cameraCoordsToUnityCoordsMatrix == null)
            {
                outMatrix = GetIdentityMatrixFloatArray();
                return false;
            }

            // Transpose the matrices to obtain a proper transform matrix
            cameraViewTransform = Matrix4x4.Transpose(cameraViewTransform);
            Matrix4x4 cameraCoordsToUnityCoords = Matrix4x4.Transpose(cameraCoordsToUnityCoordsMatrix.Value);

            Matrix4x4 viewToWorldInCameraCoordsMatrix;
            Matrix4x4.Invert(cameraViewTransform, out viewToWorldInCameraCoordsMatrix);
            Matrix4x4 viewToWorldInUnityCoordsMatrix = Matrix4x4.Multiply(cameraCoordsToUnityCoords, viewToWorldInCameraCoordsMatrix);

            // Change from right handed coordinate system to left handed UnityEngine
            viewToWorldInUnityCoordsMatrix.M31 *= -1f;
            viewToWorldInUnityCoordsMatrix.M32 *= -1f;
            viewToWorldInUnityCoordsMatrix.M33 *= -1f;
            viewToWorldInUnityCoordsMatrix.M34 *= -1f;

            outMatrix = ConvertMatrixToFloatArray(viewToWorldInUnityCoordsMatrix);

            return true;
        }

        /// <summary>
        /// This returns the projection matrix at the time the photo was captured, if location data if available.
        /// If it's not, that is probably an indication that the HoloLens is not tracking and its location is not known.
        /// It could also mean the VideoCapture stream is not running.
        /// If location data is unavailable then the projecgtion matrix will be set to the identity matrix.
        /// </summary>
        /// <param name="matrix">The projection matrix used to match the true camera projection.
        /// The matrix will have to be converted to a Unity matrix before it can be used by methods in the UnityEngine namespace.
        /// See https://forum.unity3d.com/threads/locatable-camera-in-unity.398803/ for details.</param>
        public bool TryGetProjectionMatrix(out float[] outMatrix)
        {
            if (frameReference.Properties.ContainsKey(projectionTransformGuid) == false)
            {
                outMatrix = GetIdentityMatrixFloatArray();
                return false;
            }

            Matrix4x4 projectionMatrix = ConvertByteArrayToMatrix4x4(frameReference.Properties[projectionTransformGuid] as byte[]);

            // Transpose matrix to match expected Unity format
            projectionMatrix = Matrix4x4.Transpose(projectionMatrix);
            outMatrix = ConvertMatrixToFloatArray(projectionMatrix);
            return true;
        }

        /// <summary>
        /// Note: This method hasn't been written yet. Help us out on GitHub!
        /// </summary>
        /// <param name="targetTexture"></param>
        public void UploadImageDataToTexture(object targetTexture)
        {
            //TODO: Figure out how to use a Texture2D in a plugin.
            throw new NotSupportedException("I'm not sure how to use a Texture2D within this plugin.");
        }

        /// <summary>
        /// When done with the VideoCapture class, you will need to dispose it to release unmanaged memory.
        /// </summary>
        public void Dispose()
        {
            bitmap.Dispose();
            frameReference.Dispose();
        }

        private float[] ConvertMatrixToFloatArray(Matrix4x4 matrix)
        {
            return new float[16] {
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44 };
        }

        private float[] ConvertByteArrayToFloatArray(byte[] values)
        {
            if (values.Length < 4 || values.Length%4 != 0)
            {
                throw new ArgumentException("Expected byte array to at least 4 bytes and divisible by 4. Array was " + values.Length + " bytes");
            }

            float[] outputArr = new float[values.Length / 4];
            for (int i=0;i<outputArr.Length;i++)
            {
                outputArr[i] = BitConverter.ToSingle(values, i * 4);
            }
            return outputArr;
        }

        private Matrix4x4 ConvertByteArrayToMatrix4x4(byte[] matrixAsBytes)
        {
            if (matrixAsBytes == null)
            {
                throw new ArgumentNullException("matrixAsBytes");
            }

            if (matrixAsBytes.Length != 64)
            {
                throw new Exception("Cannot convert byte[] to Matrix4x4. Size of array should be 64, but it is " + matrixAsBytes.Length);
            }

            var m = matrixAsBytes;
            return new Matrix4x4(
                BitConverter.ToSingle(m, 0),
                BitConverter.ToSingle(m, 4),
                BitConverter.ToSingle(m, 8),
                BitConverter.ToSingle(m, 12),
                BitConverter.ToSingle(m, 16),
                BitConverter.ToSingle(m, 20),
                BitConverter.ToSingle(m, 24),
                BitConverter.ToSingle(m, 28),
                BitConverter.ToSingle(m, 32),
                BitConverter.ToSingle(m, 36),
                BitConverter.ToSingle(m, 40),
                BitConverter.ToSingle(m, 44),
                BitConverter.ToSingle(m, 48),
                BitConverter.ToSingle(m, 52),
                BitConverter.ToSingle(m, 56),
                BitConverter.ToSingle(m, 60));
        }

        static CapturePixelFormat ConvertBitmapPixelFormatToCapturePixelFormat(BitmapPixelFormat format)
        {
            switch (format)
            {
                case BitmapPixelFormat.Bgra8:
                    return CapturePixelFormat.BGRA32;
                case BitmapPixelFormat.Nv12:
                    return CapturePixelFormat.NV12;
                default:
                    return CapturePixelFormat.Unknown;
            }
        }

        static byte[] GetIdentityMatrixByteArray()
        {
            return new byte[] { 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1, 0, 0, 0, 0, 1 };
        }

        static float[] GetIdentityMatrixFloatArray()
        {
            return new float[] { 1f, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 1f, 0, 0, 0, 0, 1f };
        }
    }
}
