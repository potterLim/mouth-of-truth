using UnityEngine;

namespace MouthOfTruth.Game.Input.Keyboard
{
    public class KeyboardHandInputAdapter : IHandInteractionInputAdapter
    {
        private readonly KeyCode mReturnToTitleKeyCode;

        public KeyboardHandInputAdapter()
            : this(KeyCode.Backspace)
        {
        }

        public KeyboardHandInputAdapter(KeyCode returnToTitleKeyCode)
        {
            mReturnToTitleKeyCode = returnToTitleKeyCode;
        }

        public bool TryGetPointerScreenPosition(out PointerScreenPosition pointerScreenPosition)
        {
            pointerScreenPosition = new PointerScreenPosition(UnityEngine.Input.mousePosition);
            return true;
        }

        public bool WasReturnToTitleTriggeredThisFrame()
        {
            return UnityEngine.Input.GetKeyDown(mReturnToTitleKeyCode);
        }
    }
}
