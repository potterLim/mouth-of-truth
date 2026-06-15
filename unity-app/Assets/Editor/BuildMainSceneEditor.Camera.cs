using UnityEngine;
using UnityEngine.SceneManagement;

namespace MouthOfTruth.Editor
{
    public static partial class BuildMainSceneEditor
    {
        private static void configureMainCamera(Scene scene, Bounds environmentBounds, CorridorAxes corridorAxes, SourceSceneCameraLayout sourceSceneCameraLayout)
        {
            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                mainCamera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            }

            GameObject cameraObject = mainCamera != null
                ? mainCamera.gameObject
                : new GameObject("Main Camera");

            if (mainCamera == null)
            {
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent<AudioListener>();
                SceneManager.MoveGameObjectToScene(cameraObject, scene);
            }

            cameraObject.name = "Main Camera";
            cameraObject.tag = "MainCamera";
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.nearClipPlane = 0.01f;
            mainCamera.farClipPlane = 200.0f;

            if (sourceSceneCameraLayout.IsValid)
            {
                mainCamera.backgroundColor = sourceSceneCameraLayout.BackgroundColor;
                mainCamera.fieldOfView = sourceSceneCameraLayout.FieldOfView;
                cameraObject.transform.position = sourceSceneCameraLayout.Position;
                cameraObject.transform.rotation = sourceSceneCameraLayout.Rotation;
                return;
            }

            mainCamera.backgroundColor = new Color(0.04f, 0.03f, 0.03f, 1.0f);
            mainCamera.fieldOfView = 42.0f;

            Vector3 stageLookTarget = calculateStageBasePosition(environmentBounds, corridorAxes)
                + Vector3.up * 2.2f;
            Vector3 cameraPosition = stageLookTarget
                - (corridorAxes.Forward * CAMERA_DEPTH_OFFSET)
                + (Vector3.up * CAMERA_HEIGHT_OFFSET);

            cameraObject.transform.position = cameraPosition;
            cameraObject.transform.LookAt(stageLookTarget);
        }

        private static CorridorAxes determineCorridorAxes(Bounds environmentBounds, Vector3 sourceSceneForward)
        {
            if (sourceSceneForward.sqrMagnitude > 0.0001f)
            {
                Vector3 sourceSceneLateral = Vector3.Cross(Vector3.up, sourceSceneForward).normalized;
                return new CorridorAxes(sourceSceneForward, sourceSceneLateral);
            }

            bool isForwardAlongZ = environmentBounds.size.z >= environmentBounds.size.x;
            Vector3 forward = isForwardAlongZ ? Vector3.forward : Vector3.right;
            Vector3 lateral = isForwardAlongZ ? Vector3.right : Vector3.forward;
            return new CorridorAxes(forward, lateral);
        }

        private static Vector3 calculateStageBasePosition(Bounds environmentBounds, CorridorAxes corridorAxes)
        {
            float stageForwardOffset = corridorAxes.GetExtent(environmentBounds) - STAGE_FORWARD_MARGIN;
            Vector3 stagePosition = environmentBounds.center + (corridorAxes.Forward * stageForwardOffset);
            stagePosition.y = environmentBounds.min.y;
            return stagePosition;
        }

        private enum EObjectLongAxis
        {
            XAxis,
            ZAxis,
        }

        private static void alignLongAxisToForward(Transform targetTransform, Vector3 forwardAxis, EObjectLongAxis objectLongAxis)
        {
            Vector3 projectedForward = Vector3.ProjectOnPlane(forwardAxis, Vector3.up).normalized;

            if (projectedForward.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 sourceLongAxis = objectLongAxis == EObjectLongAxis.XAxis ? Vector3.right : Vector3.forward;
            targetTransform.rotation = Quaternion.FromToRotation(sourceLongAxis, projectedForward);
        }

        private readonly struct CorridorAxes
        {
            public CorridorAxes(Vector3 forward, Vector3 lateral)
            {
                Forward = forward;
                Lateral = lateral;
            }

            public Vector3 Forward { get; }

            public Vector3 Lateral { get; }

            public float GetExtent(Bounds bounds)
            {
                Vector3 normalizedForward = Forward.normalized;
                float xExtent = Mathf.Abs(normalizedForward.x) * bounds.extents.x;
                float yExtent = Mathf.Abs(normalizedForward.y) * bounds.extents.y;
                float zExtent = Mathf.Abs(normalizedForward.z) * bounds.extents.z;

                return xExtent + yExtent + zExtent;
            }
        }

        private readonly struct SourceSceneCameraLayout
        {
            public static SourceSceneCameraLayout Invalid =>
                new SourceSceneCameraLayout(Vector3.zero, Quaternion.identity, 0.0f, Color.black);

            public SourceSceneCameraLayout(Vector3 position, Quaternion rotation, float fieldOfView, Color backgroundColor)
            {
                Position = position;
                Rotation = rotation;
                FieldOfView = fieldOfView;
                BackgroundColor = backgroundColor;
            }

            public Vector3 Position { get; }

            public Quaternion Rotation { get; }

            public float FieldOfView { get; }

            public Color BackgroundColor { get; }

            public bool IsValid => FieldOfView > 0.0f;
        }
    }
}
