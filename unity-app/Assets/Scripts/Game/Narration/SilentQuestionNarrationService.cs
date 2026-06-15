using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Narration
{
    public class SilentQuestionNarrationService : IQuestionNarrationService
    {
        private static readonly SecondsDuration DEFAULT_DELAY_DURATION = new SecondsDuration(1.2f);

        private readonly SecondsDuration mDelayDuration;

        public SilentQuestionNarrationService()
            : this(DEFAULT_DELAY_DURATION)
        {
        }

        public SilentQuestionNarrationService(SecondsDuration delayDuration)
        {
            mDelayDuration = delayDuration;
        }

        public Task SpeakQuestionAsync(QuestionDefinition questionDefinition, CancellationToken cancellationToken)
        {
            int delayMilliseconds = (int)(mDelayDuration.Value * 1000.0f);
            return Task.Delay(delayMilliseconds, cancellationToken);
        }
    }
}
