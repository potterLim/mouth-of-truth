using System;
using System.IO;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        [Serializable]
        private sealed class TutorialSequenceMetadata
        {
            // JsonUtility maps fields by the Lottie sequence metadata keys.
            public float fr = 0.0f;
            public float ip = 0.0f;
            public float op = 0.0f;
        }

        public async Task PlayFirstRunTutorialAsync()
        {
            SecondsDuration firstRunTutorialDuration = getFirstRunTutorialDuration();
            SecondsDuration tutorialDuration = new SecondsDuration(firstRunTutorialDuration.Value * FIRST_RUN_TUTORIAL_DURATION_SCALE);
            IsFirstRunTutorialVisible = true;
            configureExitButtonAsTopLeftIcon();
            setObjectVisibility(mTutorialOverlayImage, EUiElementVisibility.Visible);
            setObjectVisibility(mTutorialDevicePanelImage, EUiElementVisibility.Visible);
            setObjectVisibility(mTutorialLeapMotionDeviceImage, EUiElementVisibility.Visible);
            setObjectVisibility(mTutorialHandImage, EUiElementVisibility.Visible);
            setObjectVisibility(mTutorialTitleText, EUiElementVisibility.Visible);
            setObjectVisibility(mTutorialBodyText, EUiElementVisibility.Visible);
            setObjectVisibility(mTutorialStepText, EUiElementVisibility.Hidden);
            setObjectVisibility(mStartButton, EUiElementVisibility.Hidden);
            setObjectVisibility(mTryAgainButton, EUiElementVisibility.Hidden);
            setObjectVisibility(mBackToTitleButton, EUiElementVisibility.Hidden);
            setObjectVisibility(mExitButton, EUiElementVisibility.Visible);
            mTutorialOverlayImage.color = new Color(0.078f, 0.080f, 0.090f, 1.0f);
            mTutorialDevicePanelImage.color = new Color(0.145f, 0.148f, 0.162f, 0.97f);
            mTutorialOverlayImage.transform.SetAsLastSibling();
            mTutorialDevicePanelImage.transform.SetAsLastSibling();
            mTutorialLeapMotionDeviceImage.transform.SetAsLastSibling();
            mTutorialHandImage.transform.SetAsLastSibling();
            mTutorialTitleText.transform.SetAsLastSibling();
            mTutorialBodyText.transform.SetAsLastSibling();
            mExitButton.transform.SetAsLastSibling();
            setText(mTutorialTitleText, "손을 장치 위 30cm 높이에서 천천히 움직여 주세요");
            setText(mTutorialBodyText, "손끝 방향으로 버튼과 카드를 가리키고, 같은 위치에 잠시 머물면 선택됩니다.");
            RectTransform handRectTransform = mTutorialHandImage.rectTransform;

            await animateOverTimeAsync(
                tutorialDuration,
                progress =>
                {
                    float progressValue = progress.Value;
                    float firstSegmentProgress = Mathf.Clamp01(progressValue / 0.48f);
                    float secondSegmentProgress = Mathf.Clamp01((progressValue - 0.48f) / 0.52f);
                    Vector2 hoverStartPosition = new Vector2(0.0f, -130.0f);
                    Vector2 hoverReadyPosition = new Vector2(0.0f, -18.0f);
                    handRectTransform.anchoredPosition = progressValue < 0.48f
                        ? Vector2.Lerp(hoverStartPosition, hoverReadyPosition, easeOut(firstSegmentProgress))
                        : getTutorialScanPosition(secondSegmentProgress);
                    handRectTransform.localScale = Vector3.one * Mathf.Lerp(0.78f, 1.02f, easeOut(firstSegmentProgress));
                    mTutorialOverlayImage.color = new Color(0.078f, 0.080f, 0.090f, 1.0f);
                });

            hideStartScreenPresentationBehindTutorial();
        }

        private void hideFirstRunTutorialPresentation()
        {
            setObjectVisibility(mTutorialOverlayImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialDevicePanelImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialLeapMotionDeviceImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialHandImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialTitleText, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialBodyText, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialStepText, EUiElementVisibility.Hidden);
            IsFirstRunTutorialVisible = false;
        }

        private void hideStartScreenPresentationBehindTutorial()
        {
            setObjectVisibility(mLogoImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTitleVignetteImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mStartButton, EUiElementVisibility.Hidden);
        }

        private static Vector2 getTutorialScanPosition(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            Vector2 centerPosition = new Vector2(0.0f, -18.0f);
            Vector2 leftPosition = new Vector2(-220.0f, -20.0f);
            Vector2 rightPosition = new Vector2(220.0f, -20.0f);

            if (clampedProgress < 0.28f)
            {
                return Vector2.Lerp(centerPosition, leftPosition, easeInOut(clampedProgress / 0.28f));
            }

            if (clampedProgress < 0.65f)
            {
                return Vector2.Lerp(leftPosition, rightPosition, easeInOut((clampedProgress - 0.28f) / 0.37f));
            }

            return Vector2.Lerp(rightPosition, centerPosition, easeInOut((clampedProgress - 0.65f) / 0.35f));
        }

        private static SecondsDuration getFirstRunTutorialDuration()
        {
            try
            {
                RuntimeAssetFilePath firstRunTutorialSequencePath = MouthOfTruthAssetCatalog.FirstRunTutorialSequencePath;

                if (File.Exists(firstRunTutorialSequencePath.Value) == false)
                {
                    return new SecondsDuration(FIRST_RUN_TUTORIAL_FALLBACK_DURATION_SECONDS);
                }

                string jsonText = File.ReadAllText(firstRunTutorialSequencePath.Value);
                TutorialSequenceMetadata metadata = JsonUtility.FromJson<TutorialSequenceMetadata>(jsonText);

                if (metadata == null || metadata.fr <= 0.0f || metadata.op <= metadata.ip)
                {
                    return new SecondsDuration(FIRST_RUN_TUTORIAL_FALLBACK_DURATION_SECONDS);
                }

                return new SecondsDuration(Mathf.Clamp((metadata.op - metadata.ip) / metadata.fr, 3.0f, 5.0f));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to read first-run tutorial sequence metadata.\n" + exception);
                return new SecondsDuration(FIRST_RUN_TUTORIAL_FALLBACK_DURATION_SECONDS);
            }
        }
    }
}
