using UnityEngine;

namespace MouthOfTruth.Game.Input
{
    public interface IHandInteractionInputAdapter
    {
        bool TryGetPointerScreenPosition(out PointerScreenPosition pointerScreenPosition);

        bool WasReturnToTitleTriggeredThisFrame();
    }
}
