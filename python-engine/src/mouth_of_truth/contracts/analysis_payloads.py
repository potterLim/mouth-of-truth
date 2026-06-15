"""Typed payloads exchanged between modality analyzers and judgment fusion."""

from __future__ import annotations

from typing import TypedDict

from mouth_of_truth.contracts.verdict_kind import VerdictKind


class ModelPredictionPayload(TypedDict):
    """Normalized model output used by scoring pipelines."""

    label: str
    confidence: float
    class_probabilities: list[float]
    probability_by_label: dict[str, float]


class VoiceFilePredictionPayload(ModelPredictionPayload):
    """Voice model output for one complete audio file."""

    audio_path: str


class FaceRecognitionPayload(TypedDict):
    """One face-recognition observation used by face scoring."""

    label: str
    conf: float
    change_score: float
    base_score: float
    suspicion_score: float


class VoiceSegmentPayload(TypedDict):
    """One voice segment observation used by voice scoring."""

    segment_index: int
    label: str
    confidence: float
    change_score: float
    base_score: float
    suspicion_score: float
    status_text: str
    probability_by_label: dict[str, float]
    class_probabilities: list[float]


class AnalysisSummary(TypedDict):
    """Stable summary fields consumed by multimodal judgment."""

    avg_score: float
    avg_base: float
    avg_change: float
    dominant_label: str
    status_text: str
    result_text: str


class FaceAnalysisPayload(TypedDict):
    """Face-analysis payload consumed by the bridge runner."""

    frame_count: int
    recognition_count: int
    summary: AnalysisSummary


class VoiceAnalysisPayload(TypedDict):
    """Voice-analysis payload consumed by the bridge runner."""

    audio_path: str
    segment_count: int
    segments: list[VoiceSegmentPayload]
    summary: AnalysisSummary


class FusedVerdictPayload(TypedDict):
    """Fusion payload consumed by the judgment policy."""

    face_score: float
    voice_score: float
    final_score: float
    verdict: VerdictKind
    reason_codes: list[str]
