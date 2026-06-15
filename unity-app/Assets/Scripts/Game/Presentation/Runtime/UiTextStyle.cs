using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    internal readonly struct UiTextStyle
    {
        public UiTextStyle(int fontSize, FontStyle fontStyle)
        {
            FontSize = fontSize;
            FontStyle = fontStyle;
        }

        public int FontSize
        {
            get;
        }

        public FontStyle FontStyle
        {
            get;
        }
    }
}
