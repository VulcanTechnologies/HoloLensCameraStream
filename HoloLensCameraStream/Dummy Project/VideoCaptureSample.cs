//  
// Copyright (c) 2017 Vulcan, Inc. All rights reserved.  
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
//

using System;
using System.Collections.Generic;

namespace HoloLensCameraStream
{
    public class VideoCaptureSample
    {
        public int dataLength { get; private set; }

        public bool hasLocationData { get; private set; }

        public CapturePixelFormat pixelFormat { get; private set; }

        public CameraIntrinsics cameraIntrinsics { get; private set; }

        public int FrameWidth { get; private set; }

        public int FrameHeight { get; private set; }

        public void CopyRawImageDataIntoBuffer(byte[] byteBuffer)
        {
            throw new NotImplementedException();
        }
        
        public void CopyRawImageDataIntoBuffer(List<byte> byteBuffer)
        {
            throw new NotImplementedException();
        }

        public bool TryGetCameraToWorldMatrix(out float[] outMatrix)
        {
            throw new NotImplementedException();
        }

        public bool TryGetProjectionMatrix(out float[] outMatrix)
        {
            throw new NotImplementedException();
        }

        public void UploadImageDataToTexture(object targetTexture)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
