using System.Collections.Generic;
using UnityEngine;

namespace MouthOfTruth.Game.Input
{
    public class CompositeHandInteractionInputAdapter : IHandInteractionInputAdapter
    {
        private readonly IReadOnlyList<IHandInteractionInputAdapter> mInputAdapters;

        public CompositeHandInteractionInputAdapter(params IHandInteractionInputAdapter[] inputAdapters)
        {
            mInputAdapters = inputAdapters == null ? new IHandInteractionInputAdapter[0] : inputAdapters;
        }

        public bool TryGetPointerScreenPosition(out PointerScreenPosition pointerScreenPosition)
        {
            foreach (IHandInteractionInputAdapter inputAdapter in mInputAdapters)
            {
                if (inputAdapter == null)
                {
                    continue;
                }

                if (inputAdapter.TryGetPointerScreenPosition(out pointerScreenPosition))
                {
                    return true;
                }

                if (inputAdapter is IHandInteractionFallbackGate fallbackGate && fallbackGate.ShouldSuppressFallbackInput)
                {
                    pointerScreenPosition = default;
                    return false;
                }
            }

            pointerScreenPosition = default;
            return false;
        }

        public bool WasReturnToTitleTriggeredThisFrame()
        {
            foreach (IHandInteractionInputAdapter inputAdapter in mInputAdapters)
            {
                if (inputAdapter != null && inputAdapter.WasReturnToTitleTriggeredThisFrame())
                {
                    return true;
                }
            }

            return false;
        }
    }
}
