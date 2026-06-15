using System;
using MouthOfTruth.Game.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        private void setRectTransformLayout(RectTransform rectTransform, UiRectLayout rectLayout)
        {
            if (rectTransform == null)
            {
                return;
            }

            applyRectTransformLayout(rectTransform, rectLayout);
        }

        private void buildCanvas()
        {
            mCanvas = gameObject.AddComponent<Canvas>();
            mCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mCanvas.sortingOrder = 10;
            gameObject.AddComponent<GraphicRaycaster>();
            CanvasScaler canvasScaler = gameObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920.0f, 1080.0f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            GameObject canvasRootObject = new GameObject("CanvasRoot", typeof(RectTransform));
            canvasRootObject.transform.SetParent(transform, false);
            mCanvasRootTransform = canvasRootObject.transform;
            mCanvasRootRectTransform = canvasRootObject.GetComponent<RectTransform>();
            mCanvasRootRectTransform.anchorMin = Vector2.zero;
            mCanvasRootRectTransform.anchorMax = Vector2.one;
            mCanvasRootRectTransform.offsetMin = Vector2.zero;
            mCanvasRootRectTransform.offsetMax = Vector2.zero;

            mBackgroundImage = createFullScreenImage("Background", mCanvasRootTransform, Color.white);
            mSceneOverlayImage = createFullScreenImage("SceneOverlay", mCanvasRootTransform, new Color(0.01f, 0.01f, 0.015f, 0.0f));
            mCarpetImage = createImage(
                "RedCarpet",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.0f), STAGE_CARPET_POSITION, STAGE_CARPET_SIZE),
                STAGE_CARPET_TINT);
            mTitleVignetteImage = createFullScreenImage("TitleVignette", mCanvasRootTransform, new Color(1.0f, 1.0f, 1.0f, 0.55f));
            mLogoImage = createImage(
                "Logo",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.75f), new Vector2(840.0f, 360.0f)),
                Color.white);
            mQuestionPanelImage = createImage(
                "QuestionPanel",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.84f), new Vector2(1280.0f, 170.0f)),
                Color.white);
            mStatusPanelImage = createImage(
                "StatusPanel",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.105f), new Vector2(1320.0f, 150.0f)),
                Color.white);
            mResultPanelImage = createImage(
                "ResultPanel",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.39f), new Vector2(980.0f, 420.0f)),
                Color.white);
            mPromptText = createText(
                "PromptText",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.13f), new Vector2(700.0f, 80.0f)),
                new UiTextStyle(38, FontStyle.Bold));
            mStatusText = createText(
                "StatusText",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.08f), new Vector2(1200.0f, 70.0f)),
                new UiTextStyle(26, FontStyle.Bold));
            mQuestionText = createText(
                "QuestionText",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.84f), new Vector2(1200.0f, 140.0f)),
                new UiTextStyle(34, FontStyle.Bold));
            mAnalyzingDotsText = createText(
                "AnalyzingDotsText",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.57f), new Vector2(360.0f, 140.0f)),
                new UiTextStyle(90, FontStyle.Bold));
            mAnswerTimerText = createText(
                "AnswerTimerText",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.85f, 0.92f), new Vector2(320.0f, 50.0f)),
                new UiTextStyle(22, FontStyle.Normal));
            mMouthImage = createImage(
                "TruthMouth",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.50f), new Vector2(0.0f, 60.0f), new Vector2(430.0f, 430.0f)),
                Color.white);
            mMouthListeningAuraImage = createImage(
                "MouthListeningAura",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.50f), new Vector2(0.0f, 60.0f), new Vector2(640.0f, 640.0f)),
                Color.clear);
            mMouthListeningAuraImage.sprite = createRadialGlowSprite();
            mMouthListeningAuraImage.raycastTarget = false;
            mMouthAnalyzingAuraImage = createImage(
                "MouthAnalyzingAura",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.50f), new Vector2(0.0f, 60.0f), new Vector2(720.0f, 720.0f)),
                Color.clear);
            mMouthAnalyzingAuraImage.sprite = createRingGlowSprite();
            mMouthAnalyzingAuraImage.raycastTarget = false;
            mMouthLeftEyeBeamImage = createImage(
                "MouthLeftEyeBeam",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.50f), new Vector2(0.0f, 60.0f), new Vector2(360.0f, 72.0f)),
                Color.clear);
            mMouthLeftEyeBeamImage.sprite = createEyeBeamSprite(EEyeBeamSourceSide.Left);
            mMouthLeftEyeBeamImage.type = Image.Type.Simple;
            mMouthLeftEyeBeamImage.raycastTarget = false;
            mMouthRightEyeBeamImage = createImage(
                "MouthRightEyeBeam",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.50f), new Vector2(0.0f, 60.0f), new Vector2(360.0f, 72.0f)),
                Color.clear);
            mMouthRightEyeBeamImage.sprite = createEyeBeamSprite(EEyeBeamSourceSide.Right);
            mMouthRightEyeBeamImage.type = Image.Type.Simple;
            mMouthRightEyeBeamImage.raycastTarget = false;
            placeMouthEffectImagesBehindMouth();
            mHandImage = createImage(
                "HeldPointer",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.22f), HELD_POINTER_CURSOR_SIZE_PIXELS),
                Color.white);
            mRitualHandImage = createImage(
                "RitualHand",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.22f), RITUAL_HAND_SIZE_PIXELS),
                Color.white);
            mPointerImage = createImage(
                "InputPointer",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.5f), POINTER_CURSOR_SIZE_PIXELS),
                Color.white);
            mVerdictImage = createImage(
                "VerdictImage",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.63f), new Vector2(820.0f, 240.0f)),
                Color.white);
            mVerdictText = createText(
                "VerdictText",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.49f), new Vector2(640.0f, 80.0f)),
                new UiTextStyle(48, FontStyle.Bold));
            mAnswerInputField = createInputField();
            mStartButton = createButton(
                "StartButton",
                "START GAME",
                UiRectLayout.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.22f), new Vector2(280.0f, 80.0f)),
                () => mStartRequested = true);
            mTryAgainButton = createButton(
                "TryAgainButton",
                "TRY AGAIN",
                UiRectLayout.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.26f), new Vector2(280.0f, 80.0f)),
                () => mTryAgainRequested = true);
            mBackToTitleButton = createButton(
                "BackToTitleButton",
                "BACK TO TITLE",
                UiRectLayout.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.18f), new Vector2(340.0f, 72.0f)),
                () => mBackToTitleRequested = true);
            mExitButton = createButton(
                "ExitButton",
                "EXIT GAME",
                UiRectLayout.At(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.12f), new Vector2(320.0f, 72.0f)),
                () => mExitRequested = true);

            mTitleVignetteImage.transform.SetSiblingIndex(mLogoImage.transform.GetSiblingIndex());
            placeImageBehindText(mQuestionPanelImage.transform, mQuestionText.transform);
            placeImageBehindText(mStatusPanelImage.transform, mPromptText.transform);
            placeImageBehindText(mResultPanelImage.transform, mVerdictImage.transform);

            createCardView(EQuestionCardSlot.LeftCard, FALLBACK_LEFT_CARD_POSITION);
            createCardView(EQuestionCardSlot.CenterCard, FALLBACK_CENTER_CARD_POSITION);
            createCardView(EQuestionCardSlot.RightCard, FALLBACK_RIGHT_CARD_POSITION);
            mPointerImage.transform.SetAsLastSibling();
            mPointerImage.raycastTarget = false;
            mAnalyzingDotsText.transform.SetAsLastSibling();
            mTutorialOverlayImage = createFullScreenImage("FirstRunTutorialOverlay", mCanvasRootTransform, new Color(0.0f, 0.0f, 0.0f, 0.0f));
            mTutorialDevicePanelImage = createImage(
                "FirstRunTutorialPanel",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.46f), new Vector2(900.0f, 520.0f)),
                new Color(0.12f, 0.12f, 0.135f, 0.96f));
            mTutorialLeapMotionDeviceImage = createImage(
                "FirstRunTutorialLeapMotionDevice",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.46f), TUTORIAL_LEAP_MOTION_DEVICE_SIZE_PIXELS),
                Color.white);
            mTutorialHandImage = createImage(
                "FirstRunTutorialHand",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.48f), TUTORIAL_HAND_SIZE_PIXELS),
                Color.white);
            mTutorialTitleText = createText(
                "FirstRunTutorialTitle",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.755f), new Vector2(980.0f, 64.0f)),
                new UiTextStyle(34, FontStyle.Bold));
            mTutorialBodyText = createText(
                "FirstRunTutorialBody",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.25f), new Vector2(1040.0f, 72.0f)),
                new UiTextStyle(26, FontStyle.Normal));
            mTutorialStepText = createText(
                "FirstRunTutorialStep",
                mCanvasRootTransform,
                UiRectLayout.At(new Vector2(0.5f, 0.17f), new Vector2(980.0f, 54.0f)),
                new UiTextStyle(24, FontStyle.Bold));
            mTutorialOverlayImage.transform.SetAsLastSibling();
            mTutorialDevicePanelImage.transform.SetAsLastSibling();
            mTutorialLeapMotionDeviceImage.transform.SetAsLastSibling();
            mTutorialHandImage.transform.SetAsLastSibling();
            mTutorialTitleText.transform.SetAsLastSibling();
            mTutorialBodyText.transform.SetAsLastSibling();
            mTutorialStepText.transform.SetAsLastSibling();
            setObjectVisibility(mTutorialOverlayImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialDevicePanelImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialLeapMotionDeviceImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialHandImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialTitleText, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialBodyText, EUiElementVisibility.Hidden);
            setObjectVisibility(mTutorialStepText, EUiElementVisibility.Hidden);
            setMouthEffectVisualState(EMouthEffectVisualState.Hidden);
            setEyeBeamImagesVisibility(EUiElementVisibility.Hidden);
            mLoadingOverlayImage = createFullScreenImage("LoadingOverlay", mCanvasRootTransform, Color.black);
            mLoadingOverlayImage.transform.SetAsLastSibling();
            mLoadingOverlayImage.raycastTarget = true;
        }

        private void ensureEventSystemExists()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void createCardView(EQuestionCardSlot questionCardSlot, Vector2 anchoredPosition)
        {
            GameObject cardObject = new GameObject(questionCardSlot.ToString());
            QuestionCardView questionCardView = cardObject.AddComponent<QuestionCardView>();
            questionCardView.Initialize(questionCardSlot, mCanvasRootTransform, mCardBackSprite, mUiFont, mKoreanFallbackFont);
            questionCardView.SetAnchoredPosition(anchoredPosition);
            mCardViews.Add(questionCardSlot, questionCardView);
        }

        private Image createFullScreenImage(string objectName, Transform parentTransform, Color color)
        {
            return createImage(objectName, parentTransform, UiRectLayout.Fill, color);
        }

        private Image createImage(string objectName, Transform parentTransform, UiRectLayout rectLayout, Color color)
        {
            GameObject imageObject = new GameObject(objectName);
            imageObject.transform.SetParent(parentTransform, false);
            RectTransform rectTransform = imageObject.AddComponent<RectTransform>();
            applyRectTransformLayout(rectTransform, rectLayout);
            Image image = imageObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private void placeImageBehindText(Transform imageTransform, Transform textTransform)
        {
            if (imageTransform == null || textTransform == null)
            {
                return;
            }

            int targetIndex = Mathf.Max(0, textTransform.GetSiblingIndex() - 1);
            imageTransform.SetSiblingIndex(targetIndex);
        }

        private Text createText(string objectName, Transform parentTransform, UiRectLayout rectLayout, UiTextStyle textStyle)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parentTransform, false);
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            applyRectTransformLayout(rectTransform, rectLayout);
            Text text = textObject.AddComponent<Text>();
            Font textFont = mUiFont;
            if (textFont == null)
            {
                textFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            text.font = textFont;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.94f, 0.90f, 0.82f, 1.0f);
            text.fontSize = textStyle.FontSize;
            text.fontStyle = textStyle.FontStyle;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            addTextShadow(textObject);
            return text;
        }

        private void setText(Text text, string value)
        {
            if (text == null)
            {
                return;
            }

            string safeValue = string.IsNullOrEmpty(value) ? string.Empty : value;
            text.font = containsHangul(safeValue) ? mKoreanFallbackFont : mUiFont;
            text.text = safeValue;
        }

        private static bool containsHangul(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            foreach (char character in text)
            {
                if (character >= '\uac00' && character <= '\ud7a3')
                {
                    return true;
                }
            }

            return false;
        }

        private InputField createInputField()
        {
            GameObject inputFieldObject = new GameObject("AnswerInputField");
            inputFieldObject.transform.SetParent(mCanvasRootTransform, false);
            RectTransform rectTransform = inputFieldObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.14f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.14f);
            rectTransform.anchoredPosition = new Vector2(0.0f, 0.0f);
            rectTransform.sizeDelta = new Vector2(920.0f, 96.0f);

            Image backgroundImage = inputFieldObject.AddComponent<Image>();
            backgroundImage.color = Color.white;

            InputField inputField = inputFieldObject.AddComponent<InputField>();
            inputField.transition = Selectable.Transition.None;

            Text placeholderText = createText(
                "Placeholder",
                inputFieldObject.transform,
                UiRectLayout.Stretched(new Vector2(-60.0f, -20.0f)),
                new UiTextStyle(24, FontStyle.Italic));
            placeholderText.alignment = TextAnchor.MiddleLeft;
            placeholderText.color = new Color(0.80f, 0.74f, 0.66f, 0.7f);
            setText(placeholderText, "입력된 답변이 이 영역에 표시됩니다.");

            Text valueText = createText(
                "Text",
                inputFieldObject.transform,
                UiRectLayout.Stretched(new Vector2(-60.0f, -20.0f)),
                new UiTextStyle(24, FontStyle.Normal));
            valueText.alignment = TextAnchor.MiddleLeft;
            valueText.color = new Color(0.94f, 0.90f, 0.82f, 1.0f);
            valueText.supportRichText = false;

            inputField.textComponent = valueText;
            inputField.placeholder = placeholderText;
            inputField.lineType = InputField.LineType.MultiLineNewline;
            inputField.characterLimit = 240;
            inputField.interactable = false;
            return inputField;
        }

        private Button createButton(string objectName, string labelText, UiRectLayout rectLayout, Action clickedAction)
        {
            GameObject buttonObject = new GameObject(objectName);
            buttonObject.transform.SetParent(mCanvasRootTransform, false);
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            applyRectTransformLayout(rectTransform, rectLayout);

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = Color.white;

            Button button = buttonObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(
                () =>
                {
                    playInterfaceCue(mButtonConfirmClip, 0.68f);
                    clickedAction?.Invoke();
                });

            Text label = createText(
                "Label",
                buttonObject.transform,
                UiRectLayout.Stretched(new Vector2(-20.0f, -20.0f)),
                new UiTextStyle(34, FontStyle.Bold));
            setText(label, labelText);
            return button;
        }

        private static void applyRectTransformLayout(RectTransform rectTransform, UiRectLayout rectLayout)
        {
            rectTransform.anchorMin = rectLayout.AnchorMin;
            rectTransform.anchorMax = rectLayout.AnchorMax;
            rectTransform.anchoredPosition = rectLayout.AnchoredPosition;
            rectTransform.sizeDelta = rectLayout.SizeDelta;
        }

        private void addTextShadow(GameObject textObject)
        {
            Shadow shadow = textObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.05f, 0.03f, 0.02f, 0.92f);
            shadow.effectDistance = new Vector2(2.0f, -2.0f);
        }
    }
}
