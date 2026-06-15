using System.IO;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    internal static class MouthOfTruthAssetCatalog
    {
        private const string ART_DIRECTORY_NAME = "art";
        private const string AUDIO_DIRECTORY_NAME = "audio";

        private static RuntimeAssetFilePath getStreamingArtPath(string relativePath)
        {
            return new RuntimeAssetFilePath(Path.Combine(Application.streamingAssetsPath, ART_DIRECTORY_NAME, relativePath));
        }

        private static RuntimeAssetFilePath getStreamingAudioPath(string relativePath)
        {
            return new RuntimeAssetFilePath(Path.Combine(Application.streamingAssetsPath, AUDIO_DIRECTORY_NAME, relativePath));
        }

        public static RuntimeAssetFilePath TitleBackgroundPath =>
            getStreamingArtPath("backgrounds/title_background_stone_wall.jpeg");

        public static RuntimeAssetFilePath CardSelectionBackgroundPath =>
            getStreamingArtPath("backgrounds/stage_card_selection_generated.png");

        public static RuntimeAssetFilePath MouthChamberBackgroundPath =>
            getStreamingArtPath("backgrounds/stage_mouth_chamber_generated.png");

        public static RuntimeAssetFilePath TitleLogoPath =>
            getStreamingArtPath("ui/logo_title_main.png");

        public static RuntimeAssetFilePath TitleVignettePath =>
            getStreamingArtPath("ui/title_vignette.png");

        public static RuntimeAssetFilePath QuestionPanelFramePath =>
            getStreamingArtPath("ui/panel_question.png");

        public static RuntimeAssetFilePath StatusPanelFramePath =>
            getStreamingArtPath("ui/panel_status.png");

        public static RuntimeAssetFilePath ResultPanelFramePath =>
            getStreamingArtPath("ui/panel_result.png");

        public static RuntimeAssetFilePath StartButtonPath =>
            getStreamingArtPath("ui/button_start_game.png");

        public static RuntimeAssetFilePath TryAgainButtonPath =>
            getStreamingArtPath("ui/button_try_again.png");

        public static RuntimeAssetFilePath EndGameButtonPath =>
            getStreamingArtPath("ui/button_end_game.png");

        public static RuntimeAssetFilePath ExitIconButtonPath =>
            getStreamingArtPath("ui/button_exit_icon.png");

        public static string UiFontResourceName => "Fonts/GowunDodum-Regular";

        public static string KoreanFallbackFontResourceName => "Fonts/GowunDodum-Regular";

        public static RuntimeAssetFilePath FloorRunnerPath =>
            getStreamingArtPath("environment/floor_red_carpet_runner.png");

        public static RuntimeAssetFilePath FirstRunTutorialSequencePath =>
            new RuntimeAssetFilePath(Path.Combine(Application.streamingAssetsPath, "tutorial", "combined_sequence.json"));

        public static RuntimeAssetFilePath QuestionCardBackPath =>
            getStreamingArtPath("cards/question_card_back.png");

        public static RuntimeAssetFilePath QuestionCardFrontPath =>
            getStreamingArtPath("cards/question_card_front.png");

        public static RuntimeAssetFilePath TruthMouthFacePath =>
            getStreamingArtPath("mouth/truth_mouth_face.png");

        public static RuntimeAssetFilePath TrueVerdictPath =>
            getStreamingArtPath("verdict/verdict_true.png");

        public static RuntimeAssetFilePath FalseVerdictPath =>
            getStreamingArtPath("verdict/verdict_false.png");

        public static RuntimeAssetFilePath UncertainVerdictPath =>
            getStreamingArtPath("verdict/verdict_uncertain.png");

        public static RuntimeAssetFilePath PrimaryButtonFramePath =>
            getStreamingArtPath("ui/button_frame_primary.png");

        public static RuntimeAssetFilePath HandPointerPath =>
            getStreamingArtPath("input/hand_pointer_cursor.png");

        public static RuntimeAssetFilePath RitualHandInsertPath =>
            getStreamingArtPath("input/ritual_hand_insert.png");

        public static RuntimeAssetFilePath LeapMotionDevicePath =>
            getStreamingArtPath("input/leap_motion_device.png");

        public static RuntimeAssetFilePath CardSelectionGlowPath =>
            getStreamingArtPath("effects/card_selection_glow.png");

        public static RuntimeAssetFilePath CardSelectionProgressFillPath =>
            getStreamingArtPath("effects/card_selection_progress_fill.png");

        public static RuntimeAssetFilePath TitleAmbiencePath =>
            getStreamingAudioPath("ambience/title_temple_ambience_loop.wav");

        public static RuntimeAssetFilePath ButtonConfirmPath =>
            getStreamingAudioPath("ui/button_confirm.wav");

        public static RuntimeAssetFilePath CardHoverPath =>
            getStreamingAudioPath("cards/card_hover.wav");

        public static RuntimeAssetFilePath CardSelectPath =>
            getStreamingAudioPath("cards/card_select.wav");

        public static RuntimeAssetFilePath CardRevealPath =>
            getStreamingAudioPath("cards/card_reveal.wav");

        public static RuntimeAssetFilePath HandInsertPath =>
            getStreamingAudioPath("interaction/hand_insert.wav");

        public static RuntimeAssetFilePath HandPromptPath =>
            getStreamingAudioPath("interaction/hand_prompt.wav");

        public static RuntimeAssetFilePath ResultTruePath =>
            getStreamingAudioPath("results/result_true.wav");

        public static RuntimeAssetFilePath ResultFalsePath =>
            getStreamingAudioPath("results/result_false.wav");

        public static RuntimeAssetFilePath ResultUncertainPath =>
            getStreamingAudioPath("results/result_uncertain.wav");
    }
}
