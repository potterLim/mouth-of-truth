using System.Collections.Generic;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public partial class MouthOfTruthGameView
    {
        private async Task loadSpritesAsync()
        {
            mCardBackSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.QuestionCardBackPath);
            if (mCardBackSprite == null)
            {
                mCardBackSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.43f, 0.63f, 0.95f, 1.0f));
            }

            mCardFrontSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.QuestionCardFrontPath);
            if (mCardFrontSprite == null)
            {
                mCardFrontSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.96f, 0.93f, 0.88f, 1.0f));
            }

            mButtonFrameSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.PrimaryButtonFramePath);
            if (mButtonFrameSprite == null)
            {
                mButtonFrameSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.38f, 0.21f, 0.11f, 1.0f));
            }

            mStartButtonSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.StartButtonPath);
            if (mStartButtonSprite == null)
            {
                mStartButtonSprite = mButtonFrameSprite;
            }

            mTryAgainButtonSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.TryAgainButtonPath);
            if (mTryAgainButtonSprite == null)
            {
                mTryAgainButtonSprite = mButtonFrameSprite;
            }

            mEndGameButtonSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.EndGameButtonPath);
            if (mEndGameButtonSprite == null)
            {
                mEndGameButtonSprite = mButtonFrameSprite;
            }

            mExitIconButtonSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.ExitIconButtonPath);
            if (mExitIconButtonSprite == null)
            {
                mExitIconButtonSprite = mButtonFrameSprite;
            }

            mPointerCursorSprite = createPointerCursorSprite();
            mRitualHandSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.RitualHandInsertPath);
            if (mRitualHandSprite == null)
            {
                mRitualHandSprite = mPointerCursorSprite;
            }

            mLeapMotionDeviceSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.LeapMotionDevicePath);
            if (mLeapMotionDeviceSprite == null)
            {
                mLeapMotionDeviceSprite = RuntimeSpriteLoader.CreateSolidSprite(Color.clear);
            }

            mVerdictTrueSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.TrueVerdictPath);
            if (mVerdictTrueSprite == null)
            {
                mVerdictTrueSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.45f, 0.80f, 0.54f, 1.0f));
            }

            mVerdictFalseSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.FalseVerdictPath);
            if (mVerdictFalseSprite == null)
            {
                mVerdictFalseSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.84f, 0.38f, 0.43f, 1.0f));
            }

            mVerdictUncertainSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.UncertainVerdictPath);
            if (mVerdictUncertainSprite == null)
            {
                mVerdictUncertainSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.80f, 0.69f, 0.36f, 1.0f));
            }

            mTitleVignetteSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.TitleVignettePath);
            if (mTitleVignetteSprite == null)
            {
                mTitleVignetteSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.0f, 0.0f, 0.0f, 0.30f));
            }

            mQuestionPanelSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.QuestionPanelFramePath);
            if (mQuestionPanelSprite == null)
            {
                mQuestionPanelSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.15f, 0.10f, 0.07f, 0.90f));
            }

            mStatusPanelSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.StatusPanelFramePath);
            if (mStatusPanelSprite == null)
            {
                mStatusPanelSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.08f, 0.05f, 0.03f, 0.76f));
            }

            mResultPanelSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.ResultPanelFramePath);
            if (mResultPanelSprite == null)
            {
                mResultPanelSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.17f, 0.10f, 0.08f, 0.90f));
            }

            mCardGlowSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.CardSelectionGlowPath);
            if (mCardGlowSprite == null)
            {
                mCardGlowSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.90f, 0.72f, 0.25f, 0.35f));
            }

            mDwellFillSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.CardSelectionProgressFillPath);
            if (mDwellFillSprite == null)
            {
                mDwellFillSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.95f, 0.82f, 0.33f, 0.95f));
            }

            mTitleBackgroundSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.TitleBackgroundPath);
            if (mTitleBackgroundSprite == null)
            {
                mTitleBackgroundSprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.12f, 0.09f, 0.07f, 1.0f));
            }

            mCardSelectionBackgroundSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.CardSelectionBackgroundPath);
            if (mCardSelectionBackgroundSprite == null)
            {
                mCardSelectionBackgroundSprite = mTitleBackgroundSprite;
            }

            mMouthChamberBackgroundSprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.MouthChamberBackgroundPath);
            if (mMouthChamberBackgroundSprite == null)
            {
                mMouthChamberBackgroundSprite = mCardSelectionBackgroundSprite == null
                    ? mTitleBackgroundSprite
                    : mCardSelectionBackgroundSprite;
            }

            mCarpetImage.sprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.FloorRunnerPath);
            if (mCarpetImage.sprite == null)
            {
                mCarpetImage.sprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.44f, 0.03f, 0.05f, 1.0f));
            }

            mLogoImage.sprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.TitleLogoPath);
            if (mLogoImage.sprite == null)
            {
                mLogoImage.sprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.82f, 0.71f, 0.52f, 1.0f));
            }

            mMouthImage.sprite = await RuntimeSpriteLoader.LoadSpriteOrNullAsync(MouthOfTruthAssetCatalog.TruthMouthFacePath);
            if (mMouthImage.sprite == null)
            {
                mMouthImage.sprite = RuntimeSpriteLoader.CreateSolidSprite(new Color(0.85f, 0.83f, 0.78f, 1.0f));
            }

            mBackgroundImage.sprite = mTitleBackgroundSprite;
        }

        private void loadUiFonts()
        {
            mUiFont = Resources.Load<Font>(MouthOfTruthAssetCatalog.UiFontResourceName);
            if (mUiFont == null)
            {
                mUiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            mKoreanFallbackFont = Resources.Load<Font>(MouthOfTruthAssetCatalog.KoreanFallbackFontResourceName);
            if (mKoreanFallbackFont == null)
            {
                mKoreanFallbackFont = mUiFont;
            }
        }

        private void applyTheme()
        {
            mBackgroundImage.type = Image.Type.Sliced;
            mBackgroundImage.preserveAspect = true;
            mSceneOverlayImage.color = new Color(SCENE_OVERLAY_COLOR.r, SCENE_OVERLAY_COLOR.g, SCENE_OVERLAY_COLOR.b, 0.0f);
            mSceneOverlayImage.raycastTarget = false;
            mCarpetImage.preserveAspect = true;
            mCarpetImage.raycastTarget = false;
            mTitleVignetteImage.sprite = mTitleVignetteSprite;
            mTitleVignetteImage.type = Image.Type.Sliced;
            mTitleVignetteImage.raycastTarget = false;
            mLogoImage.preserveAspect = true;
            mLogoImage.raycastTarget = false;
            mMouthImage.preserveAspect = true;
            mMouthImage.raycastTarget = false;
            mHandImage.sprite = mPointerCursorSprite;
            mHandImage.preserveAspect = true;
            mHandImage.raycastTarget = false;
            mRitualHandImage.sprite = mRitualHandSprite;
            mRitualHandImage.preserveAspect = true;
            mRitualHandImage.raycastTarget = false;
            mPointerImage.sprite = mPointerCursorSprite;
            mPointerImage.preserveAspect = true;
            mPointerImage.raycastTarget = false;
            mTutorialHandImage.sprite = mRitualHandSprite;
            mTutorialHandImage.preserveAspect = true;
            mTutorialHandImage.raycastTarget = false;
            mTutorialLeapMotionDeviceImage.sprite = mLeapMotionDeviceSprite;
            mTutorialLeapMotionDeviceImage.preserveAspect = true;
            mTutorialLeapMotionDeviceImage.raycastTarget = false;
            mTutorialOverlayImage.raycastTarget = false;
            mTutorialDevicePanelImage.type = Image.Type.Sliced;
            mTutorialDevicePanelImage.raycastTarget = false;
            mVerdictImage.preserveAspect = true;
            mVerdictImage.raycastTarget = false;
            mQuestionPanelImage.sprite = mQuestionPanelSprite;
            mQuestionPanelImage.type = Image.Type.Sliced;
            mQuestionPanelImage.raycastTarget = false;
            mStatusPanelImage.sprite = mStatusPanelSprite;
            mStatusPanelImage.type = Image.Type.Sliced;
            mStatusPanelImage.raycastTarget = false;
            mResultPanelImage.sprite = mResultPanelSprite;
            mResultPanelImage.type = Image.Type.Sliced;
            mResultPanelImage.raycastTarget = false;
            if (mAnswerInputField?.image != null)
            {
                mAnswerInputField.image.sprite = mStatusPanelSprite;
                mAnswerInputField.image.type = Image.Type.Sliced;
                mAnswerInputField.image.color = new Color(1.0f, 1.0f, 1.0f, 0.96f);
            }

            mStartButton.image.sprite = mStartButtonSprite;
            mTryAgainButton.image.sprite = mTryAgainButtonSprite;
            mBackToTitleButton.image.sprite = mButtonFrameSprite;
            mExitButton.image.sprite = mExitIconButtonSprite;
            mStartButton.image.type = Image.Type.Simple;
            mTryAgainButton.image.type = Image.Type.Simple;
            mBackToTitleButton.image.type = Image.Type.Sliced;
            mExitButton.image.type = Image.Type.Simple;
            mStartButton.image.preserveAspect = true;
            mTryAgainButton.image.preserveAspect = true;
            mExitButton.image.preserveAspect = true;
            setButtonLabelVisible(mStartButton, false);
            setButtonLabelVisible(mTryAgainButton, false);
            setButtonLabelVisible(mExitButton, false);

            foreach (KeyValuePair<EQuestionCardSlot, QuestionCardView> pair in mCardViews)
            {
                pair.Value.SetBack(mCardBackSprite);
                pair.Value.SetDecorSprites(mCardGlowSprite, mDwellFillSprite);
            }
        }
    }
}
