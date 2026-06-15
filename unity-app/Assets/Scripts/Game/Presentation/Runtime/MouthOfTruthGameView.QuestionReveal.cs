using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        public Task PlayQuestionRevealAsync(EQuestionCardSlot selectedQuestionCardSlot, QuestionDefinition questionDefinition)
        {
            return PlayQuestionRevealAsync(selectedQuestionCardSlot, questionDefinition, null);
        }

        public async Task PlayQuestionRevealAsync(EQuestionCardSlot selectedQuestionCardSlot, QuestionDefinition questionDefinition, Func<Task> questionNarrationTaskFactoryOrNull)
        {
            resetHandPromptPanelAlpha();
            mIsCardAbsorptionPresentationActive = false;
            mCardAbsorptionPresentationProgress = NormalizedProgress.Zero;
            setObjectVisibility(mPromptText, EUiElementVisibility.Hidden);
            setObjectVisibility(mStatusText, EUiElementVisibility.Hidden);
            setObjectVisibility(mQuestionPanelImage, EUiElementVisibility.Hidden);
            setObjectVisibility(mQuestionText, EUiElementVisibility.Hidden);
            setObjectVisibility(mSceneOverlayImage, EUiElementVisibility.Visible);
            setMouthEffectVisualState(EMouthEffectVisualState.Hidden);
            setOverlayAlpha(0.12f);
            mLastAudibleHoveredCardSlotOrNull = null;
            mLastCardHoverCueTimeSeconds = Time.unscaledTime;
            playInterfaceCueClean(mCardSelectClip, 0.58f);
            await animateOverTimeAsync(new SecondsDuration(CARD_SELECTION_CUE_SETTLE_SECONDS), ignoreAnimationProgress);

            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                bool isSelected = pair.Key == selectedQuestionCardSlot;
                EQuestionCardVisualState questionCardVisualState = isSelected
                    ? EQuestionCardVisualState.Selected
                    : EQuestionCardVisualState.Dimmed;
                pair.Value.SetVisualState(questionCardVisualState, NormalizedProgress.Zero);
                pair.Value.gameObject.SetActive(isSelected);
            }

            QuestionCardView selectedCardView = mCardViews[selectedQuestionCardSlot];
            Vector2 startPosition = selectedCardView.RectTransform.anchoredPosition;
            Vector2 endPosition = getCenteredCardRevealPosition();

            await animateOverTimeAsync(
                new SecondsDuration(0.75f),
                progress =>
                {
                    float easedProgress = easeOut(progress);
                    selectedCardView.RectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, easedProgress);
                    selectedCardView.RectTransform.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.22f, easedProgress);
                });

            await animateOverTimeAsync(
                new SecondsDuration(CARD_FLIP_CLOSE_SECONDS),
                progress =>
                {
                    float easedProgress = easeInOut(progress);
                    float verticalScale = 1.22f + (Mathf.Sin(progress.Value * Mathf.PI) * 0.02f);
                    selectedCardView.SetScale(new QuestionCardScale(Mathf.Lerp(1.22f, 0.08f, easedProgress), verticalScale));
                });

            selectedCardView.SetFront(mCardFrontSprite, questionDefinition.Text);
            playInterfaceCueClean(mCardRevealClip, 0.58f);

            await animateOverTimeAsync(
                new SecondsDuration(CARD_FLIP_OPEN_SECONDS + CARD_REVEAL_CUE_SETTLE_SECONDS),
                progress =>
                {
                    float easedProgress = easeOut(progress);
                    float settlePulse = Mathf.Sin(progress.Value * Mathf.PI) * 0.012f;
                    selectedCardView.SetScale(new QuestionCardScale(Mathf.Lerp(0.08f, 1.26f, easedProgress), Mathf.Lerp(1.24f, 1.26f, easedProgress) + settlePulse));
                });

            await animateOverTimeAsync(
                new SecondsDuration(CARD_FRONT_FOCUS_BEFORE_NARRATION_SECONDS),
                progress =>
                {
                    float pulse = Mathf.Sin(progress.Value * Mathf.PI) * 0.006f;
                    selectedCardView.SetScale(QuestionCardScale.Uniform(1.26f + pulse));
                });

            Task questionNarrationTask = Task.CompletedTask;
            if (questionNarrationTaskFactoryOrNull != null)
            {
                questionNarrationTask = questionNarrationTaskFactoryOrNull.Invoke();
            }

            SecondsDuration cardFrontReadHoldDuration = getCardFrontReadHoldDuration(questionDefinition.Text);
            float elapsedFrontReadHoldSeconds = 0.0f;

            while (elapsedFrontReadHoldSeconds < cardFrontReadHoldDuration.Value
                || questionNarrationTask.IsCompleted == false)
            {
                elapsedFrontReadHoldSeconds += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsedFrontReadHoldSeconds / cardFrontReadHoldDuration.Value);
                float pulse = Mathf.Sin(progress * Mathf.PI) * 0.012f;
                selectedCardView.SetScale(QuestionCardScale.Uniform(1.26f + pulse));
                await Task.Yield();
            }

            await questionNarrationTask;
            await animateOverTimeAsync(
                new SecondsDuration(CARD_FRONT_AFTER_NARRATION_HOLD_SECONDS),
                progress =>
                {
                    float pulse = Mathf.Sin(progress.Value * Mathf.PI) * 0.008f;
                    selectedCardView.SetScale(QuestionCardScale.Uniform(1.26f + pulse));
                });

            bool isTempleApproachSceneVisible = mTempleApproachCameraObject != null;
            ECardLaunchPresentationMode cardLaunchPresentationMode = isTempleApproachSceneVisible
                ? ECardLaunchPresentationMode.TempleApproach
                : ECardLaunchPresentationMode.MouthChamber;
            prepareCardLaunchPresentation(cardLaunchPresentationMode);
            Vector2 launchStartPosition = selectedCardView.RectTransform.anchoredPosition;
            TempleCameraScale targetCameraScale = new TempleCameraScale(TEMPLE_APPROACH_END_SCALE);
            TempleCameraTransition templeCameraTransition = default;
            if (isTempleApproachSceneVisible)
            {
                templeCameraTransition = captureTempleCameraTransition(targetCameraScale, TEMPLE_MOUTH_FOCUS_CENTER);
                setTempleApproachMouthAlpha(CARD_SELECTION_DIM_MOUTH_ALPHA);
            }
            else
            {
                setTempleApproachMouthAlpha(1.0f);
            }

            mIsCardAbsorptionPresentationActive = true;
            mCardAbsorptionPresentationProgress = NormalizedProgress.Zero;

            await animateOverTimeAsync(
                new SecondsDuration(CARD_TO_MOUTH_ABSORPTION_SECONDS),
                progress =>
                {
                    float progressValue = progress.Value;
                    mCardAbsorptionPresentationProgress = progress;
                    NormalizedProgress cameraProgress = NormalizedProgress.FromUnclamped(easeInOut(progress));
                    float suctionProgress = easeIn(Mathf.Clamp01(progressValue * 1.04f));
                    float absorptionProgress = easeIn(Mathf.Clamp01((progressValue - 0.48f) / 0.52f));
                    Vector2 launchTargetPosition;

                    if (isTempleApproachSceneVisible)
                    {
                        float cameraScale = templeCameraTransition.GetScale(targetCameraScale, cameraProgress);
                        Vector2 cameraPosition = templeCameraTransition.GetPosition(cameraProgress);
                        float inhaleBob = Mathf.Sin(progressValue * Mathf.PI * 2.0f) * (1.0f - cameraProgress.Value) * 1.2f;
                        setTempleCameraPose(cameraScale, cameraPosition.y + inhaleBob, cameraPosition.x);
                        float mouthRevealProgress = easeOut(Mathf.Clamp01(progressValue / 0.42f));
                        setTempleApproachMouthAlpha(Mathf.Lerp(CARD_SELECTION_DIM_MOUTH_ALPHA, 1.0f, mouthRevealProgress));
                        launchTargetPosition = getTempleApproachMouthCanvasPosition() + new Vector2(0.0f, -20.0f);
                    }
                    else
                    {
                        launchTargetPosition = getMouthAnchorPosition() + new Vector2(0.0f, -24.0f);
                        mMouthImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.94f, 1.04f, Mathf.Sin(progressValue * Mathf.PI));
                    }

                    Vector2 basePosition = Vector2.Lerp(launchStartPosition, launchTargetPosition, suctionProgress);
                    float remainingAbsorption = 1.0f - absorptionProgress;
                    Vector2 inhaleOffset = new Vector2(
                        Mathf.Sin(progressValue * Mathf.PI * 3.0f) * remainingAbsorption * 16.0f,
                        Mathf.Sin(progressValue * Mathf.PI) * 34.0f * remainingAbsorption);
                    selectedCardView.RectTransform.anchoredPosition = basePosition + inhaleOffset;
                    NormalizedProgress selectedCardAlpha = NormalizedProgress.FromUnclamped(Mathf.Lerp(1.0f, 0.0f, absorptionProgress));
                    selectedCardView.SetScale(QuestionCardScale.Uniform(Mathf.Lerp(1.26f, 0.18f, absorptionProgress)));
                    selectedCardView.SetAlpha(selectedCardAlpha);
                });

            mIsCardAbsorptionPresentationActive = false;
            mCardAbsorptionPresentationProgress = NormalizedProgress.Complete;
            setCardsVisibility(EUiElementVisibility.Hidden);
            if (isTempleApproachSceneVisible)
            {
                setTempleCameraPoseCenteredOnMouth(TEMPLE_APPROACH_END_SCALE, TEMPLE_MOUTH_FOCUS_CENTER);
                setTempleApproachMouthAlpha(1.0f);
            }

            selectedCardView.SetAlpha(NormalizedProgress.Complete);
            selectedCardView.ResetTransformState();
            await animateOverTimeAsync(new SecondsDuration(HAND_PROMPT_AFTER_CARD_LAUNCH_DELAY_SECONDS), ignoreAnimationProgress);
        }

        private static SecondsDuration getCardFrontReadHoldDuration(QuestionText questionText)
        {
            int questionLength = string.IsNullOrWhiteSpace(questionText.Value) ? 0 : questionText.Value.Trim().Length;
            float weightedDuration = questionLength * CARD_FRONT_READ_HOLD_PER_CHARACTER_SECONDS;
            float clampedDurationSeconds = Mathf.Clamp(
                CARD_FRONT_READ_HOLD_MINIMUM_SECONDS + weightedDuration,
                CARD_FRONT_READ_HOLD_MINIMUM_SECONDS,
                CARD_FRONT_READ_HOLD_MAXIMUM_SECONDS);
            return new SecondsDuration(clampedDurationSeconds);
        }

        private static void ignoreAnimationProgress(NormalizedProgress progress)
        {
            _ = progress;
        }
    }
}
