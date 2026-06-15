using System;
using System.Collections.Generic;
using System.Linq;
using MouthOfTruth.Game.App;
using MouthOfTruth.Game.Presentation.Runtime;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace MouthOfTruth.Editor
{
    public static partial class BuildMainSceneEditor
    {
        private const string MAIN_SCENE_PATH = "Assets/Scenes/Main.unity";
        private const string DUNGEON_DEMO_SCENE_PATH = "Assets/ThirdParty/Environment/DungeonModularPack/Scenes/DemoScene.unity";
        private const string DUNGEON_WALL_MATERIAL_PATH = "Assets/ThirdParty/Environment/DungeonModularPack/Materials/M_Wall.mat";
        private const string GENERATED_MATERIAL_DIRECTORY_PATH = "Assets/Materials/GeneratedEnvironment";
        private const string TORCH_PREFAB_PATH = "Assets/ThirdParty/Environment/DungeonModularPack/Prefabs/Torch_B.prefab";
        private const string ARCH_PREFAB_PATH = "Assets/ThirdParty/Environment/DungeonModularPack/Prefabs/Arch_A.prefab";
        private const string RED_RUNNER_TEXTURE_PATH = "Assets/StreamingAssets/art/environment/floor_red_carpet_runner.png";
        private const string RED_RUNNER_MATERIAL_PATH = "Assets/Materials/GeneratedEnvironment/M_FloorRedRunner.mat";
        private static readonly string[] THIRD_PARTY_MODEL_DIRECTORIES =
        {
            "Assets/ThirdParty/Environment/DungeonModularPack/Meshes",
            "Assets/ThirdParty/Environment/PersianCarpetUrp/Models",
        };
        private static readonly string[] REQUIRED_THIRD_PARTY_ASSET_PATHS =
        {
            DUNGEON_DEMO_SCENE_PATH,
            DUNGEON_WALL_MATERIAL_PATH,
            TORCH_PREFAB_PATH,
            ARCH_PREFAB_PATH,
            "Assets/ThirdParty/Environment/DungeonModularPack/Meshes",
            "Assets/ThirdParty/Environment/PersianCarpetUrp/Models",
        };

        private const float CARD_ANCHOR_SPACING = 3.3f;
        private const float CARD_DEPTH_OFFSET = 7.2f;
        private const float CAMERA_DEPTH_OFFSET = 17.5f;
        private const float CAMERA_HEIGHT_OFFSET = 4.4f;
        private const float STAGE_FORWARD_MARGIN = 4.2f;

        [MenuItem("Mouth Of Truth/Build Main Scene")]
        public static void Run()
        {
            ConfigureUniversalRenderPipelineEditor.Run();
            validateRequiredThirdPartyAssets();
            normalizeThirdPartyModelImports();
            Scene sourceScene = EditorSceneManager.OpenScene(DUNGEON_DEMO_SCENE_PATH, OpenSceneMode.Single);
            Transform sourceEnvironmentRoot = findRequiredRoot(sourceScene, "Models");
            Bounds sourceEnvironmentBounds = calculateCombinedBounds(sourceEnvironmentRoot);
            Camera sourceSceneCamera = findSourceSceneCameraOrNull(sourceScene);
            SourceSceneCameraLayout sourceSceneCameraLayout = captureSourceSceneCameraLayout(sourceSceneCamera);
            Vector3 sourceSceneForward = getProjectedHorizontalForward(sourceSceneCamera, sourceEnvironmentBounds.center);
            List<GameObject> sourceSceneRootClones = cloneSourceSceneRoots(sourceScene);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);

            foreach (GameObject sourceSceneRootClone in sourceSceneRootClones)
            {
                SceneManager.MoveGameObjectToScene(sourceSceneRootClone, scene);
            }

            EditorSceneManager.CloseScene(sourceScene, true);

            unpackScenePrefabInstances(scene);

            Transform environmentRoot = findRequiredRoot(scene, "Models");
            Bounds environmentBounds = calculateCombinedBounds(environmentRoot);
            CorridorAxes corridorAxes = determineCorridorAxes(environmentBounds, sourceSceneForward);

            configureMainCamera(scene, environmentBounds, corridorAxes, sourceSceneCameraLayout);
            ensureEventSystem(scene);
            ensureApplicationRoot(scene);
            buildPresentationStage(scene, environmentBounds, corridorAxes);
            configureEnvironmentLighting(scene);
            unpackScenePrefabInstances(scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MAIN_SCENE_PATH);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MAIN_SCENE_PATH, true),
            };

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void validateRequiredThirdPartyAssets()
        {
            foreach (string requiredAssetPath in REQUIRED_THIRD_PARTY_ASSET_PATHS)
            {
                if (AssetDatabase.IsValidFolder(requiredAssetPath) || AssetDatabase.LoadMainAssetAtPath(requiredAssetPath) != null)
                {
                    continue;
                }

                throw new BuildFailedException(
                    $"Required Unity Asset Store asset is missing: {requiredAssetPath}\n"
                    + "Restore the third-party environment assets by following THIRD_PARTY_ASSETS.md.");
            }
        }

        private static void normalizeThirdPartyModelImports()
        {
            foreach (string modelDirectoryPath in THIRD_PARTY_MODEL_DIRECTORIES)
            {
                string[] modelGuids = AssetDatabase.FindAssets("t:Model", new[] { modelDirectoryPath });

                foreach (string modelGuid in modelGuids)
                {
                    string modelAssetPath = AssetDatabase.GUIDToAssetPath(modelGuid);
                    ModelImporter modelImporter = AssetImporter.GetAtPath(modelAssetPath) as ModelImporter;

                    if (modelImporter == null)
                    {
                        continue;
                    }

                    if (modelImporter.materialImportMode == ModelImporterMaterialImportMode.None)
                    {
                        continue;
                    }

                    modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                    modelImporter.SaveAndReimport();
                }
            }
        }

        private static Camera findSourceSceneCameraOrNull(Scene sourceScene)
        {
            foreach (GameObject rootGameObject in sourceScene.GetRootGameObjects())
            {
                Camera rootCamera = rootGameObject.GetComponent<Camera>();

                if (rootCamera != null)
                {
                    return rootCamera;
                }

                Camera childCamera = rootGameObject.GetComponentInChildren<Camera>(true);

                if (childCamera != null)
                {
                    return childCamera;
                }
            }

            return null;
        }

        private static List<GameObject> cloneSourceSceneRoots(Scene sourceScene)
        {
            List<GameObject> rootClones = new List<GameObject>();

            foreach (GameObject rootGameObject in sourceScene.GetRootGameObjects())
            {
                if (rootGameObject.name == "Main Camera")
                {
                    continue;
                }

                GameObject rootClone = UnityEngine.Object.Instantiate(rootGameObject);
                rootClone.name = rootGameObject.name;
                rootClones.Add(rootClone);
            }

            return rootClones;
        }

        private static Vector3 getProjectedHorizontalForward(Camera sourceSceneCamera, Vector3 environmentCenter)
        {
            if (sourceSceneCamera == null)
            {
                return Vector3.zero;
            }

            Vector3 projectedCameraForward = Vector3.ProjectOnPlane(sourceSceneCamera.transform.forward, Vector3.up);

            if (projectedCameraForward.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            Vector3 normalizedForward = projectedCameraForward.normalized;
            Vector3 directionToEnvironmentCenter = Vector3.ProjectOnPlane(environmentCenter - sourceSceneCamera.transform.position, Vector3.up);

            if (directionToEnvironmentCenter.sqrMagnitude > 0.0001f && Vector3.Dot(normalizedForward, directionToEnvironmentCenter.normalized) < 0.0f)
            {
                normalizedForward = -normalizedForward;
            }

            return normalizedForward;
        }

        private static SourceSceneCameraLayout captureSourceSceneCameraLayout(Camera sourceSceneCamera)
        {
            if (sourceSceneCamera == null)
            {
                return SourceSceneCameraLayout.Invalid;
            }

            return new SourceSceneCameraLayout(sourceSceneCamera.transform.position, sourceSceneCamera.transform.rotation, sourceSceneCamera.fieldOfView, sourceSceneCamera.backgroundColor);
        }

        private static void unpackScenePrefabInstances(Scene scene)
        {
            HashSet<GameObject> outermostPrefabRoots = new HashSet<GameObject>();

            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                foreach (Transform childTransform in rootGameObject.GetComponentsInChildren<Transform>(true))
                {
                    GameObject candidate = childTransform.gameObject;

                    if (PrefabUtility.IsPartOfPrefabInstance(candidate) == false)
                    {
                        continue;
                    }

                    GameObject outermostRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(candidate);

                    if (outermostRoot != null)
                    {
                        outermostPrefabRoots.Add(outermostRoot);
                    }
                }
            }

            foreach (GameObject outermostPrefabRoot in outermostPrefabRoots)
            {
                PrefabUtility.UnpackPrefabInstance(outermostPrefabRoot, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }

        private static void ensureApplicationRoot(Scene scene)
        {
            GameObject appObject = getOrCreateRootObject(scene, "MouthOfTruthApp");
            ensureComponent<MouthOfTruthGameView>(appObject);
            ensureComponent<MouthOfTruthAppController>(appObject);
        }

        private static void ensureEventSystem(Scene scene)
        {
            EventSystem existingEventSystem = UnityEngine.Object.FindAnyObjectByType<EventSystem>();

            if (existingEventSystem != null)
            {
                ensureComponent<StandaloneInputModule>(existingEventSystem.gameObject);
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            SceneManager.MoveGameObjectToScene(eventSystemObject, scene);
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

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

        private static void alignLongAxisToForward(Transform targetTransform, Vector3 forwardAxis, bool isLongAxisX)
        {
            Vector3 projectedForward = Vector3.ProjectOnPlane(forwardAxis, Vector3.up).normalized;

            if (projectedForward.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 sourceLongAxis = isLongAxisX ? Vector3.right : Vector3.forward;
            targetTransform.rotation = Quaternion.FromToRotation(sourceLongAxis, projectedForward);
        }

        private static Transform findRequiredRoot(Scene scene, string rootObjectName)
        {
            GameObject rootObject = scene.GetRootGameObjects()
                .FirstOrDefault(candidate => candidate.name == rootObjectName);

            if (rootObject == null)
            {
                throw new InvalidOperationException(
                    $"씬 루트 '{rootObjectName}' 을(를) 찾을 수 없습니다.");
            }

            return rootObject.transform;
        }

        private static GameObject getOrCreateRootObject(Scene scene, string objectName)
        {
            GameObject existingObject = scene.GetRootGameObjects()
                .FirstOrDefault(candidate => candidate.name == objectName);

            if (existingObject != null)
            {
                return existingObject;
            }

            GameObject createdObject = new GameObject(objectName);
            SceneManager.MoveGameObjectToScene(createdObject, scene);
            return createdObject;
        }

        private static Transform createChild(Transform parentTransform, string objectName)
        {
            Transform existingChild = parentTransform.Find(objectName);

            if (existingChild != null)
            {
                return existingChild;
            }

            GameObject childObject = new GameObject(objectName);
            childObject.transform.SetParent(parentTransform, false);
            return childObject.transform;
        }

        private static GameObject instantiatePrefab(GameObject prefab, Transform parentTransform, string objectName)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, parentTransform) as GameObject;

            if (instance == null)
            {
                throw new InvalidOperationException($"프리팹 생성에 실패했습니다: {prefab.name}");
            }

            instance.name = objectName;
            return instance;
        }

        private static T ensureComponent<T>(GameObject gameObject)
            where T : Component
        {
            T existingComponent = gameObject.GetComponent<T>();
            return existingComponent != null ? existingComponent : gameObject.AddComponent<T>();
        }

        private static void destroyChildren(Transform parentTransform)
        {
            for (int childIndex = parentTransform.childCount - 1; childIndex >= 0; childIndex--)
            {
                UnityEngine.Object.DestroyImmediate(parentTransform.GetChild(childIndex).gameObject);
            }
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

                return (Mathf.Abs(normalizedForward.x) * bounds.extents.x) + (Mathf.Abs(normalizedForward.y) * bounds.extents.y) + (Mathf.Abs(normalizedForward.z) * bounds.extents.z);
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
