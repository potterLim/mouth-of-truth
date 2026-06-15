using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    [DisallowMultipleComponent]
    public partial class MouthOfTruthGameView : MonoBehaviour
    {
        private static readonly Vector2 FALLBACK_LEFT_CARD_POSITION = new Vector2(-390.0f, 60.0f);
        private static readonly Vector2 FALLBACK_CENTER_CARD_POSITION = new Vector2(0.0f, 60.0f);
        private static readonly Vector2 FALLBACK_RIGHT_CARD_POSITION = new Vector2(390.0f, 60.0f);
        private static readonly Vector2 FALLBACK_MOUTH_POSITION = new Vector2(0.0f, 60.0f);
        private static readonly Vector2 FALLBACK_HAND_FRONT_POSITION = new Vector2(0.0f, -20.0f);
        private static readonly Vector2 FALLBACK_HAND_INNER_POSITION = new Vector2(0.0f, 230.0f);
        private static readonly Color TITLE_BACKGROUND_TINT = new Color(0.95f, 0.95f, 0.97f, 1.0f);
        private static readonly Color STAGE_BACKGROUND_TINT = new Color(0.82f, 0.82f, 0.86f, 1.0f);
        private const float FRONT_ANCHOR_RADIUS_FACTOR = 0.066f;
        private const float INNER_ANCHOR_RADIUS_FACTOR = 0.040f;
        private const float FRONT_ENTRY_HALF_WIDTH_FACTOR = 0.039f;
        private const float FRONT_ENTRY_HALF_HEIGHT_FACTOR = 0.050f;
        private const float INNER_ENTRY_HALF_WIDTH_FACTOR = 0.027f;
        private const float INNER_ENTRY_HALF_HEIGHT_FACTOR = 0.036f;
        private const float CARD_INTENT_LEFT_MAX_NORMALIZED_X = 0.39f;
        private const float CARD_INTENT_RIGHT_MIN_NORMALIZED_X = 0.61f;
        private const float CARD_INTENT_MIN_NORMALIZED_Y = 0.28f;
        private const float CARD_INTENT_MAX_NORMALIZED_Y = 0.84f;
        private const float HAND_DETECTION_VERTICAL_OFFSET_FACTOR = 0.105f;
        private const float MOUTH_INTENT_LEFT_WIDTH_FACTOR = 0.1108f;
        private const float MOUTH_INTENT_RIGHT_WIDTH_FACTOR = 0.0772f;
        private const float MOUTH_INTENT_LOWER_MARGIN_FACTOR = 0.066f;
        private const float MOUTH_INTENT_UPPER_MARGIN_FACTOR = 0.040f;
        private const float MOUTH_INTENT_INNER_SWITCH_FACTOR = 0.58f;
        private const float BUTTON_INTENT_EXPANSION_PIXELS = 54.0f;
        private const float EXIT_BUTTON_INTENT_EXPANSION_PIXELS = 32.0f;
        private const float CARD_FRONT_READ_HOLD_MINIMUM_SECONDS = 1.875f;
        private const float CARD_FRONT_READ_HOLD_MAXIMUM_SECONDS = 3.225f;
        private const float CARD_FRONT_READ_HOLD_PER_CHARACTER_SECONDS = 0.01875f;
        private const float CARD_FRONT_FOCUS_BEFORE_NARRATION_SECONDS = 1.05f;
        private const float CARD_FRONT_AFTER_NARRATION_HOLD_SECONDS = 0.64f;
        private const float CARD_HOVER_AUDIO_COOLDOWN_SECONDS = 0.60f;
        private const float CARD_SELECTION_CUE_SETTLE_SECONDS = 0.32f;
        private const float CARD_REVEAL_CUE_SETTLE_SECONDS = 0.46f;
        private const float CARD_FLIP_CLOSE_SECONDS = 0.28f;
        private const float CARD_FLIP_OPEN_SECONDS = 0.36f;
        private const float CARD_TO_MOUTH_ABSORPTION_SECONDS = 1.17f;
        private const float HAND_PROMPT_AFTER_CARD_LAUNCH_DELAY_SECONDS = 0.16f;
        private const float FIRST_RUN_TUTORIAL_FALLBACK_DURATION_SECONDS = 4.0f;
        private const float FIRST_RUN_TUTORIAL_DURATION_SCALE = 3.0f;
        private const float HAND_INSERTION_DURATION_SECONDS = 2.35f;
        private const float HAND_PROMPT_PANEL_FALLBACK_HOLD_SECONDS = 1.65f;
        private const float HAND_PROMPT_PANEL_DISMISS_SECONDS = 0.36f;
        private const float MOUTH_JUDGEMENT_FOCUS_SECONDS = 0.72f;
        private const float ANALYSIS_FOCUS_RAMP_SECONDS = 2.10f;
        private const float ANSWER_BEAM_SWEEP_RATE = 0.725f;
        private const float ANSWER_BEAM_SOURCE_Y_FACTOR = 0.108f;
        private const float ANSWER_BEAM_END_BOTTOM_Y_FACTOR = -0.43f;
        private const float ANSWER_BEAM_END_TOP_Y_FACTOR = 0.50f;
        private const float TEMPLE_APPROACH_FORWARD_DURATION_SECONDS = 3.0f;
        private const float TEMPLE_APPROACH_MOUTH_DIM_SECONDS = 0.48f;
        private const float TEMPLE_APPROACH_STAIR_START_SCALE = 1.85f;
        private const float TEMPLE_APPROACH_END_SCALE = 4.36f;
        private const float TEMPLE_ANSWER_FOCUS_SCALE = 4.36f;
        private const float TEMPLE_ANALYSIS_FOCUS_SCALE = 6.82f;
        private const float TEMPLE_RESULT_FOCUS_SCALE = TEMPLE_ANALYSIS_FOCUS_SCALE;
        private const float TEMPLE_APPROACH_START_OVERLAY_ALPHA = 0.42f;
        private const float TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA = 0.18f;
        private const float CARD_SELECTION_SETTLED_OVERLAY_ALPHA = 0.18f;
        private const float CARD_SELECTION_DIM_MOUTH_ALPHA = 0.22f;
        private const float CARD_SELECTION_ENTRANCE_SECONDS = 0.82f;
        private const float CARD_SELECTION_ENTRANCE_SETTLE_SECONDS = 0.22f;
        private const float AMBIENCE_AUDIO_VOLUME = 0.32f;
        private const float INTERFACE_AUDIO_VOLUME = 0.78f;
        private const float INTERFACE_AUDIO_MAX_VOLUME_SCALE = 0.74f;
        private const float INTERFACE_AUDIO_OVERLAP_DUCK_SCALE = 0.72f;
        private const int POINTER_CURSOR_TEXTURE_SIZE = 64;
        private static readonly Vector2 POINTER_CURSOR_SIZE_PIXELS = new Vector2(46.0f, 46.0f);
        private static readonly Vector2 HELD_POINTER_CURSOR_SIZE_PIXELS = new Vector2(58.0f, 58.0f);
        private static readonly Vector2 RITUAL_HAND_SIZE_PIXELS = new Vector2(340.0f, 380.0f);
        private static readonly Vector2 TUTORIAL_HAND_SIZE_PIXELS = new Vector2(260.0f, 290.0f);
        private static readonly Vector2 TUTORIAL_LEAP_MOTION_DEVICE_SIZE_PIXELS = new Vector2(350.0f, 160.0f);
        private static readonly Vector2 ANSWERING_FOCUS_MOUTH_ANCHOR = new Vector2(0.5f, 0.51f);
        private static readonly Vector2 ANSWERING_FOCUS_MOUTH_SIZE_PIXELS = new Vector2(1120.0f, 1120.0f);
        private static readonly Vector2 RESULT_MOUTH_ANCHOR = new Vector2(0.5f, 0.52f);
        private static readonly Vector2 RESULT_MOUTH_SIZE_PIXELS = new Vector2(1680.0f, 1680.0f);
        private static readonly Vector2 RESULT_VERDICT_SIZE_PIXELS = new Vector2(1390.0f, 322.0f);
        private static readonly Vector2 RESULT_SHORT_VERDICT_SIZE_PIXELS = new Vector2(1580.0f, 365.0f);
        private static readonly Color SCENE_OVERLAY_COLOR = new Color(0.03f, 0.02f, 0.02f, 1.0f);
        private static readonly Color STAGE_OVERLAY_TINT = new Color(0.020f, 0.014f, 0.010f, 1.0f);
        private static readonly Vector2 TEMPLE_MOUTH_FOCUS_CENTER = Vector2.zero;
        private static readonly Color POINTER_CURSOR_FILL_COLOR = new Color(0.62f, 0.64f, 0.66f, 0.54f);
        private static readonly Color POINTER_CURSOR_RING_COLOR = new Color(0.90f, 0.91f, 0.92f, 0.86f);
        private static readonly Vector2 STAGE_CARPET_POSITION = new Vector2(0.0f, 166.0f);
        private static readonly Vector2 STAGE_CARPET_SIZE = new Vector2(880.0f, 328.0f);
        private static readonly Color STAGE_CARPET_TINT = new Color(0.58f, 0.52f, 0.48f, 0.82f);
        private static readonly Vector2 TEMPLE_APPROACH_MOUTH_POSITION = new Vector2(0.0f, 90.0f);
        private static readonly Vector2 TEMPLE_APPROACH_MOUTH_SIZE = new Vector2(246.0f, 246.0f);
        private static readonly Vector2 TEMPLE_HAND_FRONT_OFFSET_FACTOR = new Vector2(0.0f, -0.20f);
        private static readonly Vector2 TEMPLE_HAND_INNER_OFFSET_FACTOR = new Vector2(0.0f, -0.17f);
        private readonly Dictionary<EQuestionCardSlot, QuestionCardView> mCardViews = new Dictionary<EQuestionCardSlot, QuestionCardView>();
        private readonly Vector3[] mHitTestWorldCorners = new Vector3[4];
        private readonly Vector3[] mTempleMouthWorldCorners = new Vector3[4];

        private Canvas mCanvas;
        private RectTransform mCanvasRootRectTransform;
        private Transform mCanvasRootTransform;
        private Image mBackgroundImage;
        private Image mSceneOverlayImage;
        private Image mLoadingOverlayImage;
        private Image mTutorialOverlayImage;
        private Image mTutorialDevicePanelImage;
        private Image mTutorialLeapMotionDeviceImage;
        private Image mTutorialHandImage;
        private Text mTutorialTitleText;
        private Text mTutorialBodyText;
        private Text mTutorialStepText;
        private Image mCarpetImage;
        private Image mTitleVignetteImage;
        private Image mLogoImage;
        private Image mQuestionPanelImage;
        private Image mStatusPanelImage;
        private Image mResultPanelImage;
        private Text mPromptText;
        private Text mQuestionText;
        private Text mStatusText;
        private Text mAnalyzingDotsText;
        private Text mAnswerTimerText;
        private InputField mAnswerInputField;
        private Image mMouthImage;
        private Image mMouthListeningAuraImage;
        private Image mMouthAnalyzingAuraImage;
        private Image mMouthLeftEyeBeamImage;
        private Image mMouthRightEyeBeamImage;
        private GameObject mTempleApproachCameraObject;
        private RectTransform mTempleApproachCameraRectTransform;
        private Image mTempleApproachMouthImage;
        private Image mHandImage;
        private Image mRitualHandImage;
        private Image mPointerImage;
        private Image mVerdictImage;
        private Text mVerdictText;
        private Button mStartButton;
        private Button mTryAgainButton;
        private Button mBackToTitleButton;
        private Button mExitButton;
        private Sprite mCardBackSprite;
        private Sprite mCardFrontSprite;
        private Sprite mButtonFrameSprite;
        private Sprite mStartButtonSprite;
        private Sprite mTryAgainButtonSprite;
        private Sprite mEndGameButtonSprite;
        private Sprite mExitIconButtonSprite;
        private Sprite mPointerCursorSprite;
        private Sprite mRitualHandSprite;
        private Sprite mLeapMotionDeviceSprite;
        private Sprite mVerdictTrueSprite;
        private Sprite mVerdictFalseSprite;
        private Sprite mVerdictUncertainSprite;
        private Sprite mTitleVignetteSprite;
        private Sprite mQuestionPanelSprite;
        private Sprite mStatusPanelSprite;
        private Sprite mResultPanelSprite;
        private Sprite mCardGlowSprite;
        private Sprite mDwellFillSprite;
        private Sprite mTitleBackgroundSprite;
        private Sprite mCardSelectionBackgroundSprite;
        private Sprite mMouthChamberBackgroundSprite;
        private AudioSource mAmbienceAudioSource;
        private AudioSource mInterfaceAudioSource;
        private AudioClip mTitleAmbienceClip;
        private AudioClip mButtonConfirmClip;
        private AudioClip mCardHoverClip;
        private AudioClip mCardSelectClip;
        private AudioClip mCardRevealClip;
        private AudioClip mHandInsertClip;
        private AudioClip mHandPromptClip;
        private AudioClip mResultTrueClip;
        private AudioClip mResultFalseClip;
        private AudioClip mResultUncertainClip;
        private Font mUiFont;
        private Font mKoreanFallbackFont;
        private EQuestionCardSlot? mLastAudibleHoveredCardSlotOrNull;
        private Camera mWorldCamera;
        private CardPresentationAnchorSet mCardPresentationAnchorSet;
        private MouthAnchorSet mMouthAnchorSet;
        private bool mUseWorldEnvironmentLayout;
        private EUiActionTarget? mLastHoveredUiActionTargetOrNull;
        private bool mIsAnsweringPresentationActive;
        private bool mIsAnalyzingPresentationActive;
        private bool mIsHandPromptPanelDismissalActive;
        private bool mIsCardAbsorptionPresentationActive;
        private float mAnsweringPresentationStartedAtSeconds;
        private float mAnalyzingPresentationStartedAtSeconds;
        private float mHandPromptPanelDismissalStartedAtSeconds;
        private float mHandPromptPanelDismissalStartAlpha = 1.0f;
        private NormalizedProgress mCardAbsorptionPresentationProgress;
        private float mLastCardHoverCueTimeSeconds = -999.0f;

        private bool mStartRequested;
        private bool mTryAgainRequested;
        private bool mBackToTitleRequested;
        private bool mExitRequested;

        public bool IsFirstRunTutorialVisible { get; private set; }

        public SecondsDuration AnalysisFocusRampDuration => new SecondsDuration(ANALYSIS_FOCUS_RAMP_SECONDS);

        public SecondsDuration AnswerBeamSweepCycleDuration => new SecondsDuration(2.0f / ANSWER_BEAM_SWEEP_RATE);

        public bool IsCardAbsorptionPresentationActive => mIsCardAbsorptionPresentationActive;

        public NormalizedProgress CardAbsorptionPresentationProgress => mCardAbsorptionPresentationProgress;

        public SecondsDuration HandPromptPanelHoldDuration => new SecondsDuration(getHandPromptPanelHoldSeconds());

        public async Task InitializeAsync()
        {
            Debug.Log("MouthOfTruthGameView initialization started.");
            ensureEventSystemExists();
            loadUiFonts();
            buildCanvas();
            cacheWorldPresentationReferences();
            buildAudioSources();
            Debug.Log("MouthOfTruthGameView loading sprites.");
            await loadSpritesAsync();
            Debug.Log("MouthOfTruthGameView loading audio.");
            await loadAudioClipsAsync();
            Debug.Log("MouthOfTruthGameView applying theme.");
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
                pair.Value.SetScale(0.92f);
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
                        pair.Value.SetScale(Mathf.Lerp(0.92f, 1.0f, cardProgress));
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
                pair.Value.SetScale(1.0f);
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
                TempleCameraTransition templeCameraTransition = captureTempleCameraTransition(TEMPLE_ANSWER_FOCUS_SCALE, TEMPLE_MOUTH_FOCUS_CENTER);

                await animateOverTimeAsync(
                    MOUTH_JUDGEMENT_FOCUS_SECONDS * 1.28f,
                    progress =>
                    {
                        float easedProgress = easeInOut(progress);
                        float pulse = Mathf.Sin(progress * Mathf.PI);
                        float cameraScale = templeCameraTransition.GetScale(TEMPLE_ANSWER_FOCUS_SCALE, easedProgress) + (pulse * 0.018f);
                        Vector2 cameraPosition = templeCameraTransition.GetPosition(easedProgress);
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

        private void cacheWorldPresentationReferences()
        {
            mWorldCamera = Camera.main;

            if (mWorldCamera == null)
            {
                mWorldCamera = FindAnyObjectByType<Camera>();
            }

            mCardPresentationAnchorSet = FindAnyObjectByType<CardPresentationAnchorSet>();
            mMouthAnchorSet = FindAnyObjectByType<MouthAnchorSet>();
            mUseWorldEnvironmentLayout = mWorldCamera != null && mCardPresentationAnchorSet != null && mCardPresentationAnchorSet.HasRequiredAnchors() && mMouthAnchorSet != null && mMouthAnchorSet.HasRequiredAnchors();
        }

        private void refreshWorldPresentationLayout()
        {
            if (mUseWorldEnvironmentLayout == false)
            {
                return;
            }

            applyCardAnchorPositions();
            applyMouthAnchoredLayout();
            setHandVisual(0.0f);
        }

        private void applyCardAnchorPositions()
        {
            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                pair.Value.SetAnchoredPosition(getCardAnchorPosition(pair.Key));
            }
        }

        private void applyMouthAnchoredLayout()
        {
            RectTransform mouthRectTransform = mMouthImage.rectTransform;
            mouthRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            mouthRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            mouthRectTransform.anchoredPosition = getMouthAnchorPosition();
        }

        private Vector2 getCenteredCardRevealPosition()
        {
            return getCardAnchorPosition(EQuestionCardSlot.CenterCard) + new Vector2(0.0f, 10.0f);
        }

        private static float getCardEntranceDelay(EQuestionCardSlot questionCardSlot)
        {
            return questionCardSlot switch
            {
                EQuestionCardSlot.LeftCard => 0.04f,
                EQuestionCardSlot.CenterCard => 0.00f,
                EQuestionCardSlot.RightCard => 0.08f,
                _ => 0.0f,
            };
        }

        private Vector2 getCardAnchorPosition(EQuestionCardSlot questionCardSlot)
        {
            Vector2 fallbackPosition = questionCardSlot switch
            {
                EQuestionCardSlot.LeftCard => FALLBACK_LEFT_CARD_POSITION,
                EQuestionCardSlot.CenterCard => FALLBACK_CENTER_CARD_POSITION,
                EQuestionCardSlot.RightCard => FALLBACK_RIGHT_CARD_POSITION,
                _ => FALLBACK_CENTER_CARD_POSITION,
            };

            if (mUseWorldEnvironmentLayout == false)
            {
                return fallbackPosition;
            }

            Transform anchorTransform = mCardPresentationAnchorSet.GetAnchor(questionCardSlot);
            Vector2 anchoredPosition;
            return tryProjectWorldAnchor(anchorTransform, fallbackPosition, out anchoredPosition)
                ? anchoredPosition
                : fallbackPosition;
        }

        private Vector2 getMouthAnchorPosition()
        {
            Vector2 mouthCenter;
            Vector2 ignoredMouthSize;
            if (isTempleApproachSceneActive() && tryGetActiveStageMouthLayout(out mouthCenter, out ignoredMouthSize))
            {
                return mouthCenter;
            }

            Vector2 anchoredPosition;
            return tryProjectWorldAnchor(mMouthAnchorSet != null ? mMouthAnchorSet.TruthMouth : null, FALLBACK_MOUTH_POSITION, out anchoredPosition)
                ? anchoredPosition
                : FALLBACK_MOUTH_POSITION;
        }

        private Vector2 getTempleApproachMouthCanvasPosition()
        {
            if (mTempleApproachMouthImage == null || mCanvasRootRectTransform == null)
            {
                return getMouthAnchorPosition();
            }

            RectTransform mouthRectTransform = mTempleApproachMouthImage.rectTransform;
            Vector3 mouthWorldPosition = mouthRectTransform.TransformPoint(mouthRectTransform.rect.center);
            return mCanvasRootRectTransform.InverseTransformPoint(mouthWorldPosition);
        }

        private Vector2 getHandFrontPosition()
        {
            Vector2 mouthCenter;
            Vector2 mouthSize;
            if (isTempleApproachSceneActive() && tryGetActiveStageMouthLayout(out mouthCenter, out mouthSize))
            {
                return mouthCenter + new Vector2(mouthSize.x * TEMPLE_HAND_FRONT_OFFSET_FACTOR.x, mouthSize.y * TEMPLE_HAND_FRONT_OFFSET_FACTOR.y);
            }

            Vector2 anchoredPosition;
            return tryProjectWorldAnchor(mMouthAnchorSet != null ? mMouthAnchorSet.MouthFrontAnchor : null, FALLBACK_HAND_FRONT_POSITION, out anchoredPosition)
                ? anchoredPosition
                : FALLBACK_HAND_FRONT_POSITION;
        }

        private Vector2 getHandInnerPosition()
        {
            Vector2 mouthCenter;
            Vector2 mouthSize;
            if (isTempleApproachSceneActive() && tryGetActiveStageMouthLayout(out mouthCenter, out mouthSize))
            {
                return mouthCenter + new Vector2(mouthSize.x * TEMPLE_HAND_INNER_OFFSET_FACTOR.x, mouthSize.y * TEMPLE_HAND_INNER_OFFSET_FACTOR.y);
            }

            Vector2 anchoredPosition;
            return tryProjectWorldAnchor(mMouthAnchorSet != null ? mMouthAnchorSet.MouthInnerAnchor : null, FALLBACK_HAND_INNER_POSITION, out anchoredPosition)
                ? anchoredPosition
                : FALLBACK_HAND_INNER_POSITION;
        }

        private bool tryProjectWorldAnchor(Transform worldAnchorTransform, Vector2 fallbackPosition, out Vector2 anchoredPosition)
        {
            anchoredPosition = fallbackPosition;

            if (mUseWorldEnvironmentLayout == false || worldAnchorTransform == null || mWorldCamera == null)
            {
                return false;
            }

            Vector3 screenPosition = mWorldCamera.WorldToScreenPoint(worldAnchorTransform.position);

            if (screenPosition.z <= 0.0f)
            {
                return false;
            }

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvasRootRectTransform, screenPosition, null, out anchoredPosition);
        }

        private bool tryConvertScreenPointToCanvasPosition(Vector2 screenPosition, out Vector2 anchoredPosition)
        {
            anchoredPosition = default;

            return mCanvasRootRectTransform != null
                && RectTransformUtility.ScreenPointToLocalPointInRectangle(mCanvasRootRectTransform, screenPosition, null, out anchoredPosition);
        }

        private void applyStartScreenLayout()
        {
            setRectTransformLayout(mLogoImage.rectTransform, new Vector2(0.5f, 0.55f), new Vector2(1000.0f, 560.0f));
            setRectTransformLayout(mStartButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.13f), new Vector2(520.0f, 150.0f));
            applyTopLeftExitButtonLayout();
        }

        private void applyCardSelectionLayout()
        {
            setRectTransformLayout(mPromptText.rectTransform, new Vector2(0.5f, 0.07f), new Vector2(1080.0f, 64.0f));
            mPromptText.fontSize = 30;
            applyTopLeftExitButtonLayout();
        }

        private void applyHandPromptPanelLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(mQuestionPanelImage.rectTransform, new Vector2(0.5f, 0.105f), new Vector2(1500.0f, 122.0f));
            setRectTransformLayout(mQuestionText.rectTransform, new Vector2(0.5f, 0.105f), new Vector2(1320.0f, 70.0f));
            setRectTransformLayout(mHandImage.rectTransform, new Vector2(0.5f, 0.21f), HELD_POINTER_CURSOR_SIZE_PIXELS);
            mQuestionText.fontSize = 30;
            mQuestionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void applyNarrationLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(mMouthImage.rectTransform, new Vector2(0.5f, 0.53f), new Vector2(640.0f, 640.0f));
            mMouthImage.rectTransform.localScale = Vector3.one;
            setRectTransformLayout(mQuestionPanelImage.rectTransform, new Vector2(0.5f, 0.105f), new Vector2(1500.0f, 122.0f));
            setRectTransformLayout(mQuestionText.rectTransform, new Vector2(0.5f, 0.105f), new Vector2(1320.0f, 70.0f));
            mQuestionText.fontSize = 30;
            mQuestionText.alignment = TextAnchor.MiddleCenter;
            mQuestionText.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        private void applyAwaitingHandInsertionLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(mMouthImage.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(700.0f, 700.0f));
            mMouthImage.rectTransform.localScale = Vector3.one;
            setRectTransformLayout(mQuestionPanelImage.rectTransform, new Vector2(0.5f, 0.105f), new Vector2(1500.0f, 122.0f));
            setRectTransformLayout(mQuestionText.rectTransform, new Vector2(0.5f, 0.105f), new Vector2(1320.0f, 70.0f));
            setRectTransformLayout(mHandImage.rectTransform, new Vector2(0.5f, 0.22f), HELD_POINTER_CURSOR_SIZE_PIXELS);
            mQuestionText.fontSize = 30;
            mQuestionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void applyAnswerStageLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(mMouthImage.rectTransform, new Vector2(0.5f, 0.60f), new Vector2(760.0f, 760.0f));
            mMouthImage.rectTransform.localScale = Vector3.one;
            setRectTransformLayout(mQuestionPanelImage.rectTransform, new Vector2(0.5f, 0.105f), new Vector2(1500.0f, 122.0f));
            setRectTransformLayout(mQuestionText.rectTransform, new Vector2(0.5f, 0.105f), new Vector2(1320.0f, 70.0f));
            setRectTransformLayout(mHandImage.rectTransform, new Vector2(0.5f, 0.21f), HELD_POINTER_CURSOR_SIZE_PIXELS);
            mQuestionText.fontSize = 30;
            mQuestionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void applyAnsweringFocusLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(mMouthImage.rectTransform, ANSWERING_FOCUS_MOUTH_ANCHOR, ANSWERING_FOCUS_MOUTH_SIZE_PIXELS);
            mMouthImage.rectTransform.anchoredPosition = Vector2.zero;
            mMouthImage.rectTransform.localScale = Vector3.one;
            mMouthImage.color = Color.white;
            syncMouthEffectImageLayout(mMouthListeningAuraImage, 1.26f);
            syncMouthEffectImageLayout(mMouthAnalyzingAuraImage, 1.18f);
        }

        private void updateMouthEffectImage(Image effectImage, Color color, float scale, float rotationDegrees)
        {
            if (effectImage == null || effectImage.gameObject.activeSelf == false || mMouthImage == null)
            {
                return;
            }

            syncMouthEffectImageLayout(effectImage, 1.28f);
            effectImage.color = color;
            effectImage.rectTransform.localScale = mMouthImage.rectTransform.localScale * Mathf.Max(0.01f, scale);
            effectImage.rectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, rotationDegrees);
            placeMouthEffectImagesBehindMouth();
        }

        private void updateAnsweringEyeBeamImages(float elapsedSeconds, float quickPulse)
        {
            if (mMouthImage == null)
            {
                return;
            }

            float mouthWidth = mMouthImage.rectTransform.sizeDelta.x;
            float mouthHeight = mMouthImage.rectTransform.sizeDelta.y;
            float sweepProgress = Mathf.PingPong(elapsedSeconds * ANSWER_BEAM_SWEEP_RATE, 1.0f);
            float easedSweepProgress = easeInOut(sweepProgress);
            float sourceYOffset = mouthHeight * ANSWER_BEAM_SOURCE_Y_FACTOR;
            float endYOffset = mouthHeight * Mathf.Lerp(ANSWER_BEAM_END_BOTTOM_Y_FACTOR, ANSWER_BEAM_END_TOP_Y_FACTOR, easedSweepProgress);
            float beamHeight = Mathf.Max(1.0f, Mathf.Abs(sourceYOffset - endYOffset));
            float beamVerticalScale = endYOffset > sourceYOffset ? -1.0f : 1.0f;
            float beamAlpha = Mathf.Lerp(0.44f, 0.74f, Mathf.Pow(quickPulse, 1.18f));
            Vector2 beamSize = new Vector2(mouthWidth * 1.003f, beamHeight);
            Color beamColor = new Color(0.95f, 0.20f, 0.14f, beamAlpha);
            updateEyeBeamImage(mMouthLeftEyeBeamImage, new Vector2(-(mouthWidth * 0.084f), sourceYOffset), beamSize, beamColor, -2.6f, new Vector2(0.5f, 1.0f), beamVerticalScale);
            updateEyeBeamImage(mMouthRightEyeBeamImage, new Vector2(mouthWidth * 0.058f, sourceYOffset), beamSize, beamColor, 2.6f, new Vector2(0.5f, 1.0f), beamVerticalScale);
        }

        private void updateEyeBeamImage(Image beamImage, Vector2 offsetFromMouthCenter, Vector2 sizeDelta, Color color, float rotationDegrees, Vector2 pivot, float verticalScale)
        {
            if (beamImage == null || beamImage.gameObject.activeSelf == false || mMouthImage == null)
            {
                return;
            }

            RectTransform mouthRectTransform = mMouthImage.rectTransform;
            RectTransform beamRectTransform = beamImage.rectTransform;
            beamRectTransform.anchorMin = mouthRectTransform.anchorMin;
            beamRectTransform.anchorMax = mouthRectTransform.anchorMax;
            beamRectTransform.pivot = pivot;
            beamRectTransform.anchoredPosition = mouthRectTransform.anchoredPosition + offsetFromMouthCenter;
            beamRectTransform.sizeDelta = sizeDelta;
            beamRectTransform.localScale = new Vector3(1.0f, verticalScale, 1.0f);
            beamRectTransform.localRotation = Quaternion.Euler(0.0f, 0.0f, rotationDegrees);
            beamImage.color = color;
            placeEyeBeamImagesAboveMouth();
        }

        private void syncMouthEffectImageLayout(Image effectImage, float sizeMultiplier)
        {
            if (effectImage == null || mMouthImage == null)
            {
                return;
            }

            RectTransform mouthRectTransform = mMouthImage.rectTransform;
            RectTransform effectRectTransform = effectImage.rectTransform;
            effectRectTransform.anchorMin = mouthRectTransform.anchorMin;
            effectRectTransform.anchorMax = mouthRectTransform.anchorMax;
            effectRectTransform.anchoredPosition = mouthRectTransform.anchoredPosition;
            effectRectTransform.sizeDelta = mouthRectTransform.sizeDelta * Mathf.Max(0.01f, sizeMultiplier);
            effectRectTransform.localRotation = Quaternion.identity;
        }

        private void setMouthEffectImagesActive(bool isListeningAuraActive, bool isAnalyzingAuraActive)
        {
            setObjectActive(mMouthListeningAuraImage, isListeningAuraActive);
            setObjectActive(mMouthAnalyzingAuraImage, isAnalyzingAuraActive);
            placeMouthEffectImagesBehindMouth();
        }

        private void setEyeBeamImagesActive(bool isActive)
        {
            setObjectActive(mMouthLeftEyeBeamImage, isActive);
            setObjectActive(mMouthRightEyeBeamImage, isActive);
            placeEyeBeamImagesAboveMouth();
        }

        private void placeMouthEffectImagesBehindMouth()
        {
            if (mMouthImage == null)
            {
                return;
            }

            int mouthSiblingIndex = mMouthImage.transform.GetSiblingIndex();

            if (mMouthListeningAuraImage != null)
            {
                int targetSiblingIndex = Mathf.Min(mCanvasRootTransform.childCount - 1, mouthSiblingIndex + 1);
                mMouthListeningAuraImage.transform.SetSiblingIndex(targetSiblingIndex);
            }

            if (mMouthAnalyzingAuraImage != null)
            {
                int targetSiblingIndex = Mathf.Min(mCanvasRootTransform.childCount - 1, mouthSiblingIndex + 2);
                mMouthAnalyzingAuraImage.transform.SetSiblingIndex(targetSiblingIndex);
            }
        }

        private void placeEyeBeamImagesAboveMouth()
        {
            if (mMouthImage == null)
            {
                return;
            }

            int mouthSiblingIndex = mMouthImage.transform.GetSiblingIndex();
            int targetSiblingIndex = Mathf.Min(mCanvasRootTransform.childCount - 1, mouthSiblingIndex + 1);

            if (mMouthLeftEyeBeamImage != null)
            {
                mMouthLeftEyeBeamImage.transform.SetSiblingIndex(targetSiblingIndex);
            }

            if (mMouthRightEyeBeamImage != null)
            {
                mMouthRightEyeBeamImage.transform.SetSiblingIndex(targetSiblingIndex);
            }
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
