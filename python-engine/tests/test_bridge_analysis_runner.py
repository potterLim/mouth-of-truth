from __future__ import annotations

import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

from mouth_of_truth.contracts.analysis_payloads import FaceAnalysisPayload, VoiceAnalysisPayload
from mouth_of_truth.runners import bridge_analysis_runner


class BridgeAnalysisRunnerTest(unittest.TestCase):
    def test_run_once_writes_fused_result_from_analysis_payloads(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            request_file_path = Path(temporary_directory) / "bridge" / "analysis_request.json"
            result_file_path = Path(temporary_directory) / "bridge" / "analysis_result.json"
            _write_request(
                request_file_path,
                request_id="request-false",
                answer_transcript="대답",
                answer_audio_file_path="captures/answer.wav",
                face_frames_directory_path="captures/face",
                face_frame_count=10,
                voice_segment_count=2,
            )
            face_analysis: FaceAnalysisPayload = {
                "frame_count": 10,
                "recognition_count": 1,
                "summary": {"avg_score": 45.0, "dominant_label": "fear"},
            }
            voice_analysis: VoiceAnalysisPayload = {
                "audio_path": "captures/answer.wav",
                "segment_count": 1,
                "segments": [],
                "summary": {"avg_score": 20.0, "dominant_label": "fru"},
            }

            with patch.object(
                bridge_analysis_runner,
                "_analyze_modalities",
                return_value=(face_analysis, voice_analysis),
            ):
                bridge_analysis_runner.run_once(request_file_path, result_file_path)

            result_payload = _read_result_payload(result_file_path)
            self.assertEqual(result_payload["RequestID"], "request-false")
            self.assertEqual(result_payload["Verdict"], "FALSE")
            self.assertEqual(result_payload["AnswerTranscript"], "대답")
            self.assertEqual(result_payload["ReasonCodes"], [])

    def test_run_once_uses_capture_counts_when_no_capture_files_are_present(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            request_file_path = Path(temporary_directory) / "analysis_request.json"
            result_file_path = Path(temporary_directory) / "analysis_result.json"
            _write_request(
                request_file_path,
                request_id="request-count-fallback",
                answer_transcript="대답",
                answer_audio_file_path="",
                face_frames_directory_path="",
                face_frame_count=1,
                voice_segment_count=1,
            )
            face_analysis: FaceAnalysisPayload = {
                "frame_count": 0,
                "recognition_count": 0,
                "summary": {"avg_score": 20.0, "dominant_label": "neutral"},
            }
            voice_analysis: VoiceAnalysisPayload = {
                "audio_path": "",
                "segment_count": 0,
                "segments": [],
                "summary": {"avg_score": 10.0, "dominant_label": "neu"},
            }

            with patch.object(
                bridge_analysis_runner,
                "_analyze_modalities",
                return_value=(face_analysis, voice_analysis),
            ):
                bridge_analysis_runner.run_once(request_file_path, result_file_path)

            result_payload = _read_result_payload(result_file_path)
            self.assertEqual(result_payload["RequestID"], "request-count-fallback")
            self.assertEqual(result_payload["Verdict"], "TRUE")
            self.assertEqual(result_payload["ReasonCodes"], [])

    def test_run_once_uses_analyzer_counts_when_capture_files_are_present(self) -> None:
        with tempfile.TemporaryDirectory() as temporary_directory:
            request_file_path = Path(temporary_directory) / "bridge" / "analysis_request.json"
            result_file_path = Path(temporary_directory) / "bridge" / "analysis_result.json"
            _write_request(
                request_file_path,
                request_id="request-missing-evidence",
                answer_transcript="대답",
                answer_audio_file_path="captures/answer.wav",
                face_frames_directory_path="captures/face",
                face_frame_count=10,
                voice_segment_count=2,
            )
            face_analysis: FaceAnalysisPayload = {
                "frame_count": 10,
                "recognition_count": 0,
                "summary": {"avg_score": 80.0, "dominant_label": "anger"},
            }
            voice_analysis: VoiceAnalysisPayload = {
                "audio_path": "captures/answer.wav",
                "segment_count": 0,
                "segments": [],
                "summary": {"avg_score": 80.0, "dominant_label": "ang"},
            }

            with patch.object(
                bridge_analysis_runner,
                "_analyze_modalities",
                return_value=(face_analysis, voice_analysis),
            ):
                bridge_analysis_runner.run_once(request_file_path, result_file_path)

            result_payload = _read_result_payload(result_file_path)
            self.assertEqual(result_payload["RequestID"], "request-missing-evidence")
            self.assertEqual(result_payload["Verdict"], "UNCERTAIN")
            self.assertEqual(
                result_payload["ReasonCodes"],
                ["insufficient_face_data", "insufficient_voice_data"],
            )


def _write_request(
    request_file_path: Path,
    *,
    request_id: str,
    answer_transcript: str,
    answer_audio_file_path: str,
    face_frames_directory_path: str,
    face_frame_count: int,
    voice_segment_count: int,
) -> None:
    request_file_path.parent.mkdir(parents=True, exist_ok=True)
    request_file_path.write_text(
        json.dumps(
            {
                "RequestID": request_id,
                "QuestionID": "question-1",
                "QuestionText": "질문",
                "AnswerTranscript": answer_transcript,
                "AnswerAudioFilePath": answer_audio_file_path,
                "FaceFramesDirectoryPath": face_frames_directory_path,
                "FaceFrameCount": face_frame_count,
                "VoiceSegmentCount": voice_segment_count,
                "RequestedAtUtc": "2026-06-03T00:00:00Z",
            }
        ),
        encoding="utf-8",
    )


def _read_result_payload(result_file_path: Path) -> dict[str, object]:
    return json.loads(result_file_path.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()
