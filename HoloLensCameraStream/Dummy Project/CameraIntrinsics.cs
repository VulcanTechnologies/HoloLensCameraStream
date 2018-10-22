namespace HoloLensCameraStream
{
    public struct CameraIntrinsics
    {

        public double FocalLengthX;//focal length x.
        public double FocalLengthY;//focal length y.
        public double PrincipalPointX;//principal point x.
        public double PrincipalPointY;//principal point y.
        public double RadialDistK1;//radial distortion coefficient k1.
        public double RadialDistK2;//radial distortion coefficient k2.
        public double TangentialDistP1;//tangential distortion coefficient p1.
        public double TangentialDistP2;//tangential distortion coefficient p2.
        public double RadialDistK3;//radial distortion coefficient k3.
    }
}