using System.Collections.Generic;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Diagnostics;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    [DisallowMultipleComponent]
    public partial class MouthOfTruthGameView : MonoBehaviour
    {
        public async Task InitializeAsync()
        {
            MouthOfTruthLog.logInfo("MouthOfTruthGameView initialization started.");
            ensureEventSystemExists();
            loadUiFonts();
            buildCanvas();
            cacheWorldPresentationReferences();
            buildAudioSources();
            MouthOfTruthLog.logInfo("MouthOfTruthGameView loading sprites.");
            await loadSpritesAsync();
            MouthOfTruthLog.logInfo("MouthOfTruthGameView loading audio.");
            await loadAudioClipsAsync();
            MouthOfTruthLog.logInfo("MouthOfTruthGameView applying theme.");
            applyTheme();
            refreshWorldPresentationLayout();
            ShowStartScreen();
            setObjectVisibility(mLoadingOverlayImage, EUiElementVisibility.Hidden);
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
            hideCommonScreenElements();
            setObjectVisibility(mLogoImage, EUiElementVisibility.Visible);
            setObjectVisibility(mTitleVignetteImage, EUiElementVisibility.Visible);
            setObjectVisibility(mStartButton, EUiElementVisibility.Visible);
            setObjectVisibility(mExitButton, EUiElementVisibility.Visible);
            setObjectVisibility(mBackgroundImage, EUiElementVisibility.Visible);
            setObjectVisibility(mCarpetImage, EUiElementVisibility.Hidden);
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
            hideCommonScreenElements();
            bool isTempleApproachSceneVisible = mTempleApproachCameraObject != null;
            EUiElementVisibility stageBackdropVisibility = isTempleApproachSceneVisible
                ? EUiElementVisibility.Hidden
                : EUiElementVisibility.Visible;
            setObjectVisibility(mBackgroundImage, stageBackdropVisibility);
            setObjectVisibility(mCarpetImage, stageBackdropVisibility);

            if (isTempleApproachSceneVisible)
            {
                setTempleApproachMouthAlpha(CARD_SELECTION_DIM_MOUTH_ALPHA);
            }

            setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
            setOverlayTint(STAGE_OVERLAY_TINT, TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA);
            setObjectVisibility(mExitButton, EUiElementVisibility.Visible);
            setObjectVisibility(mPromptText, EUiElementVisibility.Visible);
            setCardsVisibility(EUiElementVisibility.Visible);

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
            setObjectVisibility(mPromptText, EUiElementVisibility.Hidden);
            setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
            setOverlayTint(STAGE_OVERLAY_TINT, TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA);

            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                pair.Value.SetAlpha(NormalizedProgress.Zero);
                pair.Value.SetScale(QuestionCardScale.Uniform(0.92f));
            }

            await animateOverTimeAsync(
                new SecondsDuration(CARD_SELECTION_ENTRANCE_SECONDS),
                progress =>
                {
                    float overlayProgress = easeOut(progress);
                    float overlayAlpha = Mathf.Lerp(TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA, CARD_SELECTION_SETTLED_OVERLAY_ALPHA, overlayProgress);
                    setOverlayTint(STAGE_OVERLAY_TINT, overlayAlpha);

                    foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
                    {
                        float cardProgress = easeOut(Mathf.Clamp01((progress.Value - getCardEntranceDelay(pair.Key)) / 0.70f));
                        pair.Value.SetAlpha(NormalizedProgress.FromUnclamped(cardProgress));
                        pair.Value.SetScale(QuestionCardScale.Uniform(Mathf.Lerp(0.92f, 1.0f, cardProgress)));
                    }
                });

            await animateOverTimeAsync(
                new SecondsDuration(CARD_SELECTION_ENTRANCE_SETTLE_SECONDS),
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
            setObjectVisibility(mPromptText, EUiElementVisibility.Visible);
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

    }
}
