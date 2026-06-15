using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        private void LateUpdate()
        {
            ensureAmbiencePlayback();
            stabilizeAudioSourceLevels();
            updateHandPromptPanelDismissal();
            updateAnsweringPresentation();
            updateAnalyzingPresentation();
        }

        private void updateAnsweringPresentation()
        {
            if (mIsAnsweringPresentationActive == false || mMouthImage == null)
            {
                return;
            }

            float elapsedSeconds = Time.unscaledTime - mAnsweringPresentationStartedAtSeconds;
            float slowPulse = (Mathf.Sin(elapsedSeconds * 1.85f) + 1.0f) * 0.5f;
            float quickPulse = (Mathf.Sin(elapsedSeconds * 4.8f) + 1.0f) * 0.5f;
            float breathScale = Mathf.Lerp(1.0f, 1.035f, slowPulse);
            setOverlayTint(new Color(0.018f, 0.012f, 0.010f, 1.0f), Mathf.Lerp(0.30f, 0.36f, slowPulse));

            if (isTempleApproachSceneActive())
            {
                float cameraPulse = slowPulse * 0.024f;
                setTempleCameraPoseCenteredOnMouth(TEMPLE_ANSWER_FOCUS_SCALE + cameraPulse, TEMPLE_MOUTH_FOCUS_CENTER);
                setTempleApproachMouthColor(new Color(1.0f, Mathf.Lerp(0.93f, 1.0f, slowPulse), Mathf.Lerp(0.78f, 0.94f, slowPulse), 1.0f));
                syncTempleStageMouthOverlay(0.0f);
                updateAnsweringEyeBeamImages(elapsedSeconds, quickPulse);
                return;
            }

            mMouthImage.color = new Color(1.0f, Mathf.Lerp(0.93f, 1.0f, slowPulse), Mathf.Lerp(0.78f, 0.94f, slowPulse), 1.0f);
            mMouthImage.rectTransform.localScale = Vector3.one * breathScale;
            updateAnsweringEyeBeamImages(elapsedSeconds, quickPulse);
        }

        private void updateAnalyzingPresentation()
        {
            if (mIsAnalyzingPresentationActive == false || mMouthImage == null)
            {
                return;
            }

            float elapsedSeconds = Time.unscaledTime - mAnalyzingPresentationStartedAtSeconds;
            float pulse = (Mathf.Sin(elapsedSeconds * 3.4f) + 1.0f) * 0.5f;
            float surge = (Mathf.Sin(elapsedSeconds * 5.8f) + 1.0f) * 0.5f;
            float tremor = Mathf.Sin(elapsedSeconds * 10.5f) * 6.6f;
            float verticalTremor = Mathf.Sin(elapsedSeconds * 13.2f) * 2.4f;
            float focusProgress = Mathf.Clamp01(elapsedSeconds / ANALYSIS_FOCUS_RAMP_SECONDS);
            setOverlayTint(new Color(0.024f, 0.016f, 0.012f, 1.0f), Mathf.Lerp(0.34f, 0.40f, pulse));

            if (isTempleApproachSceneActive())
            {
                float focusScale = Mathf.Lerp(TEMPLE_ANSWER_FOCUS_SCALE, TEMPLE_ANALYSIS_FOCUS_SCALE, easeOut(focusProgress));
                float cameraScale = focusScale + (pulse * 0.026f);
                setTempleCameraPoseCenteredOnMouth(cameraScale, TEMPLE_MOUTH_FOCUS_CENTER, tremor * 0.94f, verticalTremor * 0.68f);
                setTempleApproachMouthColor(new Color(1.0f, Mathf.Lerp(0.86f, 0.98f, pulse), Mathf.Lerp(0.74f, 0.90f, pulse), Mathf.Lerp(0.92f, 1.0f, pulse)));
                syncTempleStageMouthOverlay(0.0f);
                updateMouthEffectImage(mMouthListeningAuraImage, new Color(1.0f, 0.64f, 0.40f, Mathf.Lerp(0.12f, 0.20f, pulse)), 1.10f + (surge * 0.12f), -elapsedSeconds * 9.0f);
                updateMouthEffectImage(mMouthAnalyzingAuraImage, new Color(1.0f, 0.34f, 0.26f, Mathf.Lerp(0.18f, 0.29f, surge)), 1.20f + (pulse * 0.16f), elapsedSeconds * 14.0f);
                return;
            }

            mMouthImage.color = new Color(1.0f, Mathf.Lerp(0.86f, 0.98f, pulse), Mathf.Lerp(0.74f, 0.90f, pulse), Mathf.Lerp(0.74f, 0.92f, pulse));
            mMouthImage.rectTransform.anchoredPosition = new Vector2(tremor * 1.34f, verticalTremor * 0.72f);
            mMouthImage.rectTransform.localScale = Vector3.one * (Mathf.Lerp(1.02f, 1.13f, easeOut(focusProgress)) + (pulse * 0.035f));
            updateMouthEffectImage(mMouthListeningAuraImage, new Color(1.0f, 0.64f, 0.40f, Mathf.Lerp(0.12f, 0.20f, pulse)), 1.10f + (surge * 0.12f), -elapsedSeconds * 9.0f);
            updateMouthEffectImage(mMouthAnalyzingAuraImage, new Color(1.0f, 0.34f, 0.26f, Mathf.Lerp(0.18f, 0.29f, surge)), 1.20f + (pulse * 0.16f), elapsedSeconds * 14.0f);
        }

        private async Task playMouthJudgementFocusTransitionAsync()
        {
            if (isTempleApproachSceneActive())
            {
                TempleCameraScale targetCameraScale = new TempleCameraScale(TEMPLE_ANSWER_FOCUS_SCALE);
                TempleCameraTransition templeCameraTransition = captureTempleCameraTransition(targetCameraScale, TEMPLE_MOUTH_FOCUS_CENTER);

                await animateOverTimeAsync(
                    new SecondsDuration(MOUTH_JUDGEMENT_FOCUS_SECONDS * 1.28f),
                    progress =>
                    {
                        float easedProgress = easeInOut(progress);
                        float pulse = Mathf.Sin(progress.Value * Mathf.PI);
                        NormalizedProgress cameraProgress = NormalizedProgress.FromUnclamped(easedProgress);
                        float cameraScale = templeCameraTransition.GetScale(targetCameraScale, cameraProgress) + (pulse * 0.018f);
                        Vector2 cameraPosition = templeCameraTransition.GetPosition(cameraProgress);
                        setTempleCameraPose(cameraScale, cameraPosition.y, cameraPosition.x);
                        setOverlayTint(new Color(0.025f, 0.015f, 0.012f, 1.0f), Mathf.Lerp(0.36f, 0.46f, easedProgress));
                        setTempleApproachMouthColor(new Color(1.0f, Mathf.Lerp(0.96f, 0.92f, easedProgress), Mathf.Lerp(0.86f, 0.78f, easedProgress), 1.0f));
                    });

                syncTempleStageMouthOverlay(0.0f);
                return;
            }

            RectTransform mouthRectTransform = mMouthImage.rectTransform;
            Vector2 startAnchor = mouthRectTransform.anchorMin;
            Vector2 startPosition = mouthRectTransform.anchoredPosition;
            Vector2 startSize = mouthRectTransform.sizeDelta;
            Vector3 startScale = mouthRectTransform.localScale;
            Color startColor = mMouthImage.color;

            await animateOverTimeAsync(
                new SecondsDuration(MOUTH_JUDGEMENT_FOCUS_SECONDS * 1.28f),
                progress =>
                {
                    float easedProgress = easeInOut(progress);
                    float pulse = Mathf.Sin(progress.Value * Mathf.PI);
                    Vector2 currentAnchor = Vector2.Lerp(startAnchor, ANSWERING_FOCUS_MOUTH_ANCHOR, easedProgress);
                    mouthRectTransform.anchorMin = currentAnchor;
                    mouthRectTransform.anchorMax = currentAnchor;
                    mouthRectTransform.anchoredPosition = Vector2.Lerp(startPosition, Vector2.zero, easedProgress);
                    mouthRectTransform.sizeDelta = Vector2.Lerp(startSize, ANSWERING_FOCUS_MOUTH_SIZE_PIXELS, easedProgress);
                    mouthRectTransform.localScale = Vector3.Lerp(startScale, Vector3.one, easedProgress) * (1.0f + (pulse * 0.035f));
                    setOverlayTint(new Color(0.025f, 0.015f, 0.012f, 1.0f), Mathf.Lerp(0.36f, 0.46f, easedProgress));
                    mMouthImage.color = Color.Lerp(startColor, new Color(1.0f, 0.92f, 0.78f, 0.98f), easedProgress);
                });
        }
    }
}
