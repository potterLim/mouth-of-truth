using System;
using System.IO;
using MouthOfTruth.Game.App;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Voice
{
    public static class AnswerAudioWorkspacePaths
    {
        private const string WORKSPACE_DIRECTORY_NAME = "session-workspace";
        private const string AUDIO_DIRECTORY_NAME = "answer-audio";

        public static string GetWorkspaceDirectoryPath()
        {
            return Path.Combine(MouthOfTruthRuntimePaths.GetPythonEngineRootPath(), "data", WORKSPACE_DIRECTORY_NAME);
        }

        public static string GetAudioDirectoryPath()
        {
            return Path.Combine(GetWorkspaceDirectoryPath(), AUDIO_DIRECTORY_NAME);
        }

        public static AnswerAudioFilePath BuildAudioFilePath(QuestionId questionId)
        {
            string sanitizedQuestionId = string.IsNullOrWhiteSpace(questionId.Value)
                ? "question"
                : questionId.Value.Trim().Replace(" ", "_");
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
            return new AnswerAudioFilePath(Path.Combine(GetAudioDirectoryPath(), $"{sanitizedQuestionId}_{timestamp}.wav"));
        }
    }
}
