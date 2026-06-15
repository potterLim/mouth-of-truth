"""Face-emotion model loading and inference helpers."""

from __future__ import annotations

from typing import Any

from ultralytics import YOLO

from mouth_of_truth.contracts.analysis_payloads import ModelPredictionPayload
from mouth_of_truth.runtime.model_paths import resolve_face_model_path


_CACHED_FACE_MODEL: YOLO | None = None


def load_face_model() -> YOLO:
    """Loads one trained face-emotion model."""
    global _CACHED_FACE_MODEL

    if _CACHED_FACE_MODEL is not None:
        return _CACHED_FACE_MODEL

    face_model_path = resolve_face_model_path()
    _CACHED_FACE_MODEL = YOLO(str(face_model_path))
    return _CACHED_FACE_MODEL


def build_probability_by_label(model_names: dict[int, str], class_probabilities: list[float]) -> dict[str, float]:
    """Converts one probability list into one label-to-score dictionary."""
    probability_by_label: dict[str, float] = {}

    for class_index, class_name in model_names.items():
        probability_by_label[class_name] = float(class_probabilities[class_index])

    return probability_by_label


def predict_face_crop(model: YOLO, face_crop: Any) -> ModelPredictionPayload:
    """Runs face-emotion prediction on one cropped face image."""
    prediction_results = model.predict(face_crop, verbose=False)

    if not prediction_results:
        raise RuntimeError("No prediction result returned from the face model.")

    prediction_result = prediction_results[0]
    probabilities = prediction_result.probs

    if probabilities is None:
        raise RuntimeError("The face model did not return classification probabilities.")

    top_index = int(probabilities.top1)
    top_confidence = float(probabilities.top1conf)
    top_label = model.names[top_index]
    class_probabilities = probabilities.data.tolist()

    return {
        "label": top_label,
        "confidence": top_confidence,
        "class_probabilities": class_probabilities,
        "probability_by_label": build_probability_by_label(model.names, class_probabilities),
    }
