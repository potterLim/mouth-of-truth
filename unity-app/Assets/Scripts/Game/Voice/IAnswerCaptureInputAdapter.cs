using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Voice
{
    public interface IAnswerCaptureInputAdapter
    {
        bool RequiresManualTextEntry { get; }

        string TranscriptPlaceholderText { get; }

        void Reset();

        void BeginCollection();

        void PauseCollection();

        void ResumeCollection();

        void CancelCollection();

        AnswerCaptureFrameSnapshot Update(SecondsDuration deltaTime);

        Task<AnswerCaptureResult> CompleteCollectionAsync(QuestionId questionId, CancellationToken cancellationToken);
    }
}
