using System.Collections.Generic;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
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

        private enum EEyeBeamSourceSide
        {
            Left,
            Right,
        }

        private enum EMouthEffectVisualState
        {
            Hidden,
            ListeningAndAnalyzing,
        }
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

        public SecondsDuration HandPromptPanelHoldDuration => getHandPromptPanelHoldDuration();
    }
}
