using System;
using MouthOfTruth.Game.Data;

namespace MouthOfTruth.Game.Input
{
    public class CardDwellSelectionTracker
    {
        private readonly SecondsDuration mRequiredDwellDuration;

        private EQuestionCardSlot? mHoveredQuestionCardSlotOrNull;
        private SecondsDuration mHoveredDuration;

        public CardDwellSelectionTracker()
            : this(new SecondsDuration(0.7f))
        {
        }

        public CardDwellSelectionTracker(SecondsDuration requiredDwellDuration)
        {
            if (requiredDwellDuration.Value <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredDwellDuration));
            }

            mRequiredDwellDuration = requiredDwellDuration;
        }

        public SecondsDuration HoveredDuration => mHoveredDuration;

        public EQuestionCardSlot? UpdateHoveredCardOrNull(EQuestionCardSlot? hoveredQuestionCardSlotOrNull, SecondsDuration deltaTimeDuration)
        {
            if (hoveredQuestionCardSlotOrNull == null)
            {
                Reset();
                return null;
            }

            if (mHoveredQuestionCardSlotOrNull != hoveredQuestionCardSlotOrNull)
            {
                mHoveredQuestionCardSlotOrNull = hoveredQuestionCardSlotOrNull;
                mHoveredDuration = SecondsDuration.Zero;
            }

            mHoveredDuration = mHoveredDuration.Add(deltaTimeDuration);

            if (mHoveredDuration.Value < mRequiredDwellDuration.Value)
            {
                return null;
            }

            EQuestionCardSlot confirmedQuestionCardSlot = hoveredQuestionCardSlotOrNull.Value;
            Reset();
            return confirmedQuestionCardSlot;
        }

        public void Reset()
        {
            mHoveredQuestionCardSlotOrNull = null;
            mHoveredDuration = SecondsDuration.Zero;
        }
    }
}
