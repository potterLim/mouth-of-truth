using System.IO;
using UnityEngine;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public static class MouthOfTruthAssetCatalog
    {
        private const string ART_DIRECTORY_NAME = "art";
        private const string AUDIO_DIRECTORY_NAME = "audio";

        private static string getStreamingArtPath(string relativePath)
        {
            return Path.Combine(Application.streamingAssetsPath, ART_DIRECTORY_NAME, relativePath);
        }

        private static string getStreamingAudioPath(string relativePath)
        {
            return Path.Combine(Application.streamingAssetsPath, AUDIO_DIRECTORY_NAME, relativePath);
        }

        public static string TitleBackgroundPath =>
            getStreamingArtPath("backgrounds/title_background_stone_wall.jpeg");

        public static string CardSelectionBackgroundPath =>
            getStreamingArtPath("backgrounds/stage_card_selection_generated.png");

        public static string MouthChamberBackgroundPath =>
            getStreamingArtPath("backgrounds/stage_mouth_chamber_generated.png");

        public static string TitleLogoPath =>
            getStreamingArtPath("ui/logo_title_main.png");

        public static string TitleVignettePath =>
            getStreamingArtPath("ui/title_vignette.png");

        public static string QuestionPanelFramePath =>
            getStreamingArtPath("ui/panel_question.png");

        public static string StatusPanelFramePath =>
            getStreamingArtPath("ui/panel_status.png");

        public static string ResultPanelFramePath =>
            getStreamingArtPath("ui/panel_result.png");

        public static string StartButtonPath =>
            getStreamingArtPath("ui/button_start_game.png");

        public static string TryAgainButtonPath =>
            getStreamingArtPath("ui/button_try_again.png");

        public static string EndGameButtonPath =>
            getStreamingArtPath("ui/button_end_game.png");

        public static string ExitIconButtonPath =>
            getStreamingArtPath("ui/button_exit_icon.png");

        public static string UiFontResourceName => "Fonts/GowunDodum-Regular";

        public static string KoreanFallbackFontResourceName => "Fonts/GowunDodum-Regular";

        public static string FloorRunnerPath =>
            getStreamingArtPath("environment/floor_red_carpet_runner.png");

        public static string FirstRunTutorialSequencePath =>
            Path.Combine(Application.streamingAssetsPath, "tutorial", "combined_sequence.json");

        public static string QuestionCardBackPath =>
            getStreamingArtPath("cards/question_card_back.png");

        public static string QuestionCardFrontPath =>
            getStreamingArtPath("cards/question_card_front.png");

        public static string TruthMouthFacePath =>
            getStreamingArtPath("mouth/truth_mouth_face.png");

        public static string TrueVerdictPath =>
            getStreamingArtPath("verdict/verdict_true.png");

        public static string FalseVerdictPath =>
            getStreamingArtPath("verdict/verdict_false.png");

        public static string UncertainVerdictPath =>
            getStreamingArtPath("verdict/verdict_uncertain.png");

        public static string PrimaryButtonFramePath =>
            getStreamingArtPath("ui/button_frame_primary.png");

        public static string HandPointerPath =>
            getStreamingArtPath("input/hand_pointer_cursor.png");

        public static string RitualHandInsertPath =>
            getStreamingArtPath("input/ritual_hand_insert.png");

        public static string LeapMotionDevicePath =>
            getStreamingArtPath("input/leap_motion_device.png");

        public static string CardSelectionGlowPath =>
            getStreamingArtPath("effects/card_selection_glow.png");

        public static string CardSelectionProgressFillPath =>
            getStreamingArtPath("effects/card_selection_progress_fill.png");

        public static string TitleAmbiencePath =>
            getStreamingAudioPath("ambience/title_temple_ambience_loop.wav");

        public static string ButtonConfirmPath =>
            getStreamingAudioPath("ui/button_confirm.wav");

        public static string CardHoverPath =>
            getStreamingAudioPath("cards/card_hover.wav");

        public static string CardSelectPath =>
            getStreamingAudioPath("cards/card_select.wav");

        public static string CardRevealPath =>
            getStreamingAudioPath("cards/card_reveal.wav");

        public static string HandInsertPath =>
            getStreamingAudioPath("interaction/hand_insert.wav");

        public static string HandPromptPath =>
            getStreamingAudioPath("interaction/hand_prompt.wav");

        public static string ResultTruePath =>
            getStreamingAudioPath("results/result_true.wav");

        public static string ResultFalsePath =>
            getStreamingAudioPath("results/result_false.wav");

        public static string ResultUncertainPath =>
            getStreamingAudioPath("results/result_uncertain.wav");
    }
}
