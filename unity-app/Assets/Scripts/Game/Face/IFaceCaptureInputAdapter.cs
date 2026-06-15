using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Face
{
    public interface IFaceCaptureInputAdapter
    {
        bool HasAvailableDevice();

        void Reset();

        void BeginCollection(QuestionId questionId);

        void PauseCollection();

        void ResumeCollection();

        void CancelCollection();

        void Update(SecondsDuration deltaTime);

        Task<FaceCaptureResult> CompleteCollectionAsync(CancellationToken cancellationToken);
    }
}
