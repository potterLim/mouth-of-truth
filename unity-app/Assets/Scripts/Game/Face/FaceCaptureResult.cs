namespace MouthOfTruth.Game.Face
{
    public class FaceCaptureResult
    {
        public FaceCaptureResult(FaceFramesDirectoryPath faceFramesDirectoryPath, FaceFrameCount capturedFrameCount)
        {
            FaceFramesDirectoryPath = faceFramesDirectoryPath;
            CapturedFrameCount = capturedFrameCount;
        }

        public FaceFramesDirectoryPath FaceFramesDirectoryPath { get; }

        public FaceFrameCount CapturedFrameCount { get; }
    }
}
