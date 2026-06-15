using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    internal readonly struct UiRectLayout
    {
        public static UiRectLayout Fill =>
            new UiRectLayout(Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        public static UiRectLayout At(Vector2 anchor, Vector2 sizeDelta)
        {
            return At(anchor, Vector2.zero, sizeDelta);
        }

        public static UiRectLayout At(Vector2 anchor, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            return new UiRectLayout(anchor, anchor, anchoredPosition, sizeDelta);
        }

        public static UiRectLayout Stretched(Vector2 sizeDelta)
        {
            return new UiRectLayout(Vector2.zero, Vector2.one, Vector2.zero, sizeDelta);
        }

        private UiRectLayout(Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            AnchorMin = anchorMin;
            AnchorMax = anchorMax;
            AnchoredPosition = anchoredPosition;
            SizeDelta = sizeDelta;
        }

        public Vector2 AnchorMin
        {
            get;
        }

        public Vector2 AnchorMax
        {
            get;
        }

        public Vector2 AnchoredPosition
        {
            get;
        }

        public Vector2 SizeDelta
        {
            get;
        }
    }
}
