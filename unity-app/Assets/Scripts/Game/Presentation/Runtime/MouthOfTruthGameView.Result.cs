using System.Threading.Tasks;
using MouthOfTruth.Game.Analysis;
using MouthOfTruth.Game.Data;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        public async Task PlayAnalysisCompleteTransitionAsync()
        {
            if (mIsAnalyzingPresentationActive == false)
            {
                return;
            }

            if (isTempleApproachSceneActive())
            {
                TempleCameraScale targetCameraScale = new TempleCameraScale(TEMPLE_RESULT_FOCUS_SCALE);
                TempleCameraTransition templeCameraTransition = captureTempleCameraTransition(targetCameraScale, TEMPLE_MOUTH_FOCUS_CENTER);

                await animateOverTimeAsync(
                    new SecondsDuration(0.54f),
                    progress =>
                    {
                        float progressValue = progress.Value;
                        float easedProgress = easeInOut(progress);
                        float pulse = Mathf.Sin(progressValue * Mathf.PI);
                        float shakeFalloff = 1.0f - easedProgress;
                        float residualShake = Mathf.Sin(progressValue * Mathf.PI * 7.0f) * shakeFalloff * 4.2f;
                        float residualVerticalShake = Mathf.Sin(progressValue * Mathf.PI * 8.0f) * shakeFalloff * 1.8f;
                        NormalizedProgress cameraProgress = NormalizedProgress.FromUnclamped(easedProgress);
                        float cameraScale = templeCameraTransition.GetScale(targetCameraScale, cameraProgress) + (pulse * 0.020f);
                        Vector2 cameraPosition = templeCameraTransition.GetPosition(cameraProgress);
                        setTempleCameraPose(cameraScale, cameraPosition.y + residualVerticalShake, cameraPosition.x + residualShake);
                        setOverlayTint(new Color(0.022f, 0.016f, 0.014f, 1.0f), Mathf.Lerp(0.39f, 0.38f, easedProgress));
                        setTempleApproachMouthColor(new Color(1.0f, 0.94f, 0.84f, Mathf.Lerp(0.90f, 1.0f, easedProgress)));

                        if (mMouthListeningAuraImage != null)
                        {
                            Color listeningAuraColor = mMouthListeningAuraImage.color;
                            mMouthListeningAuraImage.color = new Color(
                                listeningAuraColor.r,
                                listeningAuraColor.g,
                                listeningAuraColor.b,
                                Mathf.Lerp(listeningAuraColor.a, 0.0f, easedProgress));
                        }

                        if (mMouthAnalyzingAuraImage != null)
                        {
                            Color auraColor = mMouthAnalyzingAuraImage.color;
                            mMouthAnalyzingAuraImage.color = new Color(auraColor.r, auraColor.g, auraColor.b, Mathf.Lerp(auraColor.a, 0.0f, easedProgress));
                        }
                    });

                disableAnalyzingPresentation(EAnalyzingPresentationResetMode.PreserveMouthLayout);
                return;
            }

            RectTransform mouthRectTransform = mMouthImage.rectTransform;
            Vector2 startAnchor = mouthRectTransform.anchorMin;
            Vector2 startPosition = mouthRectTransform.anchoredPosition;
            Vector2 startSize = mouthRectTransform.sizeDelta;
            Vector3 startScale = mouthRectTransform.localScale;

            await animateOverTimeAsync(
                new SecondsDuration(0.54f),
                progress =>
                {
                    float easedProgress = easeInOut(progress);
                    float pulse = Mathf.Sin(progress.Value * Mathf.PI);
                    Vector2 currentAnchor = Vector2.Lerp(startAnchor, RESULT_MOUTH_ANCHOR, easedProgress);
                    mouthRectTransform.anchorMin = currentAnchor;
                    mouthRectTransform.anchorMax = currentAnchor;
                    mouthRectTransform.anchoredPosition = Vector2.Lerp(startPosition, Vector2.zero, easedProgress);
                    mouthRectTransform.sizeDelta = Vector2.Lerp(startSize, RESULT_MOUTH_SIZE_PIXELS, easedProgress);
                    mouthRectTransform.localScale = Vector3.Lerp(startScale, Vector3.one, easedProgress) * (1.0f + (pulse * 0.035f));
                    setOverlayTint(new Color(0.022f, 0.016f, 0.014f, 1.0f), Mathf.Lerp(0.46f, 0.38f, easedProgress));
                    mMouthImage.color = new Color(1.0f, 0.93f, 0.82f, Mathf.Lerp(0.62f, 0.88f, easedProgress));

                    if (mMouthListeningAuraImage != null)
                    {
                        Color listeningAuraColor = mMouthListeningAuraImage.color;
                        mMouthListeningAuraImage.color = new Color(
                            listeningAuraColor.r,
                            listeningAuraColor.g,
                            listeningAuraColor.b,
                            Mathf.Lerp(listeningAuraColor.a, 0.0f, easedProgress));
                    }

                    if (mMouthAnalyzingAuraImage != null)
                    {
                        Color auraColor = mMouthAnalyzingAuraImage.color;
                        mMouthAnalyzingAuraImage.color = new Color(auraColor.r, auraColor.g, auraColor.b, Mathf.Lerp(auraColor.a, 0.0f, easedProgress));
                    }
                });

            disableAnalyzingPresentation(EAnalyzingPresentationResetMode.PreserveMouthLayout);
        }

        public void ShowResult(EVerdictKind verdictKind)
        {
            resetHandPromptPanelAlpha();
            disableAnsweringPresentation();
            disableAnalyzingPresentation();
            applyResultLayout(verdictKind);
            configureExitButtonAsTopLeftIcon();
            if (isTempleApproachSceneActive())
            {
                applyTempleStageBackgroundPresentation(0.38f);
            }
            else
            {
                mBackgroundImage.sprite = mMouthChamberBackgroundSprite;
                setBackgroundTint(STAGE_BACKGROUND_TINT);
                setObjectVisibility(mBackgroundImage, EUiElementVisibility.Visible);
                setObjectVisibility(mCarpetImage, EUiElementVisibility.Hidden);
            }

            hideCommonScreenElements();
            setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
            setGameplayOverlayAlpha(0.38f);
            setObjectVisibility(mMouthImage, EUiElementVisibility.Visible);
            setObjectVisibility(mVerdictImage, EUiElementVisibility.Visible);
            setObjectVisibility(mTryAgainButton, EUiElementVisibility.Visible);
            setObjectVisibility(mExitButton, EUiElementVisibility.Visible);
            mAnswerInputField.interactable = false;

            mVerdictImage.sprite = verdictKind switch
            {
                EVerdictKind.True => mVerdictTrueSprite,
                EVerdictKind.False => mVerdictFalseSprite,
                _ => mVerdictUncertainSprite,
            };
            string verdictText = verdictKind switch
            {
                EVerdictKind.True => "TRUE",
                EVerdictKind.False => "FALSE",
                _ => "UNCERTAIN",
            };
            setText(mVerdictText, verdictText);
            if (isTempleApproachSceneActive() == false)
            {
                applyMouthAnchoredLayout();
            }

            if (isTempleApproachSceneActive())
            {
                setTempleApproachMouthColor(Color.white);
                syncTempleStageMouthOverlay(0.0f);
            }
            else
            {
                mMouthImage.color = Color.white;
                mMouthImage.rectTransform.localScale = Vector3.one;
            }

            mVerdictImage.color = Color.white;
            mVerdictImage.rectTransform.localRotation = Quaternion.identity;
            mVerdictImage.rectTransform.localScale = Vector3.one;
            playVerdictCue(verdictKind);
        }

        public async Task PlayResultRevealAnimationAsync(EVerdictKind verdictKind)
        {
            setObjectVisibility(mTryAgainButton, EUiElementVisibility.Hidden);

            if (isTempleApproachSceneActive())
            {
                await playTempleResultRevealAnimationAsync(verdictKind);
            }
            else if (verdictKind == EVerdictKind.True)
            {
                await playTrueRevealAnimationAsync();
            }
            else if (verdictKind == EVerdictKind.False)
            {
                await playFalseRevealAnimationAsync();
            }
            else
            {
                await playUncertainRevealAnimationAsync();
            }

            setObjectVisibility(mTryAgainButton, EUiElementVisibility.Visible);
            setGameplayOverlayAlpha(0.38f);
            if (isTempleApproachSceneActive())
            {
                setTempleApproachMouthColor(Color.white);
                syncTempleStageMouthOverlay(0.0f);
            }
            else
            {
                mMouthImage.color = Color.white;
                mMouthImage.rectTransform.localScale = Vector3.one;
            }

            mVerdictImage.color = Color.white;
            mVerdictImage.rectTransform.localRotation = Quaternion.identity;
            mVerdictImage.rectTransform.localScale = Vector3.one;
        }

        private async Task playTempleResultRevealAnimationAsync(EVerdictKind verdictKind)
        {
            mVerdictImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            Color overlayTint = verdictKind switch
            {
                EVerdictKind.True => new Color(0.05f, 0.11f, 0.06f, 1.0f),
                EVerdictKind.False => new Color(0.22f, 0.006f, 0.006f, 1.0f),
                _ => new Color(0.055f, 0.055f, 0.075f, 1.0f),
            };
            Color mouthTint = verdictKind switch
            {
                EVerdictKind.True => new Color(0.88f, 1.0f, 0.82f, 1.0f),
                EVerdictKind.False => new Color(1.0f, 0.68f, 0.58f, 1.0f),
                _ => new Color(0.74f, 0.76f, 0.82f, 1.0f),
            };

            await animateOverTimeAsync(
                new SecondsDuration(verdictKind == EVerdictKind.False ? 0.88f : 0.72f),
                progress =>
                {
                    float progressValue = progress.Value;
                    float easedProgress = easeOut(progress);
                    float pulse = Mathf.Sin(progressValue * Mathf.PI);
                    float falloff = 1.0f - easedProgress;
                    float residualShake = Mathf.Sin(progressValue * Mathf.PI * 8.0f) * falloff;
                    float shake = verdictKind == EVerdictKind.False
                        ? Mathf.Sin(progressValue * Mathf.PI * 12.0f) * falloff
                        : residualShake * 0.45f;
                    float verticalShake = Mathf.Sin(progressValue * Mathf.PI * 7.0f) * falloff * 0.72f;
                    setTempleCameraPoseCenteredOnMouth(TEMPLE_RESULT_FOCUS_SCALE + (pulse * 0.018f), TEMPLE_MOUTH_FOCUS_CENTER, shake * 1.4f, verticalShake);
                    setOverlayTint(overlayTint, Mathf.Lerp(0.48f, 0.38f, easedProgress));
                    setTempleApproachMouthColor(Color.Lerp(Color.white, mouthTint, 1.0f - easedProgress * 0.25f));
                    mVerdictImage.color = new Color(1.0f, 1.0f, 1.0f, easedProgress);
                    mVerdictImage.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, shake * 2.4f);
                    mVerdictImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.92f + (pulse * 0.08f), 1.0f, easedProgress);
                });
        }

        private async Task playTrueRevealAnimationAsync()
        {
            mVerdictImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            await animateOverTimeAsync(
                new SecondsDuration(0.68f),
                progress =>
                {
                    float easedProgress = easeOut(progress);
                    float glow = Mathf.Sin(easedProgress * Mathf.PI);
                    setOverlayTint(new Color(0.05f, 0.11f, 0.06f, 1.0f), Mathf.Lerp(0.46f, 0.32f, easedProgress));
                    mMouthImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.025f, 1.0f, easedProgress);
                    mMouthImage.color = new Color(0.88f, 1.0f, 0.82f, Mathf.Lerp(0.86f, 1.0f, easedProgress));
                    mVerdictImage.color = new Color(1.0f, 1.0f, 1.0f, easedProgress);
                    mVerdictImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.90f, 1.0f + (glow * 0.02f), easedProgress);
                });
        }

        private async Task playFalseRevealAnimationAsync()
        {
            mVerdictImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            await animateOverTimeAsync(
                new SecondsDuration(0.32f),
                progress =>
                {
                    float easedProgress = easeInOut(progress);
                    setOverlayTint(new Color(0.28f, 0.012f, 0.008f, 1.0f), Mathf.Lerp(0.42f, 0.70f, easedProgress));
                    mMouthImage.color = new Color(1.0f, 0.68f, 0.58f, Mathf.Lerp(0.88f, 1.0f, easedProgress));
                    mMouthImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.22f, easedProgress);
                });

            await animateOverTimeAsync(
                new SecondsDuration(0.56f),
                progress =>
                {
                    float easedProgress = easeOut(progress);
                    float shake = Mathf.Sin(progress.Value * Mathf.PI * 14.0f) * (1.0f - easedProgress);
                    setOverlayTint(new Color(0.22f, 0.006f, 0.006f, 1.0f), Mathf.Lerp(0.70f, 0.40f, easedProgress));
                    mMouthImage.color = new Color(1.0f, Mathf.Lerp(0.60f, 1.0f, easedProgress), Mathf.Lerp(0.54f, 1.0f, easedProgress), 1.0f);
                    mMouthImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.22f, 1.0f, easedProgress);
                    mVerdictImage.color = new Color(1.0f, 1.0f, 1.0f, easedProgress);
                    mVerdictImage.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, shake * 2.5f);
                    mVerdictImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(1.34f, 1.0f, easedProgress);
                });
        }

        private async Task playUncertainRevealAnimationAsync()
        {
            mVerdictImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            await animateOverTimeAsync(
                new SecondsDuration(0.72f),
                progress =>
                {
                    float progressValue = progress.Value;
                    float easedProgress = easeInOut(progress);
                    float wobble = Mathf.Sin(progressValue * Mathf.PI * 7.0f) * (1.0f - easedProgress);
                    float flickerAlpha = Mathf.Lerp(0.25f, 1.0f, easedProgress)
                        + (Mathf.Sin(progressValue * Mathf.PI * 9.0f) * 0.08f * (1.0f - easedProgress));
                    setOverlayTint(new Color(0.055f, 0.055f, 0.075f, 1.0f), Mathf.Lerp(0.48f, 0.40f, easedProgress));
                    mMouthImage.color = new Color(0.74f, 0.76f, 0.82f, Mathf.Lerp(0.74f, 1.0f, easedProgress));
                    mMouthImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.985f, 1.0f, easedProgress);
                    mVerdictImage.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp01(flickerAlpha));
                    mVerdictImage.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, wobble * 2.5f);
                    mVerdictImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.96f, 1.0f, easedProgress);
                });
        }

        private void applyResultLayout(EVerdictKind verdictKind)
        {
            if (isTempleApproachSceneActive())
            {
                syncTempleStageMouthOverlay(0.0f);
            }
            else
            {
                setRectTransformLayout(mMouthImage.rectTransform, UiRectLayout.At(RESULT_MOUTH_ANCHOR, RESULT_MOUTH_SIZE_PIXELS));
                mMouthImage.rectTransform.localScale = Vector3.one;
            }

            Vector2 verdictSizePixels = verdictKind == EVerdictKind.True || verdictKind == EVerdictKind.False
                ? RESULT_SHORT_VERDICT_SIZE_PIXELS
                : RESULT_VERDICT_SIZE_PIXELS;
            setRectTransformLayout(mVerdictImage.rectTransform, UiRectLayout.At(new Vector2(0.5f, 0.54f), verdictSizePixels));
            setRectTransformLayout(mHandImage.rectTransform, UiRectLayout.At(new Vector2(0.5f, 0.18f), HELD_POINTER_CURSOR_SIZE_PIXELS));
            setRectTransformLayout(
                mTryAgainButton.GetComponent<RectTransform>(),
                UiRectLayout.At(new Vector2(0.5f, 0.105f), new Vector2(360.0f, 100.0f)));
            applyTopLeftExitButtonLayout();
        }
    }
}
