using System.Collections.Generic;

namespace MouthOfTruth.Game.Presentation.Runtime
{
    public static class MouthOfTruthStreamingAssetManifest
    {
        private static readonly string[] REQUIRED_PROJECT_STREAMING_ASSET_FILES =
        {
            "unity-app/Assets/StreamingAssets/questions/question_pool.json",
            "unity-app/Assets/StreamingAssets/tutorial/combined_sequence.json",
            "unity-app/Assets/StreamingAssets/art/backgrounds/title_background_stone_wall.jpeg",
            "unity-app/Assets/StreamingAssets/art/backgrounds/stage_card_selection_generated.png",
            "unity-app/Assets/StreamingAssets/art/backgrounds/stage_mouth_chamber_generated.png",
            "unity-app/Assets/StreamingAssets/art/cards/question_card_back.png",
            "unity-app/Assets/StreamingAssets/art/cards/question_card_front.png",
            "unity-app/Assets/StreamingAssets/art/effects/card_selection_glow.png",
            "unity-app/Assets/StreamingAssets/art/effects/card_selection_progress_fill.png",
            "unity-app/Assets/StreamingAssets/art/environment/floor_red_carpet_runner.png",
            "unity-app/Assets/StreamingAssets/art/input/hand_pointer_cursor.png",
            "unity-app/Assets/StreamingAssets/art/input/leap_motion_device.png",
            "unity-app/Assets/StreamingAssets/art/input/ritual_hand_insert.png",
            "unity-app/Assets/StreamingAssets/art/mouth/truth_mouth_face.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_end_game.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_exit_icon.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_frame_primary.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_start_game.png",
            "unity-app/Assets/StreamingAssets/art/ui/button_try_again.png",
            "unity-app/Assets/StreamingAssets/art/ui/logo_title_main.png",
            "unity-app/Assets/StreamingAssets/art/ui/panel_question.png",
            "unity-app/Assets/StreamingAssets/art/ui/panel_result.png",
            "unity-app/Assets/StreamingAssets/art/ui/panel_status.png",
            "unity-app/Assets/StreamingAssets/art/ui/title_vignette.png",
            "unity-app/Assets/StreamingAssets/art/verdict/verdict_false.png",
            "unity-app/Assets/StreamingAssets/art/verdict/verdict_true.png",
            "unity-app/Assets/StreamingAssets/art/verdict/verdict_uncertain.png",
            "unity-app/Assets/StreamingAssets/audio/ambience/title_temple_ambience_loop.wav",
            "unity-app/Assets/StreamingAssets/audio/cards/card_hover.wav",
            "unity-app/Assets/StreamingAssets/audio/cards/card_reveal.wav",
            "unity-app/Assets/StreamingAssets/audio/cards/card_select.wav",
            "unity-app/Assets/StreamingAssets/audio/interaction/hand_insert.wav",
            "unity-app/Assets/StreamingAssets/audio/interaction/hand_prompt.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0001.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0002.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0003.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0004.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0005.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0006.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0007.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0008.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0009.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0010.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0011.wav",
            "unity-app/Assets/StreamingAssets/audio/questions/Q0012.wav",
            "unity-app/Assets/StreamingAssets/audio/results/result_false.wav",
            "unity-app/Assets/StreamingAssets/audio/results/result_true.wav",
            "unity-app/Assets/StreamingAssets/audio/results/result_uncertain.wav",
            "unity-app/Assets/StreamingAssets/audio/ui/button_confirm.wav",
        };

        public static IReadOnlyList<string> RequiredProjectStreamingAssetFiles => REQUIRED_PROJECT_STREAMING_ASSET_FILES;
    }
}
