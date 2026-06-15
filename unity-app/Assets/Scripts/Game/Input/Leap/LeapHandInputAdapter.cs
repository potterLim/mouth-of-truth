namespace MouthOfTruth.Game.Input.Leap
{
    public class LeapHandInputAdapter : IHandInteractionInputAdapter, IHandInteractionFallbackGate
    {
        private readonly LeapHandTrackingRuntime mLeapHandTrackingRuntime;

        public LeapHandInputAdapter()
        {
            mLeapHandTrackingRuntime = LeapHandTrackingRuntime.EnsureInstance();
        }

        public bool ShouldSuppressFallbackInput =>
            mLeapHandTrackingRuntime != null && mLeapHandTrackingRuntime.ShouldOwnPointerInput;

        public bool TryGetPointerScreenPosition(out PointerScreenPosition pointerScreenPosition)
        {
            if (mLeapHandTrackingRuntime == null)
            {
                pointerScreenPosition = default;
                return false;
            }

            return mLeapHandTrackingRuntime.TryGetPointerScreenPosition(out pointerScreenPosition);
        }

        public bool WasReturnToTitleTriggeredThisFrame()
        {
            return false;
        }
    }
}
