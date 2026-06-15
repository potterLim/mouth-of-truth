using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        public async Task PlayTempleApproachToCardSelectionAsync()
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
            setObjectVisibility(mBackgroundImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mCarpetImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
            setObjectVisibility(mExitButton, EUiElementVisibility.Visible);
            createTempleApproachScene();
            setOverlayTint(STAGE_OVERLAY_TINT, TEMPLE_APPROACH_START_OVERLAY_ALPHA);
            hideFirstRunTutorialPresentation();

            await animateOverTimeAsync(
                new SecondsDuration(TEMPLE_APPROACH_FORWARD_DURATION_SECONDS),
                progress =>
                {
                    float travelProgress = easeInOut(progress);
                    float cameraScale = Mathf.Lerp(1.0f, TEMPLE_APPROACH_STAIR_START_SCALE, travelProgress);
                    float walkingBob = Mathf.Sin(travelProgress * Mathf.PI * 4.0f) * 0.8f;
                    mTempleApproachCameraRectTransform.localScale = Vector3.one * cameraScale;
                    mTempleApproachCameraRectTransform.anchoredPosition = new Vector2(0.0f, walkingBob);
                    mTempleApproachMouthImage.color = new Color(1.0f, 1.0f, 1.0f, 0.86f);
                    float overlayAlpha = Mathf.Lerp(TEMPLE_APPROACH_START_OVERLAY_ALPHA, TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA, easeOut(progress));
                    setOverlayTint(STAGE_OVERLAY_TINT, overlayAlpha);
                });

            mTempleApproachCameraRectTransform.localScale = Vector3.one * TEMPLE_APPROACH_STAIR_START_SCALE;
            mTempleApproachCameraRectTransform.anchoredPosition = Vector2.zero;
            setOverlayTint(STAGE_OVERLAY_TINT, TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA);
            await fadeTempleApproachMouthAlphaAsync(CARD_SELECTION_DIM_MOUTH_ALPHA, new SecondsDuration(TEMPLE_APPROACH_MOUTH_DIM_SECONDS));
        }

        public Task PrepareTempleGameplayBackdropAsync()
        {
            if (isTempleApproachSceneActive() == false)
            {
                return Task.CompletedTask;
            }

            setObjectVisibility(mBackgroundImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mCarpetImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
            setOverlayTint(STAGE_OVERLAY_TINT, TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA);
            setObjectVisibility(mMouthImage, EUiElementVisibility.Visible);
            setTempleApproachMouthAlpha(1.0f);
            syncTempleStageMouthOverlay(0.0f);
            return Task.CompletedTask;
        }

        private void createTempleApproachScene()
        {
            destroyTempleApproachScene();

            mTempleApproachCameraObject = new GameObject("TempleApproachCameraRoot", typeof(RectTransform));
            mTempleApproachCameraObject.transform.SetParent(mCanvasRootTransform, false);
            mTempleApproachCameraObject.transform.SetAsFirstSibling();
            mTempleApproachCameraRectTransform = mTempleApproachCameraObject.GetComponent<RectTransform>();
            mTempleApproachCameraRectTransform.anchorMin = Vector2.zero;
            mTempleApproachCameraRectTransform.anchorMax = Vector2.one;
            mTempleApproachCameraRectTransform.offsetMin = Vector2.zero;
            mTempleApproachCameraRectTransform.offsetMax = Vector2.zero;
            mTempleApproachCameraRectTransform.pivot = new Vector2(0.5f, 0.54f);

            Image approachBackgroundImage = createFullScreenImage("TempleApproachBackground", mTempleApproachCameraRectTransform, mBackgroundImage.color);
            approachBackgroundImage.sprite = mBackgroundImage.sprite;
            approachBackgroundImage.type = mBackgroundImage.type;
            approachBackgroundImage.preserveAspect = mBackgroundImage.preserveAspect;
            approachBackgroundImage.raycastTarget = false;

            Image approachCarpetImage = createImage(
                "TempleApproachCarpet",
                mTempleApproachCameraRectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.0f), STAGE_CARPET_POSITION, STAGE_CARPET_SIZE),
                STAGE_CARPET_TINT);
            approachCarpetImage.sprite = mCarpetImage.sprite;
            approachCarpetImage.raycastTarget = false;

            mTempleApproachMouthImage = createImage(
                "TempleApproachMouth",
                mTempleApproachCameraRectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.5f), TEMPLE_APPROACH_MOUTH_POSITION, TEMPLE_APPROACH_MOUTH_SIZE),
                new Color(1.0f, 1.0f, 1.0f, 0.86f));
            mTempleApproachMouthImage.sprite = mMouthImage.sprite;
            mTempleApproachMouthImage.preserveAspect = true;
            mTempleApproachMouthImage.raycastTarget = false;
        }

        private void destroyTempleApproachScene()
        {
            if (mTempleApproachCameraObject != null)
            {
                Destroy(mTempleApproachCameraObject);
            }

            mTempleApproachCameraObject = null;
            mTempleApproachCameraRectTransform = null;
            mTempleApproachMouthImage = null;
        }

        private void prepareCardLaunchPresentation(ECardLaunchPresentationMode cardLaunchPresentationMode)
        {
            if (cardLaunchPresentationMode == ECardLaunchPresentationMode.TempleApproach)
            {
                setObjectVisibility(mBackgroundImage, EUiElementVisibility.Hidden);
                setObjectVisibility(mCarpetImage, EUiElementVisibility.Hidden);
                setObjectVisibility(mMouthImage, EUiElementVisibility.Hidden);
                setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
                setOverlayTint(STAGE_OVERLAY_TINT, TEMPLE_APPROACH_STAGE_OVERLAY_ALPHA);
                setObjectVisibility(mPromptText, EUiElementVisibility.Hidden);
                setObjectVisibility(mStatusText, EUiElementVisibility.Hidden);
                setObjectVisibility(mQuestionPanelImage, EUiElementVisibility.Hidden);
                setObjectVisibility(mQuestionText, EUiElementVisibility.Hidden);
                setObjectVisibility(mRitualHandImage, EUiElementVisibility.Hidden);
                return;
            }

            applyNarrationLayout();
            mBackgroundImage.sprite = mMouthChamberBackgroundSprite;
            setBackgroundTint(STAGE_BACKGROUND_TINT);
            setObjectVisibility(mBackgroundImage, EUiElementVisibility.Visible);
            setObjectVisibility(mCarpetImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
            setOverlayAlpha(0.18f);
            setObjectVisibility(mPromptText, EUiElementVisibility.Hidden);
            setObjectVisibility(mStatusText, EUiElementVisibility.Hidden);
            setObjectVisibility(mQuestionPanelImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mQuestionText, EUiElementVisibility.Hidden);
            setObjectVisibility(mMouthImage, EUiElementVisibility.Visible);
            setObjectVisibility(mRitualHandImage, EUiElementVisibility.Hidden);
            applyMouthAnchoredLayout();
            mMouthImage.rectTransform.localScale = Vector3.one * 0.94f;
        }

        private bool isTempleApproachSceneActive()
        {
            return mTempleApproachCameraObject != null && mTempleApproachCameraRectTransform != null;
        }

        private void applyTempleStageBackgroundPresentation(float overlayAlpha)
        {
            setObjectVisibility(mBackgroundImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mCarpetImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
            setOverlayTint(STAGE_OVERLAY_TINT, overlayAlpha);
            setObjectVisibility(mMouthImage, EUiElementVisibility.Visible);
            setTempleApproachMouthAlpha(1.0f);
            syncTempleStageMouthOverlay(0.0f);
        }

        private async Task fadeTempleApproachMouthAlphaAsync(float targetAlpha, SecondsDuration duration)
        {
            if (mTempleApproachMouthImage == null)
            {
                return;
            }

            float startAlpha = mTempleApproachMouthImage.color.a;

            await animateOverTimeAsync(
                duration,
                progress =>
                {
                    float easedProgress = easeInOut(progress);
                    setTempleApproachMouthAlpha(Mathf.Lerp(startAlpha, targetAlpha, easedProgress));
                });

            setTempleApproachMouthAlpha(targetAlpha);
        }

        private void setTempleApproachMouthAlpha(float alpha)
        {
            if (mTempleApproachMouthImage == null)
            {
                return;
            }

            float clampedAlpha = Mathf.Clamp01(alpha);
            Color currentColor = mTempleApproachMouthImage.color;
            mTempleApproachMouthImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, clampedAlpha);
        }

        private void setTempleApproachMouthColor(Color color)
        {
            if (mTempleApproachMouthImage == null)
            {
                return;
            }

            mTempleApproachMouthImage.color = color;
        }

        private void syncTempleStageMouthOverlay(float alpha)
        {
            Vector2 center;
            Vector2 size;
            if (mMouthImage == null || tryGetTempleApproachMouthCanvasLayout(out center, out size) == false)
            {
                return;
            }

            RectTransform mouthRectTransform = mMouthImage.rectTransform;
            mouthRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            mouthRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            mouthRectTransform.anchoredPosition = center;
            mouthRectTransform.sizeDelta = size;
            mouthRectTransform.localRotation = Quaternion.identity;
            mouthRectTransform.localScale = Vector3.one;
            mMouthImage.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp01(alpha));
        }

        private TempleCameraTransition captureTempleCameraTransition(TempleCameraScale targetScale, Vector2 targetMouthCenter)
        {
            return new TempleCameraTransition(
                mTempleApproachCameraRectTransform.localScale.x,
                mTempleApproachCameraRectTransform.anchoredPosition,
                getTempleCameraPositionForCenteredMouth(targetScale.Value, targetMouthCenter));
        }

        private void setTempleCameraPose(float scale, float yOffset)
        {
            setTempleCameraPose(scale, yOffset, 0.0f);
        }

        private void setTempleCameraPose(float scale, float yOffset, float xOffset)
        {
            if (mTempleApproachCameraRectTransform == null)
            {
                return;
            }

            mTempleApproachCameraRectTransform.localScale = Vector3.one * scale;
            mTempleApproachCameraRectTransform.anchoredPosition = new Vector2(xOffset, yOffset);
            syncTempleStageMouthOverlay(0.0f);
        }

        private void setTempleCameraPoseCenteredOnMouth(float scale, Vector2 targetMouthCenter)
        {
            setTempleCameraPoseCenteredOnMouth(scale, targetMouthCenter, 0.0f, 0.0f);
        }

        private void setTempleCameraPoseCenteredOnMouth(float scale, Vector2 targetMouthCenter, float xOffset, float yOffset)
        {
            Vector2 centeredPosition = getTempleCameraPositionForCenteredMouth(scale, targetMouthCenter);
            setTempleCameraPose(scale, centeredPosition.y + yOffset, centeredPosition.x + xOffset);
        }

        private Vector2 getTempleCameraPositionForCenteredMouth(float scale, Vector2 targetMouthCenter)
        {
            if (mTempleApproachCameraRectTransform == null)
            {
                return Vector2.zero;
            }

            Vector3 previousScale = mTempleApproachCameraRectTransform.localScale;
            Vector2 previousPosition = mTempleApproachCameraRectTransform.anchoredPosition;

            mTempleApproachCameraRectTransform.localScale = Vector3.one * scale;
            mTempleApproachCameraRectTransform.anchoredPosition = Vector2.zero;
            Vector2 mouthCenter;
            Vector2 ignoredMouthSize;
            bool hasMouthLayout = tryGetTempleApproachMouthCanvasLayout(out mouthCenter, out ignoredMouthSize);

            mTempleApproachCameraRectTransform.localScale = previousScale;
            mTempleApproachCameraRectTransform.anchoredPosition = previousPosition;
            syncTempleStageMouthOverlay(0.0f);

            if (hasMouthLayout == false)
            {
                return previousPosition;
            }

            return targetMouthCenter - mouthCenter;
        }

        private bool tryGetTempleApproachMouthCanvasLayout(out Vector2 center, out Vector2 size)
        {
            center = Vector2.zero;
            size = Vector2.zero;

            if (mTempleApproachMouthImage == null || mCanvasRootRectTransform == null)
            {
                return false;
            }

            RectTransform mouthRectTransform = mTempleApproachMouthImage.rectTransform;
            mouthRectTransform.GetWorldCorners(mTempleMouthWorldCorners);
            Vector2 minimum = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maximum = new Vector2(float.MinValue, float.MinValue);

            for (int index = 0; index < mTempleMouthWorldCorners.Length; index += 1)
            {
                Vector2 canvasPoint = mCanvasRootRectTransform.InverseTransformPoint(mTempleMouthWorldCorners[index]);
                minimum = Vector2.Min(minimum, canvasPoint);
                maximum = Vector2.Max(maximum, canvasPoint);
            }

            center = (minimum + maximum) * 0.5f;
            size = maximum - minimum;
            return size.x > 1.0f && size.y > 1.0f;
        }

        private bool tryGetActiveStageMouthLayout(out Vector2 center, out Vector2 size)
        {
            center = Vector2.zero;
            size = Vector2.zero;

            if (isTempleApproachSceneActive() && tryGetTempleApproachMouthCanvasLayout(out center, out size))
            {
                return true;
            }

            if (mMouthImage == null || mMouthImage.gameObject.activeInHierarchy == false)
            {
                return false;
            }

            RectTransform mouthRectTransform = mMouthImage.rectTransform;
            center = mouthRectTransform.anchoredPosition;
            size = new Vector2(mouthRectTransform.rect.width * mouthRectTransform.localScale.x, mouthRectTransform.rect.height * mouthRectTransform.localScale.y);
            return size.x > 1.0f && size.y > 1.0f;
        }

        private readonly struct TempleCameraTransition
        {
            private readonly float mStartScale;
            private readonly Vector2 mStartPosition;
            private readonly Vector2 mTargetPosition;

            public TempleCameraTransition(float startScale, Vector2 startPosition, Vector2 targetPosition)
            {
                mStartScale = startScale;
                mStartPosition = startPosition;
                mTargetPosition = targetPosition;
            }

            public float GetScale(TempleCameraScale targetScale, NormalizedProgress progress)
            {
                return Mathf.Lerp(mStartScale, targetScale.Value, progress.Value);
            }

            public Vector2 GetPosition(NormalizedProgress progress)
            {
                return Vector2.Lerp(mStartPosition, mTargetPosition, progress.Value);
            }
        }
    }
}
