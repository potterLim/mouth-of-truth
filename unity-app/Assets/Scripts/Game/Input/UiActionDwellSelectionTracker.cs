using System;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Input
{
    public class UiActionDwellSelectionTracker
    {
        private readonly SecondsDuration mRequiredDwellDuration;

        private EUiActionTarget? mHoveredUiActionTargetOrNull;
        private SecondsDuration mHoveredDuration;

        public UiActionDwellSelectionTracker()
            : this(new SecondsDuration(0.7f))
        {
        }

        public UiActionDwellSelectionTracker(SecondsDuration requiredDwellDuration)
        {
            if (requiredDwellDuration.Value <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredDwellDuration));
            }

            mRequiredDwellDuration = requiredDwellDuration;
        }

        public SecondsDuration HoveredDuration => mHoveredDuration;

        public EUiActionTarget? UpdateHoveredTargetOrNull(EUiActionTarget? hoveredUiActionTargetOrNull, SecondsDuration deltaTimeDuration)
        {
            if (hoveredUiActionTargetOrNull == null)
            {
                Reset();
                return null;
            }

            if (mHoveredUiActionTargetOrNull != hoveredUiActionTargetOrNull)
            {
                mHoveredUiActionTargetOrNull = hoveredUiActionTargetOrNull;
                mHoveredDuration = SecondsDuration.Zero;
            }

            mHoveredDuration = mHoveredDuration.Add(deltaTimeDuration);

            if (mHoveredDuration.Value < mRequiredDwellDuration.Value)
            {
                return null;
            }

            EUiActionTarget confirmedUiActionTarget = hoveredUiActionTargetOrNull.Value;
            Reset();
            return confirmedUiActionTarget;
        }

        public void Reset()
        {
            mHoveredUiActionTargetOrNull = null;
            mHoveredDuration = SecondsDuration.Zero;
        }
    }
}
