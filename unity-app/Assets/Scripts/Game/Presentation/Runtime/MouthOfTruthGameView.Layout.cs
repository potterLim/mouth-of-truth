using System.Collections.Generic;
using MouthOfTruth.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        private void cacheWorldPresentationReferences()
        {
            mWorldCamera = Camera.main;

            if (mWorldCamera == null)
            {
                mWorldCamera = FindAnyObjectByType<Camera>();
            }

            mCardPresentationAnchorSet = FindAnyObjectByType<CardPresentationAnchorSet>();
            mMouthAnchorSet = FindAnyObjectByType<MouthAnchorSet>();
            mUseWorldEnvironmentLayout = hasWorldEnvironmentLayoutReferences();
        }

        private bool hasWorldEnvironmentLayoutReferences()
        {
            return mWorldCamera != null
                && mCardPresentationAnchorSet != null
                && mCardPresentationAnchorSet.HasRequiredAnchors()
                && mMouthAnchorSet != null
                && mMouthAnchorSet.HasRequiredAnchors();
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
            setRectTransformLayout(
                mLogoImage.rectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.55f), new Vector2(1000.0f, 560.0f)));
            setRectTransformLayout(
                mStartButton.GetComponent<RectTransform>(),
                UiRectLayout.At(new Vector2(0.5f, 0.13f), new Vector2(520.0f, 150.0f)));
            applyTopLeftExitButtonLayout();
        }

        private void applyCardSelectionLayout()
        {
            setRectTransformLayout(
                mPromptText.rectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.07f), new Vector2(1080.0f, 64.0f)));
            mPromptText.fontSize = 30;
            applyTopLeftExitButtonLayout();
        }

        private void applyQuestionPanelLayout()
        {
            setRectTransformLayout(
                mQuestionPanelImage.rectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.105f), new Vector2(1500.0f, 122.0f)));
            setRectTransformLayout(
                mQuestionText.rectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.105f), new Vector2(1320.0f, 70.0f)));
        }

        private void applyHandPromptPanelLayout()
        {
            applyTopLeftExitButtonLayout();
            applyQuestionPanelLayout();
            setRectTransformLayout(mHandImage.rectTransform, UiRectLayout.At(new Vector2(0.5f, 0.21f), HELD_POINTER_CURSOR_SIZE_PIXELS));
            mQuestionText.fontSize = 30;
            mQuestionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void applyNarrationLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(
                mMouthImage.rectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.53f), new Vector2(640.0f, 640.0f)));
            mMouthImage.rectTransform.localScale = Vector3.one;
            applyQuestionPanelLayout();
            mQuestionText.fontSize = 30;
            mQuestionText.alignment = TextAnchor.MiddleCenter;
            mQuestionText.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        private void applyAwaitingHandInsertionLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(
                mMouthImage.rectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.56f), new Vector2(700.0f, 700.0f)));
            mMouthImage.rectTransform.localScale = Vector3.one;
            applyQuestionPanelLayout();
            setRectTransformLayout(mHandImage.rectTransform, UiRectLayout.At(new Vector2(0.5f, 0.22f), HELD_POINTER_CURSOR_SIZE_PIXELS));
            mQuestionText.fontSize = 30;
            mQuestionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void applyAnswerStageLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(
                mMouthImage.rectTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.60f), new Vector2(760.0f, 760.0f)));
            mMouthImage.rectTransform.localScale = Vector3.one;
            applyQuestionPanelLayout();
            setRectTransformLayout(mHandImage.rectTransform, UiRectLayout.At(new Vector2(0.5f, 0.21f), HELD_POINTER_CURSOR_SIZE_PIXELS));
            mQuestionText.fontSize = 30;
            mQuestionText.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private void applyAnsweringFocusLayout()
        {
            applyTopLeftExitButtonLayout();
            setRectTransformLayout(mMouthImage.rectTransform, UiRectLayout.At(ANSWERING_FOCUS_MOUTH_ANCHOR, ANSWERING_FOCUS_MOUTH_SIZE_PIXELS));
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
            updateEyeBeamImage(
                mMouthLeftEyeBeamImage,
                new Vector2(-(mouthWidth * 0.084f), sourceYOffset),
                beamSize,
                beamColor,
                -2.6f,
                new Vector2(0.5f, 1.0f),
                beamVerticalScale);
            updateEyeBeamImage(
                mMouthRightEyeBeamImage,
                new Vector2(mouthWidth * 0.058f, sourceYOffset),
                beamSize,
                beamColor,
                2.6f,
                new Vector2(0.5f, 1.0f),
                beamVerticalScale);
        }

        private void updateEyeBeamImage(
            Image beamImage,
            Vector2 offsetFromMouthCenter,
            Vector2 sizeDelta,
            Color color,
            float rotationDegrees,
            Vector2 pivot,
            float verticalScale)
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

        private void setMouthEffectVisualState(EMouthEffectVisualState mouthEffectVisualState)
        {
            EUiElementVisibility auraVisibility = mouthEffectVisualState == EMouthEffectVisualState.ListeningAndAnalyzing
                ? EUiElementVisibility.Visible
                : EUiElementVisibility.Hidden;
            setObjectVisibility(mMouthListeningAuraImage, auraVisibility);
            setObjectVisibility(mMouthAnalyzingAuraImage, auraVisibility);
            placeMouthEffectImagesBehindMouth();
        }

        private void setEyeBeamImagesVisibility(EUiElementVisibility visibility)
        {
            setObjectVisibility(mMouthLeftEyeBeamImage, visibility);
            setObjectVisibility(mMouthRightEyeBeamImage, visibility);
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
    }
}
