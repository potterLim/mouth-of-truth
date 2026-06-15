using MouthOfTruth.Game.Data;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        private SecondsDuration getHandPromptPanelHoldDuration()
        {
            float handPromptPanelHoldSeconds = mHandPromptClip != null
                ? Mathf.Max(0.55f, mHandPromptClip.length)
                : HAND_PROMPT_PANEL_FALLBACK_HOLD_SECONDS;

            return new SecondsDuration(handPromptPanelHoldSeconds);
        }

        private void hideHandPromptPanelImmediately()
        {
            mIsHandPromptPanelDismissalActive = false;
            setObjectActive(mQuestionPanelImage, false);
            setObjectActive(mQuestionText, false);
            resetHandPromptPanelAlpha();
        }

        private void hideHandPromptPanelAfterFade()
        {
            mIsHandPromptPanelDismissalActive = false;
            setHandPromptPanelAlpha(0.0f);
            setObjectActive(mQuestionPanelImage, false);
            setObjectActive(mQuestionText, false);
        }

        private void resetHandPromptPanelAlpha()
        {
            mIsHandPromptPanelDismissalActive = false;
            mHandPromptPanelDismissalStartAlpha = 1.0f;
            setHandPromptPanelAlpha(1.0f);
        }

        private void updateHandPromptPanelDismissal()
        {
            if (mIsHandPromptPanelDismissalActive == false)
            {
                return;
            }

            float elapsedSeconds = Time.unscaledTime - mHandPromptPanelDismissalStartedAtSeconds;
            float progress = Mathf.Clamp01(elapsedSeconds / HAND_PROMPT_PANEL_DISMISS_SECONDS);
            float easedProgress = easeOut(progress);
            setHandPromptPanelAlpha(Mathf.Lerp(mHandPromptPanelDismissalStartAlpha, 0.0f, easedProgress));

            if (progress < 1.0f)
            {
                return;
            }

            hideHandPromptPanelAfterFade();
        }

        private float getHandPromptPanelAlpha()
        {
            if (mQuestionPanelImage != null)
            {
                return mQuestionPanelImage.color.a;
            }

            return mQuestionText != null ? mQuestionText.color.a : 0.0f;
        }

        private void setHandPromptPanelAlpha(float alpha)
        {
            float clampedAlpha = Mathf.Clamp01(alpha);

            if (mQuestionPanelImage != null)
            {
                Color panelColor = mQuestionPanelImage.color;
                mQuestionPanelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, clampedAlpha);
            }

            if (mQuestionText != null)
            {
                Color textColor = mQuestionText.color;
                mQuestionText.color = new Color(textColor.r, textColor.g, textColor.b, clampedAlpha);
            }
        }
    }
}
