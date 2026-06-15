using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Diagnostics;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    [DisallowMultipleComponent]
    public partial class MouthOfTruthGameView : MonoBehaviour
    {
        public async Task InitializeAsync()
        {
            MouthOfTruthLog.LogInfo("MouthOfTruthGameView initialization started.");
            ensureEventSystemExists();
            loadUiFonts();
            buildCanvas();
            cacheWorldPresentationReferences();
            buildAudioSources();
            MouthOfTruthLog.LogInfo("MouthOfTruthGameView loading sprites.");
            await loadSpritesAsync();
            MouthOfTruthLog.LogInfo("MouthOfTruthGameView loading audio.");
            await loadAudioClipsAsync();
            MouthOfTruthLog.LogInfo("MouthOfTruthGameView applying theme.");
            applyTheme();
            refreshWorldPresentationLayout();
            ShowStartScreen();
            setObjectActive(mLoadingOverlayImage, false);
        }

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

        public void ShowStartScreen()
        {
            resetHandPromptPanelAlpha();
            disableAnsweringPresentation();
            disableAnalyzingPresentation();
            destroyTempleApproachScene();
            resetStageMotionTransforms();
            applyStartScreenLayout();
            configureExitButtonAsTopLeftIcon();
            mBackgroundImage.sprite = mTitleBackgroundSprite;
            setBackgroundTint(TITLE_BACKGROUND_TINT);
            setObjectActive(mLogoImage, true);
            setObjectActive(mTitleVignetteImage, true);
            setObjectActive(mSceneOverlayImage, false);
            setObjectActive(mStartButton, true);
            setObjectActive(mExitButton, true);
            setObjectActive(mBackgroundImage, true);
            setObjectActive(mCarpetImage, false);
            setObjectActive(mQuestionText, false);
            setObjectActive(mQuestionPanelImage, false);
            setObjectActive(mStatusPanelImage, false);
            setObjectActive(mResultPanelImage, false);
            setObjectActive(mPromptText, false);
            setObjectActive(mStatusText, false);
            setObjectActive(mAnswerTimerText, false);
            setObjectActive(mAnswerInputField, false);
            setObjectActive(mMouthImage, false);
            setMouthEffectImagesActive(false, false);
            setObjectActive(mHandImage, false);
            setObjectActive(mRitualHandImage, false);
            setObjectActive(mPointerImage, false);
            setObjectActive(mVerdictImage, false);
            setObjectActive(mVerdictText, false);
            setObjectActive(mTryAgainButton, false);
            setObjectActive(mBackToTitleButton, false);
            setCardsVisible(false);
            hideFirstRunTutorialPresentation();
            setText(mPromptText, string.Empty);
            setText(mStatusText, string.Empty);
            setText(mAnswerTimerText, string.Empty);
            mLastAudibleHoveredCardSlotOrNull = null;
            mLastHoveredUiActionTargetOrNull = null;
            mLastCardHoverCueTimeSeconds = -999.0f;
            refreshWorldPresentationLayout();
            ensureAmbiencePlayback();
        }

        public void ShowCardSelection(QuestionRoundSelection questionRoundSelection)
        {
            resetHandPromptPanelAlpha();
            disableAnsweringPresentation();
            disableAnalyzingPresentation();
            resetStageMotionTransforms();
            applyCardSelectionLayout();
            configureExitButtonAsTopLeftIcon();
            mBackgroundImage.sprite = mCardSelectionBackgroundSprite;
            setBackgroundTint(STAGE_BACKGROUND_TINT);
            bool isTempleApproachSceneVisible = mTempleApproachCameraObject != null;
            setObjectActive(mBackgroundImage, isTempleApproachSceneVisible == false);
            setObjectActive(mCarpetImage, isTempleApproachSceneVisible == false);

            if (isTempleApproachSceneVisible)
            {
                setTempleApproachMouthAlpha(CARD_SELECTION_DIM_MOUTH_ALPHA);
            }

            setObjectActive(mLogoImage, false);
            setObjectActive(mTitleVignetteImage, false);
            setObjectActive(mSceneOverlayImage, true);
            setOverlayTint(STAGE_OVERLAY_TINT, TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA);
            setObjectActive(mStartButton, false);
            setObjectActive(mExitButton, true);
            setObjectActive(mQuestionText, false);
            setObjectActive(mQuestionPanelImage, false);
            setObjectActive(mStatusPanelImage, false);
            setObjectActive(mResultPanelImage, false);
            setObjectActive(mPromptText, true);
            setObjectActive(mStatusText, false);
            setObjectActive(mAnswerTimerText, false);
            setObjectActive(mAnswerInputField, false);
            setObjectActive(mMouthImage, false);
            setMouthEffectImagesActive(false, false);
            setObjectActive(mHandImage, false);
            setObjectActive(mRitualHandImage, false);
            setObjectActive(mPointerImage, false);
            setObjectActive(mVerdictImage, false);
            setObjectActive(mVerdictText, false);
            setObjectActive(mTryAgainButton, false);
            setObjectActive(mBackToTitleButton, false);
            setCardsVisible(true);

            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                pair.Value.SetBack(mCardBackSprite);
                pair.Value.ResetTransformState();
                pair.Value.SetVisualState(EQuestionCardVisualState.Normal, NormalizedProgress.Zero);
                pair.Value.ResetHoverState();
            }

            applyCardAnchorPositions();
            setText(mPromptText, "원하는 카드를 손으로 선택하세요.");
            setText(mStatusText, string.Empty);
            setText(mAnswerTimerText, string.Empty);
            mLastAudibleHoveredCardSlotOrNull = null;
            mLastHoveredUiActionTargetOrNull = null;
            mLastCardHoverCueTimeSeconds = -999.0f;
        }

        public async Task PlayCardSelectionEntranceAsync()
        {
            setObjectActive(mPromptText, false);
            setObjectActive(mSceneOverlayImage, true);
            setOverlayTint(STAGE_OVERLAY_TINT, TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA);

            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                pair.Value.SetAlpha(NormalizedProgress.Zero);
                pair.Value.SetScale(QuestionCardScale.Uniform(0.92f));
            }

            await animateOverTimeAsync(
                CARD_SELECTION_ENTRANCE_SECONDS,
                progress =>
                {
                    float overlayProgress = easeOut(progress);
                    float overlayAlpha = Mathf.Lerp(TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA, CARD_SELECTION_SETTLED_OVERLAY_ALPHA, overlayProgress);
                    setOverlayTint(STAGE_OVERLAY_TINT, overlayAlpha);

                    foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
                    {
                        float cardProgress = easeOut(Mathf.Clamp01((progress - getCardEntranceDelay(pair.Key)) / 0.70f));
                        pair.Value.SetAlpha(NormalizedProgress.FromUnclamped(cardProgress));
                        pair.Value.SetScale(QuestionCardScale.Uniform(Mathf.Lerp(0.92f, 1.0f, cardProgress)));
                    }
                });

            await animateOverTimeAsync(
                CARD_SELECTION_ENTRANCE_SETTLE_SECONDS,
                _ =>
                {
                    setOverlayTint(STAGE_OVERLAY_TINT, CARD_SELECTION_SETTLED_OVERLAY_ALPHA);
                });

            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                pair.Value.SetAlpha(NormalizedProgress.Complete);
                pair.Value.SetScale(QuestionCardScale.Uniform(1.0f));
            }

            setOverlayTint(STAGE_OVERLAY_TINT, CARD_SELECTION_SETTLED_OVERLAY_ALPHA);
            setObjectActive(mPromptText, true);
        }

        public void UpdateCardHoverVisual(EQuestionCardSlot? hoveredQuestionCardSlotOrNull, NormalizedProgress hoverProgress)
        {
            if (hoveredQuestionCardSlotOrNull != mLastAudibleHoveredCardSlotOrNull)
            {
                bool hasEnoughCardHoverAudioGap = Time.unscaledTime - mLastCardHoverCueTimeSeconds >= CARD_HOVER_AUDIO_COOLDOWN_SECONDS;

                if (hoveredQuestionCardSlotOrNull.HasValue && hasEnoughCardHoverAudioGap)
                {
                    mLastCardHoverCueTimeSeconds = Time.unscaledTime;
                    playInterfaceCue(mCardHoverClip, 0.32f);
                }

                mLastAudibleHoveredCardSlotOrNull = hoveredQuestionCardSlotOrNull;
            }

            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                bool isHovered = hoveredQuestionCardSlotOrNull == pair.Key;
                pair.Value.SetVisualState(EQuestionCardVisualState.Normal, isHovered ? hoverProgress : NormalizedProgress.Zero);
            }
        }

        public void PreviewCardSelectionFocus(EQuestionCardSlot selectedQuestionCardSlot)
        {
            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                bool isSelected = pair.Key == selectedQuestionCardSlot;
                EQuestionCardVisualState questionCardVisualState = isSelected
                    ? EQuestionCardVisualState.Selected
                    : EQuestionCardVisualState.Dimmed;
                pair.Value.SetVisualState(questionCardVisualState, NormalizedProgress.Zero);
                pair.Value.gameObject.SetActive(isSelected);
            }

            setText(mPromptText, string.Empty);
        }

        public Vector2 GetQuestionCardScreenCenter(EQuestionCardSlot questionCardSlot)
        {
            QuestionCardView questionCardView;
            if (mCardViews.TryGetValue(questionCardSlot, out questionCardView) == false)
            {
                return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
            }

            RectTransform cardRectTransform = questionCardView.RectTransform;
            Vector3 worldCenter = cardRectTransform.TransformPoint(cardRectTransform.rect.center);
            return RectTransformUtility.WorldToScreenPoint(mCanvas.worldCamera, worldCenter);
        }

        private async Task playMouthJudgementFocusTransitionAsync()
        {
            if (isTempleApproachSceneActive())
            {
                TempleCameraScale targetCameraScale = new TempleCameraScale(TEMPLE_ANSWER_FOCUS_SCALE);
                TempleCameraTransition templeCameraTransition = captureTempleCameraTransition(targetCameraScale, TEMPLE_MOUTH_FOCUS_CENTER);

                await animateOverTimeAsync(
                    MOUTH_JUDGEMENT_FOCUS_SECONDS * 1.28f,
                    progress =>
                    {
                        float easedProgress = easeInOut(progress);
                        float pulse = Mathf.Sin(progress * Mathf.PI);
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
                MOUTH_JUDGEMENT_FOCUS_SECONDS * 1.28f,
                progress =>
                {
                    float easedProgress = easeInOut(progress);
                    float pulse = Mathf.Sin(progress * Mathf.PI);
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

        private void applyTopLeftExitButtonLayout()
        {
            setRectTransformLayout(mExitButton.GetComponent<RectTransform>(), new Vector2(0.06f, 0.90f), new Vector2(78.0f, 78.0f));
        }

        private void setOverlayAlpha(float alpha)
        {
            setOverlayTint(SCENE_OVERLAY_COLOR, alpha);
        }

        private void setGameplayOverlayAlpha(float alpha)
        {
            if (isTempleApproachSceneActive())
            {
                setOverlayTint(STAGE_OVERLAY_TINT, alpha);
                return;
            }

            setOverlayAlpha(alpha);
        }

        private void setOverlayTint(Color tintColor, float alpha)
        {
            if (mSceneOverlayImage == null)
            {
                return;
            }

            mSceneOverlayImage.color = new Color(tintColor.r, tintColor.g, tintColor.b, Mathf.Clamp01(alpha));
        }

        private float getHandPromptPanelHoldSeconds()
        {
            return mHandPromptClip != null
                ? Mathf.Max(0.55f, mHandPromptClip.length)
                : HAND_PROMPT_PANEL_FALLBACK_HOLD_SECONDS;
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

        private void setBackgroundTint(Color tintColor)
        {
            if (mBackgroundImage == null)
            {
                return;
            }

            mBackgroundImage.color = tintColor;
        }

        private void resetStageMotionTransforms()
        {
            if (mBackgroundImage != null)
            {
                RectTransform backgroundRectTransform = mBackgroundImage.rectTransform;
                backgroundRectTransform.pivot = new Vector2(0.5f, 0.5f);
                backgroundRectTransform.anchorMin = Vector2.zero;
                backgroundRectTransform.anchorMax = Vector2.one;
                backgroundRectTransform.offsetMin = Vector2.zero;
                backgroundRectTransform.offsetMax = Vector2.zero;
                backgroundRectTransform.anchoredPosition = Vector2.zero;
                backgroundRectTransform.localScale = Vector3.one;
                backgroundRectTransform.localRotation = Quaternion.identity;
            }

            if (mCarpetImage != null)
            {
                RectTransform carpetRectTransform = mCarpetImage.rectTransform;
                carpetRectTransform.pivot = new Vector2(0.5f, 0.5f);
                carpetRectTransform.anchorMin = new Vector2(0.5f, 0.0f);
                carpetRectTransform.anchorMax = new Vector2(0.5f, 0.0f);
                carpetRectTransform.anchoredPosition = STAGE_CARPET_POSITION;
                carpetRectTransform.sizeDelta = STAGE_CARPET_SIZE;
                carpetRectTransform.localScale = Vector3.one;
                carpetRectTransform.localRotation = Quaternion.identity;
                mCarpetImage.color = STAGE_CARPET_TINT;
            }

            if (mMouthImage != null)
            {
                mMouthImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                mMouthImage.rectTransform.localRotation = Quaternion.identity;
            }
        }

        private void placeRitualHandAboveMouth()
        {
            if (mRitualHandImage == null || mMouthImage == null)
            {
                return;
            }

            int mouthSiblingIndex = mMouthImage.transform.GetSiblingIndex();
            int targetSiblingIndex = Mathf.Min(mCanvasRootTransform.childCount - 1, mouthSiblingIndex + 1);
            mRitualHandImage.transform.SetSiblingIndex(targetSiblingIndex);

            if (mQuestionPanelImage != null && mQuestionPanelImage.gameObject.activeSelf)
            {
                mQuestionPanelImage.transform.SetAsLastSibling();
            }

            if (mQuestionText != null && mQuestionText.gameObject.activeSelf)
            {
                mQuestionText.transform.SetAsLastSibling();
            }

            if (mVerdictImage != null && mVerdictImage.gameObject.activeSelf)
            {
                mVerdictImage.transform.SetAsLastSibling();
            }

            if (mTryAgainButton != null && mTryAgainButton.gameObject.activeSelf)
            {
                mTryAgainButton.transform.SetAsLastSibling();
            }

            if (mExitButton != null && mExitButton.gameObject.activeSelf)
            {
                mExitButton.transform.SetAsLastSibling();
            }
        }

        private void configureExitButtonAsTopLeftIcon()
        {
            if (mExitButton?.image == null)
            {
                return;
            }

            mExitButton.image.sprite = mExitIconButtonSprite;
            mExitButton.image.type = Image.Type.Simple;
            mExitButton.image.preserveAspect = true;
            setButtonLabelVisible(mExitButton, false);
        }

        private void configureExitButtonAsEndGameButton()
        {
            if (mExitButton?.image == null)
            {
                return;
            }

            mExitButton.image.sprite = mEndGameButtonSprite;
            mExitButton.image.type = Image.Type.Simple;
            mExitButton.image.preserveAspect = true;
            setButtonLabelVisible(mExitButton, false);
        }

        private void setButtonLabelVisible(Button button, bool isVisible)
        {
            Text label = button != null ? button.GetComponentInChildren<Text>(includeInactive: true) : null;

            if (label != null)
            {
                label.gameObject.SetActive(isVisible);
            }
        }

        private bool isScreenPointOverButton(Button button, Vector2 screenPosition, float intentExpansionPixels)
        {
            return button != null
                && button.gameObject.activeInHierarchy
                && button.interactable
                && (isScreenPointOverRectTransform(button.GetComponent<RectTransform>(), screenPosition) || isScreenPointOverExpandedRectTransform(button.GetComponent<RectTransform>(), screenPosition, intentExpansionPixels));
        }

        private bool isScreenPointOverRectTransform(RectTransform rectTransform, Vector2 screenPosition)
        {
            return rectTransform != null
                && RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, null);
        }

        private bool isScreenPointOverExpandedRectTransform(RectTransform rectTransform, Vector2 screenPosition, float expansionPixels)
        {
            if (rectTransform == null || expansionPixels <= 0.0f)
            {
                return false;
            }

            rectTransform.GetWorldCorners(mHitTestWorldCorners);
            Vector2 firstScreenCorner = RectTransformUtility.WorldToScreenPoint(null, mHitTestWorldCorners[0]);
            float minimumX = firstScreenCorner.x;
            float maximumX = firstScreenCorner.x;
            float minimumY = firstScreenCorner.y;
            float maximumY = firstScreenCorner.y;

            for (int cornerIndex = 1; cornerIndex < mHitTestWorldCorners.Length; cornerIndex += 1)
            {
                Vector2 screenCorner = RectTransformUtility.WorldToScreenPoint(null, mHitTestWorldCorners[cornerIndex]);
                minimumX = Mathf.Min(minimumX, screenCorner.x);
                maximumX = Mathf.Max(maximumX, screenCorner.x);
                minimumY = Mathf.Min(minimumY, screenCorner.y);
                maximumY = Mathf.Max(maximumY, screenCorner.y);
            }

            return screenPosition.x >= minimumX - expansionPixels
                && screenPosition.x <= maximumX + expansionPixels
                && screenPosition.y >= minimumY - expansionPixels
                && screenPosition.y <= maximumY + expansionPixels;
        }

        private void updateButtonVisual(Button button, bool isHovered, NormalizedProgress hoverProgress)
        {
            if (button == null)
            {
                return;
            }

            float effectiveHoverProgress = isHovered ? hoverProgress.Value : 0.0f;
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            rectTransform.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.06f, effectiveHoverProgress);

            if (button.image != null)
            {
                button.image.color = Color.Lerp(Color.white, new Color(1.0f, 0.92f, 0.78f, 1.0f), effectiveHoverProgress);
            }

            Text label = button.GetComponentInChildren<Text>();

            if (label != null)
            {
                label.color = Color.Lerp(new Color(0.88f, 0.84f, 0.76f, 1.0f), new Color(1.0f, 0.97f, 0.84f, 1.0f), effectiveHoverProgress);
            }
        }

        private void setCardsVisible(bool isVisible)
        {
            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                pair.Value.gameObject.SetActive(isVisible);
            }
        }

        private void setObjectActive(Component component, bool isActive)
        {
            if (component == null)
            {
                return;
            }

            component.gameObject.SetActive(isActive);
        }

        private async Task animateOverTimeAsync(float durationSeconds, Action<float> updateAction)
        {
            if (durationSeconds <= 0.0f)
            {
                updateAction?.Invoke(1.0f);
                return;
            }

            float elapsedSeconds = 0.0f;

            while (elapsedSeconds < durationSeconds)
            {
                elapsedSeconds += Time.deltaTime;
                updateAction?.Invoke(Mathf.Clamp01(elapsedSeconds / durationSeconds));
                await Task.Yield();
            }

            updateAction?.Invoke(1.0f);
        }

        private float easeOut(float progress)
        {
            float inverse = 1.0f - progress;
            return 1.0f - (inverse * inverse * inverse);
        }

        private float easeIn(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            return clampedProgress * clampedProgress * clampedProgress;
        }

        private static float easeInOut(float progress)
        {
            float clampedProgress = Mathf.Clamp01(progress);
            return clampedProgress * clampedProgress * (3.0f - (2.0f * clampedProgress));
        }

        private static bool isInsideAnchorWindow(Vector2 offsetFromAnchor, float halfWidth, float halfHeight)
        {
            return Mathf.Abs(offsetFromAnchor.x) <= halfWidth
                && Mathf.Abs(offsetFromAnchor.y) <= halfHeight;
        }

    }
}
