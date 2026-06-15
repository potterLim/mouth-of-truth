using System;
using System.IO;
using MouthOfTruth.Game.App;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Face
{
    public static class FaceFrameWorkspacePaths
    {
        private const string WORKSPACE_DIRECTORY_NAME = "session-workspace";
        private const string FACE_DIRECTORY_NAME = "face-frames";

        public static string GetWorkspaceDirectoryPath()
        {
            return Path.Combine(MouthOfTruthRuntimePaths.GetPythonEngineRootPath(), "data", WORKSPACE_DIRECTORY_NAME);
        }

        public static string GetFaceFramesDirectoryPath()
        {
            return Path.Combine(GetWorkspaceDirectoryPath(), FACE_DIRECTORY_NAME);
        }

        public static FaceFramesDirectoryPath BuildCaptureDirectoryPath(QuestionId questionId)
        {
            string sanitizedQuestionId = string.IsNullOrWhiteSpace(questionId.Value)
                ? "question"
                : questionId.Value.Trim().Replace(" ", "_");
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return new FaceFramesDirectoryPath(Path.Combine(GetFaceFramesDirectoryPath(), $"{sanitizedQuestionId}_{timestamp}"));
        }
    }
}
